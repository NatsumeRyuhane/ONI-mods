using System.Collections.Generic;
using System.Linq;
using PeterHan.PLib.Options;
using TUNING;
using UnityEngine;

namespace LinkedStorage
{
    // Shared setup and text for the four linked buildings. Each one takes its vanilla counterpart's size, art, cost
    // and build menu place, without power, logic ports or a capacity.
    public static class LinkedBuildings
    {
        public const string ContentsStatusId = "LinkedStorageContents";

        public const string StoringStatusId = "LinkedStorageStoring";

        public const string TakingStatusId = "LinkedStorageTaking";

        public const string DisableStoreName = "Stop Storing";

        public const string EnableStoreName = "Allow Storing";

        public const string DisableTakeName = "Stop Taking";

        public const string EnableTakeName = "Allow Taking";

        public const string StoreItemTooltip = "Toggle whether Duplicants bring this building what its filter accepts. Whatever they bring goes into the linked contents";

        public const string StoreFluidTooltip = "Toggle whether Duplicants bring this building bottles or canisters of the elements ticked in its filter. Its input pipe stores either way";

        public const string TakeItemTooltip = "Toggle whether this building keeps a stock of what its filter accepts for Duplicants to take";

        public const string TakeFluidTooltip = "Toggle whether Duplicants may take bottles or canisters of the elements ticked in its filter. Its output pipe works either way";

        public const string ScopeWorld = "on this planetoid";

        public const string ScopeColony = "across the colony";

        public const string Enabled = "Enabled";

        public const string Disabled = "Disabled";

        public const string FoodExtra = " Food in them is kept deep frozen in a sterile atmosphere, so it never spoils.";

        public const string StoringItemOn = "Duplicants bring this building whatever its filter accepts. All of it goes into the linked contents.";

        public const string StoringItemOff = "Duplicants don't bring this building anything.";

        public const string StoringLiquidOn = "Duplicants bring this building bottles of the liquids ticked in its filter, and they go into the linked contents. Its input pipe stores either way.";

        public const string StoringLiquidOff = "Duplicants don't bring this building bottled liquid. Its input pipe still stores whatever it carries.";

        public const string StoringGasOn = "Duplicants bring this building canisters of the gases ticked in its filter, and they go into the linked contents. Its input pipe stores either way.";

        public const string StoringGasOff = "Duplicants don't bring this building canisters of gas. Its input pipe still stores whatever it carries.";

        public const string TakingItemOn = "This building keeps up to {0} of each item in its filter out of the linked contents, for Duplicants to take.";

        public const string TakingItemOff = "This building keeps nothing out of the linked contents, so Duplicants can't take anything from it.";

        public const string TakingLiquidOn = "Duplicants may take bottles of the liquids ticked in its filter from this building. Its output pipe works either way.";

        public const string TakingGasOn = "Duplicants may take canisters of the gases ticked in its filter from this building. Its output pipe works either way.";

        public const string TakingFluidOff = "Duplicants can't take anything from this building. Its output pipe still sends out what its filter ticks.";

        // {0} amount, {1} temperature, {2} this building's stock of it.
        public const string ContentsRowTooltip = "{0} at {1} in the linked contents.\n{2} of it is stocked in this building.";

        // Items that carry state the store can't keep: durability, incubation, charge or upgrades. They are also
        // left out of the bin's filter.
        public static readonly Tag[] UnlinkableTags =
        {
            GameTags.Clothes,
            GameTags.Egg,
            GameTags.ChargedPortableBattery,
            GameTags.BionicUpgrade,
            GameTags.Dehydrated
        };

        // The vanilla Storage Bin's categories without food or the item kinds above.
        public static readonly List<Tag> SolidFilters =
            STORAGEFILTERS.STORAGE_LOCKERS_STANDARD.Except(UnlinkableTags).ToList();

        public static readonly List<Storage.StoredItemModifier> StoredItemModifiers = new List<Storage.StoredItemModifier>
        {
            Storage.StoredItemModifier.Hide,
            Storage.StoredItemModifier.Seal,
            Storage.StoredItemModifier.Insulate
        };

