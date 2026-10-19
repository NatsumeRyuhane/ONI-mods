using System.Collections.Generic;
using System.Text;
using KSerialization;
using PeterHan.PLib.Options;
using UnityEngine;

namespace LinkedStorage
{
    // A linked building's own side of the shared store. Its real Storage is only a buffer: whatever Duplicants or
    // pipes put in goes straight into the LinkedPool, and the buffer keeps a small stock of what the building gives
    // out, so Duplicants, pipes and fetch errands all work on real items with the game's own code.
    //
    // Each building is its own interface to the store: its filter and its Store and Take switches are its own.
    // - Store: Duplicants bring it what its filter ticks: items, food, bottles or canisters. A reservoir's input
    //   pipe always stores.
    // - Take: the buffer's stock may be taken by Duplicants. A bin or refrigerator only keeps a stock while Take is
    //   on. A reservoir always stocks what its filter ticks, for its output pipe.
    // Every linked building of a kind shares one storageNetworkID, which FetchManager reads to never fetch from one
    // into another, so Duplicants don't carry items around between linked buildings.
    [SerializationConfig(MemberSerialization.OptIn)]
    public class LinkedStorage : KMonoBehaviour, ISim200ms
    {
        // Above any conveyor network ID, the only other use of storageNetworkID.
        private const int NetworkIdBase = 0x4C4B5300;

        // What a store errand asks for at a time, like the vanilla Storage Bin's capacity. It is renewed when met.
        private const float FetchAmountKg = 20000f;

        // How often bins and refrigerators sync. Reservoirs sync every 200 ms to keep up with their pipes.
        private const float SolidSyncSeconds = 1f;

        private static readonly EventSystem.IntraObjectHandler<LinkedStorage> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<LinkedStorage>((component, data) => component.OnCopySettings(data));

        private static readonly EventSystem.IntraObjectHandler<LinkedStorage> OnRefreshUserMenuDelegate =
            new EventSystem.IntraObjectHandler<LinkedStorage>((component, data) => component.OnRefreshUserMenu());

        // Every spawned linked building, so the shared contents can count what the buffers hold.
        public static readonly List<LinkedStorage> Instances = new List<LinkedStorage>();

        // Gauge and logic meter symbols in the vanilla storage bin, refrigerator and reservoir art.
        private static readonly string[] HiddenSymbols =
        {
            "meter_target",
            "meter_frame",
            "meter_level",
            "meter_fill",
            "meter_OL",
            "logicmeter_target"
        };

        private static StatusItem contentsStatusItem;

        private static StatusItem storingStatusItem;

        private static StatusItem takingStatusItem;

        [SerializeField]
        public LinkKind kind;

        [SerializeField]
        public Tag[] forbiddenTags;

        [Serialize]
        [SerializeField]
        public bool allowStore = true;

        [Serialize]
        [SerializeField]
        public bool allowTake = true;

        // The game assigns [MyCmpGet] fields by reflection, which the compiler can't see.
#pragma warning disable CS0649
        [MyCmpGet]
        private Storage storage;

        [MyCmpGet]
        private TreeFilterable treeFilterable;

        [MyCmpGet]
        private ConduitDispenser dispenser;

        [MyCmpGet]
        private KSelectable selectable;

        [MyCmpGet]
        private KBatchedAnimController animController;
#pragma warning restore CS0649

        private readonly HashSet<Tag> stockTags = new HashSet<Tag>();

        private readonly Dictionary<Tag, float> stockedMass = new Dictionary<Tag, float>();

        private int world;

        private int seenPoolVersion = -1;

        private FetchList2 fetchList;


        private float sinceSync;

        // Set on spawn and when Take changes: the buffer goes back to the store once, so its items are stored again
        // with the right Duplicant access.
        private bool flushPending = true;

        public int World
        {
            get { return world; }
        }

        public bool IsFluid
        {
            get { return kind == LinkKind.Liquid || kind == LinkKind.Gas; }
        }

