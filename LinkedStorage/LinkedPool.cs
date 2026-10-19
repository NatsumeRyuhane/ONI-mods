using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Klei;
using KSerialization;
using PeterHan.PLib.Options;
using UnityEngine;

namespace LinkedStorage
{
    // Which shared store a linked building uses. Each kind has its own store, so a Linked Storage Bin never sees
    // what a Linked Refrigerator holds.
    public enum LinkKind
    {
        Solid,
        Food,
        Liquid,
        Gas
    }

    // The shared contents of every linked building, kept on the SaveGame object so they are saved with the colony.
    // Contents are kept per kind and per world (or in one store per kind when Separate Contents per Planetoid is
    // off), as one entry per item ID: total mass, mass-weighted temperature and freshness, and the dominant germ. The
    // store is insulated and, for food, deep frozen and sterile, so nothing in it changes until it comes out.
    [SerializationConfig(MemberSerialization.OptIn)]
    public class LinkedPool : KMonoBehaviour
    {
        // The world key used for every building when Separate Contents per Planetoid is off.
        public const int GlobalWorld = -1;

        // Leftovers below this are dropped, so rounding doesn't leave entries of a few micrograms behind.
        private const double MinMassKg = 1e-4;

        public class Entry
        {
            public double Mass;

            public float Temperature;

            public byte DiseaseIdx = byte.MaxValue;

            public int DiseaseCount;

            // Rottable's RotValue, weighted by mass. Only food has one.
            public float Rot;
        }

        public static LinkedPool Instance { get; private set; }

        // Whether contents saved under the other Separate Contents per Planetoid setting have been moved yet. It
        // waits for the first use, when the save is loaded and the worlds exist.
        private bool worldsMerged;

        // Bumped on every change, so callers can tell when to refresh what they show.
        public int Version { get; private set; }

        // What the shared contents add to each world's inventory, by item ID and by every tag the item carries,
        // like WorldInventory's own counts. Rebuilt on demand after a change.
        private readonly Dictionary<int, Dictionary<Tag, float>> inventoryAmounts = new Dictionary<int, Dictionary<Tag, float>>();

        private int inventoryAmountsVersion = -1;

        private readonly Dictionary<long, Dictionary<Tag, Entry>> stores = new Dictionary<long, Dictionary<Tag, Entry>>();

        // The contents are saved as parallel lists of plain values, so the save holds no types of this mod beyond
        // the component itself.
        [Serialize]
        private List<int> savedKinds = new List<int>();

        [Serialize]
        private List<int> savedWorlds = new List<int>();

        [Serialize]
        private List<string> savedTags = new List<string>();

        [Serialize]
        private List<double> savedMasses = new List<double>();

        [Serialize]
        private List<float> savedTemperatures = new List<float>();

        [Serialize]
        private List<byte> savedDiseaseIdxs = new List<byte>();

        [Serialize]
        private List<int> savedDiseaseCounts = new List<int>();

        [Serialize]
        private List<float> savedRots = new List<float>();

        public static int WorldKeyFor(GameObject go)
        {
            return SingletonOptions<Config>.Instance.PerPlanetoid ? go.GetMyWorldId() : GlobalWorld;
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Instance = this;
        }

        protected override void OnCleanUp()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            base.OnCleanUp();
        }

        public Dictionary<Tag, Entry> GetStore(LinkKind kind, int world)
        {
            MergeWorldsForOption();
            Dictionary<Tag, Entry> store;
            stores.TryGetValue(Key(kind, world), out store);
            return store;
        }

        public double GetMass(LinkKind kind, int world, Tag tag)
        {
            Dictionary<Tag, Entry> store = GetStore(kind, world);
            Entry entry;
            return store != null && store.TryGetValue(tag, out entry) ? entry.Mass : 0.0;
        }

        public double GetTotalMass(LinkKind kind, int world)
        {
            double total = 0.0;
            Dictionary<Tag, Entry> store = GetStore(kind, world);
            if (store != null)
            {
                foreach (Entry entry in store.Values)
                {
                    total += entry.Mass;
                }
            }
            return total;
        }

