using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace LinkedStorage
{
    public class Patch
    {
        // Db.Initialize runs before the game registers buildings (LegacyModMain.Load) and before Techs.PostProcess,
        // so strings, the build menu entries and the research unlocks are all in place in time. Each building sits
        // after its vanilla counterpart and is unlocked by the same research. The Storage Bin needs none.
        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public static class Db_Initialize_Patch
        {
            public static void Postfix()
            {
                LinkedBuildings.AddStrings();
                ModUtil.AddBuildingToPlanScreen("Base", LinkedStorageLockerConfig.ID, "storage", StorageLockerConfig.ID);
                ModUtil.AddBuildingToPlanScreen("Base", LinkedLiquidReservoirConfig.ID, "storage", LiquidReservoirConfig.ID);
                ModUtil.AddBuildingToPlanScreen("Base", LinkedGasReservoirConfig.ID, "storage", GasReservoirConfig.ID);
                ModUtil.AddBuildingToPlanScreen("Food", LinkedFridgeConfig.ID, "storage", RefrigeratorConfig.ID);
                Db.Get().Techs.TryGet("ImprovedLiquidPiping")?.unlockedItemIDs.Add(LinkedLiquidReservoirConfig.ID);
                Db.Get().Techs.TryGet("HVAC")?.unlockedItemIDs.Add(LinkedGasReservoirConfig.ID);
                Db.Get().Techs.TryGet("Agriculture")?.unlockedItemIDs.Add(LinkedFridgeConfig.ID);
            }
        }

        // The shared contents live on the SaveGame object, so SaveLoadRoot saves and loads them with the colony.
        [HarmonyPatch(typeof(SaveGame), "OnPrefabInit")]
        public static class SaveGame_OnPrefabInit_Patch
        {
            public static void Postfix(SaveGame __instance)
            {
                __instance.gameObject.AddOrGet<LinkedPool>();
            }
        }

        // A deconstructed linked building would drop its buffer on the floor. Its contents go back to the shared
        // store first, so they stay linked.
        [HarmonyPatch(typeof(Storage), "OnQueueDestroyObject")]
        public static class Storage_OnQueueDestroyObject_Patch
        {
            public static void Prefix(Storage __instance)
            {
                LinkedStorage linked = __instance.GetComponent<LinkedStorage>();
                if (linked != null)
                {
                    linked.ReturnAll();
                }
            }
        }

        // The Contents section of a linked building lists the shared contents instead of the small buffer it really
        // holds.
        [HarmonyPatch(typeof(SimpleInfoScreen), "RefreshStoragePanel")]
        public static class SimpleInfoScreen_RefreshStoragePanel_Patch
        {
            public static bool Prefix(CollapsibleDetailContentPanel targetPanel, GameObject targetEntity)
            {
                LinkedStorage linked = targetEntity != null ? targetEntity.GetComponent<LinkedStorage>() : null;
                if (linked == null)
                {
                    return true;
                }
                linked.FillContentsPanel(targetPanel);
                return false;
            }
        }

        // The resource panel, the pinned resources, the Resources screen and the material picker all read a world's
        // amounts here. GetAmount reads them through this method too. The shared contents count for the world they
        // belong to.
        [HarmonyPatch(typeof(WorldInventory), nameof(WorldInventory.GetTotalAmount))]
        public static class WorldInventory_GetTotalAmount_Patch
        {
            public static void Postfix(WorldInventory __instance, Tag tag, ref float __result)
            {
                LinkedPool pool = LinkedPool.Instance;
                WorldContainer world = __instance.WorldContainer;
                if (pool != null && world != null)
                {
                    __result += pool.GetInventoryAmount(world.id, tag);
                }
            }
        }

        // Calories come from RationTracker, which counts food items itself, for the food rows and the colony's
        // calorie total. The shared contents' food counts too. Harmony patches this generic method for every tracker
        // type, so the postfix checks for RationTracker.
        [HarmonyPatch(typeof(WorldResourceAmountTracker<RationTracker>), nameof(WorldResourceAmountTracker<RationTracker>.CountAmount),
            new[] { typeof(Dictionary<string, float>), typeof(float), typeof(WorldInventory), typeof(bool) },
            new[] { ArgumentType.Normal, ArgumentType.Out, ArgumentType.Normal, ArgumentType.Normal })]
        public static class RationTracker_CountAmount_Patch
        {
            public static void Postfix(object __instance, Dictionary<string, float> unitCountByID,
                ref float totalUnitsFound, WorldInventory inventory, ref float __result)
            {
                LinkedPool pool = LinkedPool.Instance;
                WorldContainer world = inventory != null ? inventory.WorldContainer : null;
                if (!(__instance is RationTracker) || pool == null || world == null)
                {
                    return;
                }
                float units;
                __result += pool.GetCalories(world.id, null, unitCountByID, out units);
                totalUnitsFound += units;
            }
        }

        [HarmonyPatch(typeof(RationTracker), nameof(RationTracker.CountAmountForItemWithID))]
        public static class RationTracker_CountAmountForItemWithID_Patch
        {
            public static void Postfix(string ID, WorldInventory inventory, ref float __result)
            {
                LinkedPool pool = LinkedPool.Instance;
                WorldContainer world = inventory != null ? inventory.WorldContainer : null;
                if (pool != null && world != null)
                {
                    float units;
                    __result += pool.GetCalories(world.id, ID, null, out units);
                }
            }
        }

        // Food in a Linked Refrigerator is deep frozen, whatever the temperature around it, so it doesn't rot.
        [HarmonyPatch(typeof(Rottable), nameof(Rottable.RefrigerationLevel))]
        public static class Rottable_RefrigerationLevel_Patch
        {
            public static bool Prefix(IRottable rottable, ref Rottable.RotRefrigerationLevel __result)
            {
                if (!InLinkedFridge(rottable))
                {
                    return true;
                }
                __result = Rottable.RotRefrigerationLevel.Frozen;
                return false;
            }
        }

        // ...and in a sterile atmosphere, whatever gas is around it.
        [HarmonyPatch(typeof(Rottable), nameof(Rottable.AtmosphereQuality))]
        public static class Rottable_AtmosphereQuality_Patch
        {
            public static bool Prefix(IRottable rottable, ref Rottable.RotAtmosphereQuality __result)
            {
                if (!InLinkedFridge(rottable))
                {
                    return true;
                }
                __result = Rottable.RotAtmosphereQuality.Sterilizing;
                return false;
            }
        }

        private static bool InLinkedFridge(IRottable rottable)
        {
            Pickupable pickupable = rottable.gameObject.GetComponent<Pickupable>();
            Storage storage = pickupable != null ? pickupable.storage : null;
            LinkedStorage linked = storage != null ? storage.GetComponent<LinkedStorage>() : null;
            return linked != null && linked.kind == LinkKind.Food;
        }
    }
}