        public static int NetworkIdFor(LinkKind kind)
        {
            return NetworkIdBase + (int)kind;
        }

        public static float StockKgFor(LinkKind kind)
        {
            Config config = SingletonOptions<Config>.Instance;
            switch (kind)
            {
                case LinkKind.Food:
                    return config.FoodStockKg;
                case LinkKind.Liquid:
                    return config.LiquidStockKg;
                case LinkKind.Gas:
                    return config.GasStockKg;
                default:
                    return config.SolidStockKg;
            }
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
            Subscribe((int)GameHashes.RefreshUserMenu, OnRefreshUserMenuDelegate);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            world = LinkedPool.WorldKeyFor(gameObject);
            storage.allowItemRemoval = allowTake;
            storage.storageNetworkID = NetworkIdFor(kind);
            if (treeFilterable != null)
            {
                treeFilterable.OnFilterChanged += OnTreeFilterChanged;
            }
            UpdateDispenserFilter();
            // A fill gauge means nothing for contents without a limit. The vanilla art draws its gauge and logic
            // meter placeholders until a MeterController hides them, so they are hidden here instead.
            foreach (string symbol in HiddenSymbols)
            {
                animController.SetSymbolVisiblity(symbol, false);
            }
            Instances.Add(this);
            CreateStatusItems();
            selectable.AddStatusItem(contentsStatusItem, this);
            selectable.AddStatusItem(storingStatusItem, this);
            selectable.AddStatusItem(takingStatusItem, this);
            RefreshFetch();
        }

        protected override void OnCleanUp()
        {
            if (treeFilterable != null)
            {
                treeFilterable.OnFilterChanged -= OnTreeFilterChanged;
            }
            CancelFetch();
            Instances.Remove(this);
            base.OnCleanUp();
        }

        public void Sim200ms(float dt)
        {
            if (!IsFluid)
            {
                sinceSync += dt;
                if (sinceSync < SolidSyncSeconds)
                {
                    return;
                }
                sinceSync = 0f;
            }
            Sync();
        }

        // Puts everything in the buffer back into the store. Called before the building is deconstructed, so its
        // contents stay linked instead of dropping on the floor.
        public void ReturnAll()
        {
            LinkedPool pool = LinkedPool.Instance;
            if (pool == null)
            {
                return;
            }
            for (int i = storage.items.Count - 1; i >= 0; i--)
            {
                GameObject item = storage.items[i];
                if (item != null && CanLink(item))
                {
                    Deposit(pool, item);
                }
            }
        }

        public void SetAllowStore(bool allow)
        {
            allowStore = allow;
            RefreshFetch();
        }

        public void SetAllowTake(bool allow)
        {
            allowTake = allow;
            storage.allowItemRemoval = allow;
            flushPending = true;
            Sync();
        }

