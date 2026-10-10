using HarmonyLib;
using PeterHan.PLib.Options;

namespace LowPowerFilters
{
    // Sets each filter's power draw from the options. The options are read when the BuildingDef is created, so a
    // change needs a restart.
    public static class Patch
    {
        [HarmonyPatch(typeof(GasFilterConfig), nameof(GasFilterConfig.CreateBuildingDef))]
        public static class GasFilterConfig_CreateBuildingDef_Patch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.EnergyConsumptionWhenActive = SingletonOptions<Config>.Instance.FilterPowerRequirementGas;
            }
        }

        [HarmonyPatch(typeof(LiquidFilterConfig), nameof(LiquidFilterConfig.CreateBuildingDef))]
        public static class LiquidFilterConfig_CreateBuildingDef_Patch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.EnergyConsumptionWhenActive = SingletonOptions<Config>.Instance.FilterPowerRequirementLiquid;
            }
        }

        [HarmonyPatch(typeof(SolidFilterConfig), nameof(SolidFilterConfig.CreateBuildingDef))]
        public static class SolidFilterConfig_CreateBuildingDef_Patch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.EnergyConsumptionWhenActive = SingletonOptions<Config>.Instance.FilterPowerRequirementSolid;
            }
        }
    }
}