        public static void AddStrings()
        {
            string scope = SingletonOptions<Config>.Instance.PerPlanetoid ? "on the same planetoid" : "in the colony";
            AddBuilding(LinkedStorageLockerConfig.ID, "Linked Storage Bin",
                "A storage bin folded through a pocket of space it shares with every other Linked Storage Bin.",
                "Stores solid materials in contents shared by every Linked Storage Bin " + scope + ", with no limit on how much.\n\nEach bin has its own filter and its own switches for Duplicants storing and taking. Food is left to the Linked Refrigerator.");
            AddBuilding(LinkedFridgeConfig.ID, "Linked Refrigerator",
                "A refrigerator folded through a pocket of space it shares with every other Linked Refrigerator.",
                "Stores food in contents shared by every Linked Refrigerator " + scope + ", with no limit on how much.\n\nFood inside is kept deep frozen in a sterile atmosphere, so it never spoils. Needs no power.");
            AddBuilding(LinkedLiquidReservoirConfig.ID, "Linked Liquid Reservoir",
                "A reservoir folded through a pocket of space it shares with every other Linked Liquid Reservoir.",
                "Stores liquids from its input pipe in contents shared by every Linked Liquid Reservoir " + scope + ", with no limit on how much.\n\nIts output pipe sends out the liquids ticked in its filter, taking turns.");
            AddBuilding(LinkedGasReservoirConfig.ID, "Linked Gas Reservoir",
                "A reservoir folded through a pocket of space it shares with every other Linked Gas Reservoir.",
                "Stores gases from its input pipe in contents shared by every Linked Gas Reservoir " + scope + ", with no limit on how much.\n\nIts output pipe sends out the gases ticked in its filter, taking turns.");
            AddStatusItem(ContentsStatusId, "Linked Contents: {Mass}",
                "Everything stored here is shared with every {Building} {Scope}: {Count} kinds of item, {Mass} in all. "
                + "The Contents section below lists them.\n\nThe contents are insulated and keep their temperature and germs.{Extra}");
            AddStatusItem(StoringStatusId, "Storing: {State}",
                "{Detail}\n\nUse the Allow Storing or Stop Storing button to change it.");
            AddStatusItem(TakingStatusId, "Taking: {State}",
                "{Detail}\n\nUse the Allow Taking or Stop Taking button to change it.");
        }

        private static void AddStatusItem(string id, string name, string tooltip)
        {
            string key = "STRINGS.BUILDING.STATUSITEMS." + id.ToUpperInvariant();
            Strings.Add(key + ".NAME", name);
            Strings.Add(key + ".TOOLTIP", tooltip);
        }

        private static void AddBuilding(string id, string name, string desc, string effect)
        {
            string key = "STRINGS.BUILDINGS.PREFABS." + id.ToUpperInvariant();
            Strings.Add(key + ".NAME", STRINGS.UI.FormatAsLink(name, id));
            Strings.Add(key + ".DESC", desc);
            Strings.Add(key + ".EFFECT", effect);
        }

        public static Storage AddStorage(GameObject go, List<Tag> filters)
        {
            Storage storage = go.AddOrGet<Storage>();
            storage.showInUI = true;
            storage.showDescriptor = true;
            storage.storageFilters = filters;
            // Only the buffer is real, and it hands everything to the shared contents, so the capacity just has to
            // stay out of the way.
            storage.capacityKg = 100000f;
            storage.fetchCategory = Storage.FetchCategory.GeneralStorage;
            storage.doDiseaseTransfer = false;
            storage.showCapacityStatusItem = false;
            storage.SetDefaultStoredItemModifiers(StoredItemModifiers);
            return storage;
        }

        public static TreeFilterable AddTreeFilterable(GameObject go)
        {
            TreeFilterable treeFilterable = go.AddOrGet<TreeFilterable>();
            // The buffer goes back to the shared contents on its own, so nothing should drop when the filter changes.
            treeFilterable.dropIncorrectOnFilterChange = false;
            treeFilterable.autoSelectStoredOnLoad = false;
            return treeFilterable;
        }

        public static LinkedStorage AddLinkedStorage(GameObject go, LinkKind kind, bool allowStore, bool allowTake)
        {
            Prioritizable.AddRef(go);
            go.AddOrGet<CopyBuildingSettings>();
            go.AddOrGet<UserNameable>();
            LinkedStorage linked = go.AddOrGet<LinkedStorage>();
            linked.kind = kind;
            linked.allowStore = allowStore;
            linked.allowTake = allowTake;
            return linked;
        }

        public static void AddFluidPorts(GameObject go, ConduitType type)
        {
            ConduitConsumer consumer = go.AddOrGet<ConduitConsumer>();
            consumer.conduitType = type;
            consumer.ignoreMinMassCheck = true;
            consumer.forceAlwaysSatisfied = true;
            consumer.alwaysConsume = true;
            consumer.capacityKG = 100000f;
            ConduitDispenser dispenser = go.AddOrGet<ConduitDispenser>();
            dispenser.conduitType = type;
            dispenser.elementFilter = null;
            // The storage-bin style list, like a Bottle Emptier's, picks the elements the reservoir works with: what
            // Duplicants bring, what it stocks and what its output pipe sends out.
            AddTreeFilterable(go);
        }
    }

