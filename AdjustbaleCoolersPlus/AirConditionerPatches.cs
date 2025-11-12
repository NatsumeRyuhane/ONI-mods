using HarmonyLib;
using UnityEngine;
using PeterHan.PLib.Options;

namespace AdjustableCoolersPlus
{
    public static class AirConditionerPatches
    {
        public const float EnergyModifier = 20f / 14f;

        [HarmonyPatch(typeof(AirConditionerConfig), "ConfigureBuildingTemplate")]
        private static class Patch_AirConditionerConfig_ConfigureBuildingTemplate
        {
            public static void Postfix(GameObject go)
            {
                go.AddOrGet<AirConditionerAdjustable>();
            }
        }

        [HarmonyPatch(typeof(LiquidConditionerConfig), "ConfigureBuildingTemplate")]
        private static class Patch_LiquidConditionerConfig_ConfigureBuildingTemplate
        {
            public static void Postfix(GameObject go)
            {
                go.AddOrGet<AirConditionerAdjustable>();
            }
        }



        [HarmonyPatch(typeof(AirConditionerConfig), "CreateBuildingDef")]
        private static class PatchAirConditionerConfigCreateBuildingDef
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.EnergyConsumptionWhenActive = SingletonOptions<Config>.Instance.AirConditionerWattsPerDegree *
                                                       Mathf.Max(
                                                           Mathf.Abs(SingletonOptions<Config>.Instance.MinTemperatureDelta),
                                                           Mathf.Abs(SingletonOptions<Config>.Instance.MaxTemperatureDelta));
            }
        }

        [HarmonyPatch(typeof(LiquidConditionerConfig), "CreateBuildingDef")]
        private static class PatchLiquidConditionerConfigCreateBuildingDef
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.EnergyConsumptionWhenActive = SingletonOptions<Config>.Instance.AquatunerWattsPerDegree * 
                                                       Mathf.Max(
                                                           Mathf.Abs(SingletonOptions<Config>.Instance.MinTemperatureDelta),
                                                           Mathf.Abs(SingletonOptions<Config>.Instance.MaxTemperatureDelta));
            }
        }
    }
}