        private void Sync()
        {
            LinkedPool pool = LinkedPool.Instance;
            if (pool == null || storage == null)
            {
                return;
            }
            if (flushPending)
            {
                flushPending = false;
                ReturnAll();
            }
            CollectStockTags();
            float stockKg = StockKgFor(kind);
            stockedMass.Clear();
            // Anything the building doesn't keep a stock of goes into the store, as does stock over the limit.
            for (int i = storage.items.Count - 1; i >= 0; i--)
            {
                GameObject item = storage.items[i];
                if (item == null || !CanLink(item))
                {
                    continue;
                }
                Pickupable pickupable = item.GetComponent<Pickupable>();
                Tag tag = item.PrefabID();
                float mass = pickupable.PrimaryElement.Mass;
                float stocked;
                stockedMass.TryGetValue(tag, out stocked);
                // An item a Duplicant has reserved stays where it is, so the errand isn't broken.
                if (pickupable.ReservedAmount > 0f)
                {
                    stockedMass[tag] = stocked + mass;
                    continue;
                }
                if (!stockTags.Contains(tag))
                {
                    Deposit(pool, item);
                    continue;
                }
                float excess = stocked + mass - stockKg;
                if (Assets.IsTagCountable(tag))
                {
                    float perUnit = pickupable.PrimaryElement.MassPerUnit;
                    excess = Mathf.Floor(excess / perUnit) * perUnit;
                }
                if (excess > 0f && excess >= stockKg * 0.25f)
                {
                    if (excess >= mass)
                    {
                        Deposit(pool, item);
                        mass = 0f;
                    }
                    else
                    {
                        // Splitting leaves the rest in the buffer and returns the split-off part outside it. An item
                        // that can't split comes out of the buffer whole.
                        Pickupable taken = pickupable.Take(excess);
                        if (taken != null)
                        {
                            pool.Deposit(kind, world, taken.gameObject);
                            Util.KDestroyGameObject(taken.gameObject);
                        }
                        mass = taken == pickupable ? 0f : pickupable.PrimaryElement.Mass;
                    }
                }
                stockedMass[tag] = stocked + mass;
            }
            // Top the stock back up from the store.
            Vector3 position = transform.GetPosition();
            foreach (Tag tag in stockTags)
            {
                float stocked;
                stockedMass.TryGetValue(tag, out stocked);
                float wanted = stockKg - stocked;
                if (wanted <= 0f || (!IsFluid && wanted < stockKg * 0.25f))
                {
                    continue;
                }
                GameObject go = pool.Withdraw(kind, world, tag, wanted, position);
                if (go != null)
                {
                    storage.Store(go, true, false, false);
                }
            }
            // The Contents section only refreshes on storage events, and other buildings change the contents too.
            if (pool.Version != seenPoolVersion)
            {
                seenPoolVersion = pool.Version;
                Trigger((int)GameHashes.UpdateStorageInfo);
            }
        }

        private void Deposit(LinkedPool pool, GameObject item)
        {
            storage.Remove(item, false);
            pool.Deposit(kind, world, item);
            Util.KDestroyGameObject(item);
        }

        private void CollectStockTags()
        {
            stockTags.Clear();
            // A reservoir always stocks what its filter ticks, for its output pipe.
            if (treeFilterable != null && (allowTake || IsFluid))
            {
                stockTags.UnionWith(treeFilterable.GetTags());
            }
        }

        // Only plain stackable items go into the store. Their mass, heat, germs and freshness are all the store
        // keeps, so items with more state of their own stay in the buffer and drop when the building goes.
        private bool CanLink(GameObject item)
        {
            KPrefabID prefabId = item.GetComponent<KPrefabID>();
            PrimaryElement element = item.GetComponent<PrimaryElement>();
            if (prefabId == null || element == null || item.GetComponent<Pickupable>() == null
                || item.GetComponent<Equippable>() != null || item.GetComponent<MinionIdentity>() != null
                || item.GetComponent<CreatureBrain>() != null || prefabId.HasAnyTags(LinkedBuildings.UnlinkableTags))
            {
                return false;
            }
            switch (kind)
            {
                case LinkKind.Liquid:
                    return element.Element.IsLiquid && item.GetComponent<ElementChunk>() != null;
                case LinkKind.Gas:
                    return element.Element.IsGas && item.GetComponent<ElementChunk>() != null;
                case LinkKind.Solid:
                    return !prefabId.HasTag(GameTags.Edible) && item.GetSMI<Rottable.Instance>() == null;
                default:
                    return true;
            }
        }

        private void OnTreeFilterChanged(HashSet<Tag> tags)
        {
            UpdateDispenserFilter();
            RefreshFetch();
        }

        // A reservoir's output pipe sends out the elements its filter ticks, taking turns when there are several.
        // What comes in through the input pipe never passes straight through: anything else in the buffer is never
        // dispensed, and goes into the store at the next sync.
        private void UpdateDispenserFilter()
        {
            if (dispenser == null)
            {
                return;
            }
            List<SimHashes> elements = new List<SimHashes>();
            if (treeFilterable != null)
            {
                foreach (Tag tag in treeFilterable.GetTags())
                {
                    Element element = ElementLoader.GetElement(tag);
                    if (element != null)
                    {
                        elements.Add(element.id);
                    }
                }
            }
            if (elements.Count == 0)
            {
                elements.Add(SimHashes.Vacuum);
            }
            dispenser.elementFilter = elements.ToArray();
        }