        // Adds an item's mass, heat, germs and freshness to the store. The caller destroys the item.
        public void Deposit(LinkKind kind, int world, GameObject item)
        {
            MergeWorldsForOption();
            PrimaryElement element = item.GetComponent<PrimaryElement>();
            if (element == null || element.Mass <= 0f)
            {
                return;
            }
            Rottable.Instance rottable = item.GetSMI<Rottable.Instance>();
            Add(kind, world, item.PrefabID(), element.Mass, element.Temperature, element.DiseaseIdx,
                element.DiseaseCount, rottable != null ? rottable.RotValue : 0f);
        }

        // Takes up to maxMass of an item out of the store as a new item at the given position. Countable items, such
        // as seeds, come out in whole units. Returns null when the store has none.
        public GameObject Withdraw(LinkKind kind, int world, Tag tag, float maxMass, Vector3 position)
        {
            Dictionary<Tag, Entry> store = GetStore(kind, world);
            Entry entry;
            if (store == null || !store.TryGetValue(tag, out entry) || maxMass <= 0f)
            {
                return null;
            }
            GameObject prefab = Assets.TryGetPrefab(tag);
            PrimaryElement prefabElement = prefab != null ? prefab.GetComponent<PrimaryElement>() : null;
            if (prefabElement == null)
            {
                return null;
            }
            float mass = (float)Math.Min(entry.Mass, maxMass);
            float massPerUnit = prefabElement.MassPerUnit;
            if (Assets.IsTagCountable(tag))
            {
                mass = Mathf.Floor(mass / massPerUnit) * massPerUnit;
            }
            if (mass <= 0f)
            {
                return null;
            }
            int germs = entry.DiseaseIdx == byte.MaxValue ? 0 : (int)(entry.DiseaseCount * Math.Min(1.0, mass / entry.Mass));
            byte diseaseIdx = germs > 0 ? entry.DiseaseIdx : byte.MaxValue;
            GameObject go;
            Element element = ElementLoader.GetElement(tag);
            if (element != null && element.substance != null && prefab.GetComponent<ElementChunk>() != null)
            {
                go = element.substance.SpawnResource(position, mass, entry.Temperature, diseaseIdx, germs,
                    prevent_merge: true);
            }
            else
            {
                go = GameUtil.KInstantiate(prefab, position, Grid.SceneLayer.Ore);
                go.SetActive(true);
                PrimaryElement spawned = go.GetComponent<PrimaryElement>();
                spawned.Units = mass / massPerUnit;
                spawned.Temperature = entry.Temperature;
                if (germs > 0)
                {
                    spawned.AddDisease(diseaseIdx, germs, "LinkedStorage.Withdraw");
                }
                Rottable.Instance rottable = go.GetSMI<Rottable.Instance>();
                if (rottable != null)
                {
                    rottable.RotValue = entry.Rot;
                }
            }
            entry.Mass -= mass;
            entry.DiseaseCount -= germs;
            Version++;
            if (entry.Mass < MinMassKg)
            {
                store.Remove(tag);
            }
            return go;
        }

        private void Add(LinkKind kind, int world, Tag tag, double mass, float temperature, byte diseaseIdx,
            int diseaseCount, float rot)
        {
            long key = Key(kind, world);
            Dictionary<Tag, Entry> store;
            if (!stores.TryGetValue(key, out store))
            {
                store = new Dictionary<Tag, Entry>();
                stores[key] = store;
            }
            Entry entry;
            if (!store.TryGetValue(tag, out entry))
            {
                entry = new Entry();
                store[tag] = entry;
            }
            double total = entry.Mass + mass;
            entry.Temperature = (float)((entry.Mass * entry.Temperature + mass * temperature) / total);
            entry.Rot = (float)((entry.Mass * entry.Rot + mass * rot) / total);
            entry.Mass = total;
            MixDisease(entry, diseaseIdx, diseaseCount);
            Version++;
        }

        // How much the shared contents add to a world's inventory for a tag, in the inventory's units: kg, or units
        // for countable items and food. Stock a building keeps away from Duplicants is counted too, since the
        // inventory skips it.
        public float GetInventoryAmount(int worldId, Tag tag)
        {
            int key = SingletonOptions<Config>.Instance.PerPlanetoid ? worldId : GlobalWorld;
            if (inventoryAmountsVersion != Version)
            {
                inventoryAmounts.Clear();
                inventoryAmountsVersion = Version;
            }
            Dictionary<Tag, float> amounts;
            if (!inventoryAmounts.TryGetValue(key, out amounts))
            {
                amounts = BuildInventoryAmounts(key);
                inventoryAmounts[key] = amounts;
            }
            float amount;
            return amounts.TryGetValue(tag, out amount) ? amount : 0f;
        }

