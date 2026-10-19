using System.Collections.Generic;
using PeterHan.PLib.Options;
using TUNING;
using UnityEngine;

namespace SublimationSconce
{
    // A sconce like the vanilla Oxylite Sconce that holds any solid that sublimates. Each sconce picks its materials
    // from a flat list, like the Oxidizer Tank. The vanilla sconce has no logic of its own: it is a storage that isn't
    // sealed, so the stored item's own Sublimates component keeps running. This one works the same way, and
    // Patch.Sublimates_Sim200ms_Patch speeds up what it holds.
    public class SublimationSconceConfig : IBuildingConfig
    {
        public const string ID = "SublimationSconce";

        public const string FilterHeaderKey = "STRINGS.BUILDINGS.PREFABS.SUBLIMATIONSCONCE.UI_FILTER_CATEGORY";

        public static float CapacityKg
        {
            get { return SingletonOptions<Config>.Instance.CapacityKg; }
        }

        // Every solid whose item sublimates. GeneratedOre registers the ore prefabs before the buildings, so this
        // also picks up elements added by other mods. Nuclear Waste is a liquid and is left out with the other liquids.
        public static List<Tag> GetSublimatingSolids()
        {
            List<Tag> tags = new List<Tag>();
            foreach (Element element in ElementLoader.elements)
            {
                if (element == null || !element.IsSolid)
                {
                    continue;
                }
                GameObject prefab = Assets.TryGetPrefab(element.tag);
                Sublimates sublimates = prefab != null ? prefab.GetComponent<Sublimates>() : null;
                if (sublimates != null && !sublimates.decayStorage)
                {
                    tags.Add(element.tag);
                }
            }
            return tags;
        }

        // The sconce art ships with The Frosty Planet Pack, like the vanilla sconce.
        public override string[] GetRequiredDlcIds()
        {
            return DlcManager.DLC2;
        }

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 1, 1, "oxy_sconce_kanim", 10, 10f,
                BUILDINGS.CONSTRUCTION_MASS_KG.TIER1, MATERIALS.ALL_METALS, 800f, BuildLocationRule.Anywhere,
                noise: NOISE_POLLUTION.NOISY.TIER0, decor: BUILDINGS.DECOR.BONUS.TIER0);
            def.RequiresPowerInput = false;
            def.ExhaustKilowattsWhenActive = 0f;
            def.SelfHeatKilowattsWhenActive = 0f;
            def.ViewMode = OverlayModes.Oxygen.ID;
            def.AudioCategory = "HollowMetal";
            def.Breakable = true;
            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            Prioritizable.AddRef(go);
            List<Tag> materials = GetSublimatingSolids();
            Storage storage = go.AddOrGet<Storage>();
            storage.capacityKg = CapacityKg;
            storage.showInUI = true;
            storage.showCapacityStatusItem = true;
            storage.showCapacityAsMainStatus = true;
            // TreeFilterable reads this when a resource is discovered, so it can't be left null.
            storage.storageFilters = materials;
            TreeFilterable treeFilterable = go.AddOrGet<TreeFilterable>();
            treeFilterable.preventAutoAddOnDiscovery = true;
            FlatTagFilterable flatTagFilterable = go.AddOrGet<FlatTagFilterable>();
            flatTagFilterable.tagOptions = materials;
            flatTagFilterable.headerText = Strings.Get(FilterHeaderKey);
            go.AddOrGet<CopyBuildingSettings>();
            go.AddOrGet<SublimationSconce>();
        }

        // The vanilla sconce also counts as an oxygen source for the tutorial. This one may not make oxygen, so it
        // doesn't.
        public override void DoPostConfigureComplete(GameObject go)
        {
        }
    }
}