        private void OnFetchComplete()
        {
            fetchList = null;
            RefreshFetch();
        }

        private void CancelFetch()
        {
            if (fetchList != null)
            {
                fetchList.Cancel("");
                fetchList = null;
            }
        }

        private void RefreshFetch()
        {
            CancelFetch();
            if (!allowStore || !isSpawned)
            {
                return;
            }
            ChoreType choreType = kind == LinkKind.Food ? Db.Get().ChoreTypes.FoodFetch : Db.Get().ChoreTypes.StorageFetch;
            // Every kind fetches what its filter ticks, so a reservoir only gets the bottles or canisters picked for
            // that building, like a Bottle Emptier.
            HashSet<Tag> tags = treeFilterable != null ? treeFilterable.GetTags() : null;
            if (tags == null || tags.Count == 0)
            {
                return;
            }
            FetchList2 list = new FetchList2(storage, choreType);
            list.ShowStatusItem = false;
            list.Add(new HashSet<Tag>(tags), forbiddenTags, FetchAmountKg);
            fetchList = list;
            fetchList.Submit(OnFetchComplete, false);
        }

        private void OnCopySettings(object data)
        {
            GameObject source = data as GameObject;
            LinkedStorage other = source != null ? source.GetComponent<LinkedStorage>() : null;
            if (other == null || other.kind != kind)
            {
                return;
            }
            if (other.allowStore != allowStore)
            {
                SetAllowStore(other.allowStore);
            }
            if (other.allowTake != allowTake)
            {
                SetAllowTake(other.allowTake);
            }
        }

        private void OnRefreshUserMenu()
        {
            string storeText = allowStore ? LinkedBuildings.DisableStoreName : LinkedBuildings.EnableStoreName;
            Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo("action_switch_toggle",
                storeText, () =>
                {
                    SetAllowStore(!allowStore);
                    Game.Instance.userMenu.Refresh(gameObject);
                }, global::Action.NumActions, null, null, null, IsFluid
                    ? LinkedBuildings.StoreFluidTooltip
                    : LinkedBuildings.StoreItemTooltip));
            string takeText = allowTake ? LinkedBuildings.DisableTakeName : LinkedBuildings.EnableTakeName;
            Game.Instance.userMenu.AddButton(gameObject, new KIconButtonMenu.ButtonInfo("action_switch_toggle",
                takeText, () =>
                {
                    SetAllowTake(!allowTake);
                    Game.Instance.userMenu.Refresh(gameObject);
                }, global::Action.NumActions, null, null, null, IsFluid
                    ? LinkedBuildings.TakeFluidTooltip
                    : LinkedBuildings.TakeItemTooltip));
        }

        // Adds the mass of each item in this building's buffer to masses.
        public void AddStockMasses(Dictionary<Tag, double> masses)
        {
            foreach (GameObject item in storage.items)
            {
                PrimaryElement element = item != null ? item.GetComponent<PrimaryElement>() : null;
                if (element != null)
                {
                    double known;
                    masses.TryGetValue(item.PrefabID(), out known);
                    masses[item.PrefabID()] = known + element.Mass;
                }
            }
        }