        // The calories of the food in the shared contents for a world, like RationTracker counts them: in total, or
        // for one food ID. Fills unitsById, when given, with the units of each food.
        public float GetCalories(int worldId, string foodId, Dictionary<string, float> unitsById, out float units)
        {
            units = 0f;
            int key = SingletonOptions<Config>.Instance.PerPlanetoid ? worldId : GlobalWorld;
            float calories = 0f;
            foreach (KeyValuePair<Tag, double> pair in CollectInventoryMasses(LinkKind.Food, key))
            {
                EdiblesManager.FoodInfo info = EdiblesManager.GetFoodInfo(pair.Key.Name);
                if (info == null || (foodId != null && info.Id != foodId))
                {
                    continue;
                }
                float foodUnits = (float)(pair.Value / MassPerUnit(pair.Key));
                calories += foodUnits * info.CaloriesPerUnit;
                units += foodUnits;
                if (unitsById != null)
                {
                    float known;
                    unitsById.TryGetValue(info.Id, out known);
                    unitsById[info.Id] = known + foodUnits;
                }
            }
            return calories;
        }

        private Dictionary<Tag, float> BuildInventoryAmounts(int key)
        {
            Dictionary<Tag, float> amounts = new Dictionary<Tag, float>();
            foreach (LinkKind kind in (LinkKind[])Enum.GetValues(typeof(LinkKind)))
            {
                foreach (KeyValuePair<Tag, double> pair in CollectInventoryMasses(kind, key))
                {
                    GameObject prefab = Assets.TryGetPrefab(pair.Key);
                    KPrefabID prefabId = prefab != null ? prefab.GetComponent<KPrefabID>() : null;
                    if (prefabId == null)
                    {
                        continue;
                    }
                    float units = (float)(pair.Value / MassPerUnit(pair.Key));
                    AddAmount(amounts, pair.Key, units);
                    foreach (Tag tag in prefabId.Tags)
                    {
                        if (tag != pair.Key)
                        {
                            AddAmount(amounts, tag, units);
                        }
                    }
                }
            }
            return amounts;
        }

        // The shared contents of a kind and world key, plus stock the world inventory doesn't count: what buildings
        // with Take off keep away from Duplicants.
        private Dictionary<Tag, double> CollectInventoryMasses(LinkKind kind, int key)
        {
            Dictionary<Tag, double> masses = new Dictionary<Tag, double>();
            Dictionary<Tag, Entry> store = GetStore(kind, key);
            if (store != null)
            {
                foreach (KeyValuePair<Tag, Entry> pair in store)
                {
                    masses[pair.Key] = pair.Value.Mass;
                }
            }
            foreach (LinkedStorage linked in LinkedStorage.Instances)
            {
                if (linked.kind != kind || linked.World != key || linked.allowTake)
                {
                    continue;
                }
                linked.AddStockMasses(masses);
            }
            return masses;
        }

        private static void AddAmount(Dictionary<Tag, float> amounts, Tag tag, float amount)
        {
            float known;
            amounts.TryGetValue(tag, out known);
            amounts[tag] = known + amount;
        }

        public static float MassPerUnit(Tag tag)
        {
            GameObject prefab = Assets.TryGetPrefab(tag);
            PrimaryElement element = prefab != null ? prefab.GetComponent<PrimaryElement>() : null;
            return element != null && element.MassPerUnit > 0f ? element.MassPerUnit : 1f;
        }

        // Like SimUtil.CalculateFinalDiseaseInfo, but the sum can't overflow: a store can grow without limit.
        private static void MixDisease(Entry entry, byte idx, int count)
        {
            if (idx == byte.MaxValue || count <= 0)
            {
                return;
            }
            if (entry.DiseaseIdx == idx || entry.DiseaseIdx == byte.MaxValue)
            {
                entry.DiseaseIdx = idx;
                entry.DiseaseCount = (int)Math.Min(int.MaxValue, (long)entry.DiseaseCount + count);
                return;
            }
            SimUtil.DiseaseInfo mixed = SimUtil.CalculateFinalDiseaseInfo(entry.DiseaseIdx, entry.DiseaseCount, idx, count);
            entry.DiseaseIdx = mixed.idx;
            entry.DiseaseCount = mixed.count;
        }