    public class LinkedStorageLockerConfig : IBuildingConfig
    {
        public const string ID = "LinkedStorageLocker";

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 1, 2, "storagelocker_kanim", 30, 10f,
                BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.RAW_MINERALS_OR_METALS, 1600f, BuildLocationRule.Anywhere,
                noise: NOISE_POLLUTION.NONE, decor: BUILDINGS.DECOR.PENALTY.TIER1);
            def.Floodable = false;
            def.Overheatable = false;
            def.AudioCategory = "Metal";
            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            LinkedBuildings.AddStorage(go, LinkedBuildings.SolidFilters);
            LinkedBuildings.AddTreeFilterable(go);
            LinkedBuildings.AddLinkedStorage(go, LinkKind.Solid, true, true);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGetDef<StorageController.Def>();
        }
    }

    public class LinkedFridgeConfig : IBuildingConfig
    {
        public const string ID = "LinkedFridge";

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 1, 2, "fridge_kanim", 30, 10f,
                BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.RAW_MINERALS, 800f, BuildLocationRule.Anywhere,
                noise: NOISE_POLLUTION.NOISY.TIER0, decor: BUILDINGS.DECOR.BONUS.TIER1);
            def.Floodable = false;
            def.Overheatable = false;
            def.AudioCategory = "Metal";
            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.KitchenRefrigerator);
            LinkedBuildings.AddStorage(go, STORAGEFILTERS.FOOD);
            LinkedBuildings.AddTreeFilterable(go).allResourceFilterLabelString =
                STRINGS.UI.UISIDESCREENS.TREEFILTERABLESIDESCREEN.ALLBUTTON_EDIBLES;
            // Like the vanilla refrigerator, food marked for composting isn't fetched.
            LinkedBuildings.AddLinkedStorage(go, LinkKind.Food, true, true).forbiddenTags = new[] { GameTags.Compostable };
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGetDef<StorageController.Def>();
        }
    }

    public class LinkedLiquidReservoirConfig : IBuildingConfig
    {
        public const string ID = "LinkedLiquidReservoir";

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 2, 3, "liquidreservoir_kanim", 100, 120f,
                BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.ALL_METALS, 800f, BuildLocationRule.Anywhere,
                BUILDINGS.DECOR.PENALTY.TIER1, NOISE_POLLUTION.NOISY.TIER0);
            def.InputConduitType = ConduitType.Liquid;
            def.OutputConduitType = ConduitType.Liquid;
            def.Floodable = false;
            def.Overheatable = false;
            def.ViewMode = OverlayModes.LiquidConduits.ID;
            def.AudioCategory = "HollowMetal";
            def.UtilityInputOffset = new CellOffset(1, 2);
            def.UtilityOutputOffset = new CellOffset(0, 0);
            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.LiquidVentIDs, ID);
            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            LinkedBuildings.AddStorage(go, STORAGEFILTERS.LIQUIDS);
            LinkedBuildings.AddFluidPorts(go, ConduitType.Liquid);
            LinkedBuildings.AddLinkedStorage(go, LinkKind.Liquid, true, false);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGetDef<StorageController.Def>();
            go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayBehindConduits);
        }
    }

    public class LinkedGasReservoirConfig : IBuildingConfig
    {
        public const string ID = "LinkedGasReservoir";

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 5, 3, "gasstorage_kanim", 100, 120f,
                BUILDINGS.CONSTRUCTION_MASS_KG.TIER4, MATERIALS.ALL_METALS, 800f, BuildLocationRule.Anywhere,
                BUILDINGS.DECOR.PENALTY.TIER1, NOISE_POLLUTION.NOISY.TIER0);
            def.InputConduitType = ConduitType.Gas;
            def.OutputConduitType = ConduitType.Gas;
            def.Floodable = false;
            def.Overheatable = false;
            def.ViewMode = OverlayModes.GasConduits.ID;
            def.AudioCategory = "HollowMetal";
            def.UtilityInputOffset = new CellOffset(1, 2);
            def.UtilityOutputOffset = new CellOffset(0, 0);
            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.GasVentIDs, ID);
            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            LinkedBuildings.AddStorage(go, STORAGEFILTERS.GASES);
            LinkedBuildings.AddFluidPorts(go, ConduitType.Gas);
            LinkedBuildings.AddLinkedStorage(go, LinkKind.Gas, true, false);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGetDef<StorageController.Def>();
            go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayBehindConduits);
        }
    }
}