        // Lists the shared contents in the vanilla Contents section, in place of the buffer the section would
        // otherwise show: every item, biggest first, laid out like the vanilla rows, with the same status icons
        // (deep frozen, fresh or stale, germs) and the same legend in the tooltip. Clicking a row selects the stock
        // this building keeps of it, if any.
        public void FillContentsPanel(CollapsibleDetailContentPanel panel)
        {
            panel.gameObject.SetActive(true);
            panel.SetTitle(STRINGS.UI.DETAILTABS.DETAILS.GROUPNAME_CONTENTS);
            Dictionary<Tag, double> stocked = new Dictionary<Tag, double>();
            AddStockMasses(stocked);
            List<KeyValuePair<Tag, ContentRow>> rows = new List<KeyValuePair<Tag, ContentRow>>(CollectContents());
            rows.Sort((a, b) => b.Value.Mass.CompareTo(a.Value.Mass));
            int shown = 0;
            foreach (KeyValuePair<Tag, ContentRow> pair in rows)
            {
                ContentRow row = pair.Value;
                if (row.Mass <= 0.0)
                {
                    continue;
                }
                Tag tag = pair.Key;
                string name = STRINGS.UI.StripLinkFormatting(tag.ProperName());
                string amount = FormatMass(tag, (float)row.Mass);
                string temperature = GameUtil.GetFormattedTemperature((float)(row.Heat / row.Mass));
                List<string> icons = new List<string>();
                List<string> legend = new List<string>();
                AddStatusIcons(tag, row, icons, legend);
                double stockMass;
                stocked.TryGetValue(tag, out stockMass);
                string tooltip = name;
                if (legend.Count > 0)
                {
                    tooltip += "\n  " + string.Join("\n  ", legend);
                }
                tooltip += "\n\n" + string.Format(LinkedBuildings.ContentsRowTooltip, amount, temperature,
                    FormatMass(tag, (float)stockMass));
                KSelectable stock = row.Item != null ? row.Item.GetComponent<KSelectable>() : null;
                string label = " " + name + (icons.Count > 0 ? " " + string.Join(" ", icons) : "");
                panel.SetLabelWithButton("linked_" + tag.Name, label, temperature, amount, tooltip, () =>
                {
                    if (stock != null)
                    {
                        SelectTool.Instance.Select(stock);
                    }
                });
                shown++;
            }
            if (shown == 0)
            {
                panel.SetLabel("storage_empty", STRINGS.UI.DETAILTABS.DETAILS.STORAGE_EMPTY, "");
            }
            panel.Commit();
        }

        // The icons SimpleInfoScreen puts after a stored item's name, and their legend lines, in the same order.
        // Food in the linked contents is always deep frozen; its freshness is the mass-weighted average.
        private void AddStatusIcons(Tag tag, ContentRow row, List<string> icons, List<string> legend)
        {
            if (kind == LinkKind.Food && row.RotMass > 0.0)
            {
                icons.Add(STRINGS.UI.DETAILTABS.TEXTICONDATA.DEEPFROZEN.ICON);
                legend.Add(STRINGS.UI.DETAILTABS.TEXTICONDATA.DEEPFROZEN.ICON + " "
                    + STRINGS.UI.DETAILTABS.TEXTICONDATA.DEEPFROZEN.NAME);
            }
            GameObject prefab = Assets.TryGetPrefab(tag);
            Rottable.Def rot = prefab != null ? prefab.GetDef<Rottable.Def>() : null;
            if (rot != null && rot.spoilTime > 0f && row.RotMass > 0.0)
            {
                float rotValue = (float)(row.Rot / row.RotMass);
                bool fresh = rotValue > rot.staleTime;
                string icon = fresh ? STRINGS.UI.DETAILTABS.TEXTICONDATA.FRESH.ICON : STRINGS.UI.DETAILTABS.TEXTICONDATA.STALE.ICON;
                string state = fresh ? STRINGS.UI.DETAILTABS.TEXTICONDATA.FRESH.NAME : STRINGS.UI.DETAILTABS.TEXTICONDATA.STALE.NAME;
                icons.Add(icon);
                legend.Add(icon + " " + state + " (" + GameUtil.GetFormattedPercent(100f * rotValue / rot.spoilTime) + ")");
            }
            if (row.DiseaseIdx != byte.MaxValue && row.DiseaseCount > 0)
            {
                Klei.AI.Disease disease = Db.Get().Diseases[row.DiseaseIdx];
                icons.Add(STRINGS.UI.DETAILTABS.TEXTICONDATA.GERMS.ICON);
                legend.Add(STRINGS.UI.DETAILTABS.TEXTICONDATA.GERMS.ICON + " " + GameUtil.SafeStringFormat(
                    STRINGS.UI.OVERLAYS.DISEASE.DISEASE_FORMAT, disease.Name,
                    GameUtil.GetFormattedDiseaseAmount((int)System.Math.Min(int.MaxValue, row.DiseaseCount))));
            }
        }