        // Moves contents saved under the other setting of Separate Contents per Planetoid, so changing the option
        // never hides anything. Per-world stores merge into one, and the one store goes to the starting world.
        private void MergeWorldsForOption()
        {
            if (worldsMerged || ClusterManager.Instance == null)
            {
                return;
            }
            worldsMerged = true;
            bool perPlanetoid = SingletonOptions<Config>.Instance.PerPlanetoid;
            int startWorld = ClusterManager.Instance.GetStartWorld().id;
            List<long> keys = new List<long>(stores.Keys);
            foreach (long key in keys)
            {
                int world = WorldOf(key);
                int target = perPlanetoid ? (world == GlobalWorld ? startWorld : world) : GlobalWorld;
                if (target == world)
                {
                    continue;
                }
                LinkKind kind = KindOf(key);
                foreach (KeyValuePair<Tag, Entry> pair in stores[key])
                {
                    Entry entry = pair.Value;
                    Add(kind, target, pair.Key, entry.Mass, entry.Temperature, entry.DiseaseIdx, entry.DiseaseCount,
                        entry.Rot);
                }
                stores.Remove(key);
            }
            Version++;
        }

        [OnSerializing]
        private void OnSerializing()
        {
            savedKinds.Clear();
            savedWorlds.Clear();
            savedTags.Clear();
            savedMasses.Clear();
            savedTemperatures.Clear();
            savedDiseaseIdxs.Clear();
            savedDiseaseCounts.Clear();
            savedRots.Clear();
            foreach (KeyValuePair<long, Dictionary<Tag, Entry>> store in stores)
            {
                foreach (KeyValuePair<Tag, Entry> pair in store.Value)
                {
                    Entry entry = pair.Value;
                    savedKinds.Add((int)KindOf(store.Key));
                    savedWorlds.Add(WorldOf(store.Key));
                    savedTags.Add(pair.Key.Name);
                    savedMasses.Add(entry.Mass);
                    savedTemperatures.Add(entry.Temperature);
                    savedDiseaseIdxs.Add(entry.DiseaseIdx);
                    savedDiseaseCounts.Add(entry.DiseaseCount);
                    savedRots.Add(entry.Rot);
                }
            }
        }

        [OnSerialized]
        private void OnSerialized()
        {
            savedKinds.Clear();
            savedWorlds.Clear();
            savedTags.Clear();
            savedMasses.Clear();
            savedTemperatures.Clear();
            savedDiseaseIdxs.Clear();
            savedDiseaseCounts.Clear();
            savedRots.Clear();
        }

        // Entries whose item no longer exists, for example after removing the mod that added it, are dropped.
        [OnDeserialized]
        private void OnDeserialized()
        {
            stores.Clear();
            worldsMerged = false;
            int count = savedTags.Count;
            for (int i = 0; i < count; i++)
            {
                Tag tag = new Tag(savedTags[i]);
                if (Assets.TryGetPrefab(tag) == null || i >= savedKinds.Count || i >= savedWorlds.Count
                    || i >= savedMasses.Count || i >= savedTemperatures.Count || i >= savedDiseaseIdxs.Count
                    || i >= savedDiseaseCounts.Count || i >= savedRots.Count)
                {
                    continue;
                }
                Add((LinkKind)savedKinds[i], savedWorlds[i], tag, savedMasses[i], savedTemperatures[i],
                    savedDiseaseIdxs[i], savedDiseaseCounts[i], savedRots[i]);
            }
            OnSerialized();
        }

        private static long Key(LinkKind kind, int world)
        {
            return ((long)kind << 32) | (uint)world;
        }

        private static LinkKind KindOf(long key)
        {
            return (LinkKind)(int)(key >> 32);
        }

        private static int WorldOf(long key)
        {
            return (int)(uint)(key & 0xFFFFFFFFL);
        }
    }
}