        private class ContentRow
        {
            public double Mass;

            // Mass times temperature, to average the temperature by mass.
            public double Heat;

            // Mass times RotValue, and the mass it covers, to average freshness by mass.
            public double Rot;

            public double RotMass;

            // The dominant germ and how many there are of it.
            public byte DiseaseIdx = byte.MaxValue;

            public long DiseaseCount;

            // An item of this building's stock, for the row's select button.
            public GameObject Item;

            public void Add(double mass, float temperature, byte diseaseIdx, int diseaseCount, float? rot)
            {
                Mass += mass;
                Heat += mass * temperature;
                if (rot.HasValue)
                {
                    Rot += mass * rot.Value;
                    RotMass += mass;
                }
                if (diseaseIdx == byte.MaxValue || diseaseCount <= 0)
                {
                    return;
                }
                if (DiseaseIdx == byte.MaxValue || DiseaseIdx == diseaseIdx)
                {
                    DiseaseIdx = diseaseIdx;
                    DiseaseCount += diseaseCount;
                }
                else if (diseaseCount > DiseaseCount)
                {
                    DiseaseIdx = diseaseIdx;
                    DiseaseCount = diseaseCount;
                }
            }
        }

        // The whole shared contents of this building's kind and world: the store, plus the stock every linked
        // building of the kind keeps out of it.
        private Dictionary<Tag, ContentRow> CollectContents()
        {
            Dictionary<Tag, ContentRow> rows = new Dictionary<Tag, ContentRow>();
            LinkedPool pool = LinkedPool.Instance;
            Dictionary<Tag, LinkedPool.Entry> store = pool != null ? pool.GetStore(kind, world) : null;
            if (store != null)
            {
                foreach (KeyValuePair<Tag, LinkedPool.Entry> pair in store)
                {
                    LinkedPool.Entry entry = pair.Value;
                    GetRow(rows, pair.Key).Add(entry.Mass, entry.Temperature, entry.DiseaseIdx, entry.DiseaseCount,
                        kind == LinkKind.Food ? entry.Rot : (float?)null);
                }
            }
            foreach (LinkedStorage linked in Instances)
            {
                if (linked.kind != kind || linked.world != world)
                {
                    continue;
                }
                foreach (GameObject item in linked.storage.items)
                {
                    PrimaryElement element = item != null ? item.GetComponent<PrimaryElement>() : null;
                    if (element == null)
                    {
                        continue;
                    }
                    Rottable.Instance rottable = item.GetSMI<Rottable.Instance>();
                    ContentRow row = GetRow(rows, item.PrefabID());
                    row.Add(element.Mass, element.Temperature, element.DiseaseIdx, element.DiseaseCount,
                        rottable != null ? rottable.RotValue : (float?)null);
                    if (linked == this)
                    {
                        row.Item = item;
                    }
                }
            }
            return rows;
        }

        private static ContentRow GetRow(Dictionary<Tag, ContentRow> rows, Tag tag)
        {
            ContentRow row;
            if (!rows.TryGetValue(tag, out row))
            {
                row = new ContentRow();
                rows[tag] = row;
            }
            return row;
        }

        // Three status lines: the size of the shared contents, and the Storing and Taking switches. Each tooltip
        // explains its line.
        private static void CreateStatusItems()
        {
            if (contentsStatusItem != null)
            {
                return;
            }
            contentsStatusItem = NewStatusItem(LinkedBuildings.ContentsStatusId);
            contentsStatusItem.resolveStringCallback = (text, data) => ((LinkedStorage)data).ResolveContents(text);
            contentsStatusItem.resolveTooltipCallback = (text, data) => ((LinkedStorage)data).ResolveContents(text);
            storingStatusItem = NewStatusItem(LinkedBuildings.StoringStatusId);
            storingStatusItem.resolveStringCallback = (text, data) => ((LinkedStorage)data).ResolveStoring(text);
            storingStatusItem.resolveTooltipCallback = (text, data) => ((LinkedStorage)data).ResolveStoring(text);
            takingStatusItem = NewStatusItem(LinkedBuildings.TakingStatusId);
            takingStatusItem.resolveStringCallback = (text, data) => ((LinkedStorage)data).ResolveTaking(text);
            takingStatusItem.resolveTooltipCallback = (text, data) => ((LinkedStorage)data).ResolveTaking(text);
        }

        private static StatusItem NewStatusItem(string id)
        {
            return new StatusItem(id, "BUILDING", "", StatusItem.IconType.Info, NotificationType.Neutral, false,
                OverlayModes.None.ID);
        }

        private string ResolveContents(string text)
        {
            Dictionary<Tag, ContentRow> rows = CollectContents();
            double total = 0.0;
            foreach (ContentRow row in rows.Values)
            {
                total += row.Mass;
            }
            Building building = GetComponent<Building>();
            return text.Replace("{Mass}", GameUtil.GetFormattedMass((float)total))
                .Replace("{Count}", rows.Count.ToString())
                .Replace("{Building}", building != null ? building.Def.Name : gameObject.GetProperName())
                .Replace("{Scope}", world == LinkedPool.GlobalWorld ? LinkedBuildings.ScopeColony : LinkedBuildings.ScopeWorld)
                .Replace("{Extra}", kind == LinkKind.Food ? LinkedBuildings.FoodExtra : "");
        }

        private string ResolveStoring(string text)
        {
            string detail;
            switch (kind)
            {
                case LinkKind.Liquid:
                    detail = allowStore ? LinkedBuildings.StoringLiquidOn : LinkedBuildings.StoringLiquidOff;
                    break;
                case LinkKind.Gas:
                    detail = allowStore ? LinkedBuildings.StoringGasOn : LinkedBuildings.StoringGasOff;
                    break;
                default:
                    detail = allowStore ? LinkedBuildings.StoringItemOn : LinkedBuildings.StoringItemOff;
                    break;
            }
            return text.Replace("{State}", allowStore ? LinkedBuildings.Enabled : LinkedBuildings.Disabled)
                .Replace("{Detail}", detail);
        }

        private string ResolveTaking(string text)
        {
            string detail;
            switch (kind)
            {
                case LinkKind.Liquid:
                    detail = allowTake ? LinkedBuildings.TakingLiquidOn : LinkedBuildings.TakingFluidOff;
                    break;
                case LinkKind.Gas:
                    detail = allowTake ? LinkedBuildings.TakingGasOn : LinkedBuildings.TakingFluidOff;
                    break;
                default:
                    detail = allowTake
                        ? string.Format(LinkedBuildings.TakingItemOn, GameUtil.GetFormattedMass(StockKgFor(kind)))
                        : LinkedBuildings.TakingItemOff;
                    break;
            }
            return text.Replace("{State}", allowTake ? LinkedBuildings.Enabled : LinkedBuildings.Disabled)
                .Replace("{Detail}", detail);
        }

        // Like the vanilla Contents rows: units for countable items, mass for everything else.
        private static string FormatMass(Tag tag, float mass)
        {
            if (Assets.IsTagCountable(tag))
            {
                return GameUtil.GetFormattedUnits(mass / LinkedPool.MassPerUnit(tag));
            }
            return GameUtil.GetFormattedMass(mass);
        }
    }
}
