using HarmonyLib;
using PeterHan.PLib.Options;
using UnityEngine;

namespace BetterEmptier
{
    public static class Patch
    {
        // Both emptiers create their Storage and BottleEmptier in ConfigureBuildingTemplate. Vanilla stores 200 kg and
        // releases 10 kg/s (Bottle Emptier) or 0.25 kg/s (Gas Bottle Emptier).
        private static void Configure(GameObject go)
        {
            var config = SingletonOptions<Config>.Instance;
            go.AddOrGet<Storage>().capacityKg = config.Capacity;
            go.AddOrGet<BottleEmptier>().emptyRate *= config.EmissionRateMultiplier;
        }

        [HarmonyPatch(typeof(BottleEmptierConfig), nameof(BottleEmptierConfig.ConfigureBuildingTemplate))]
        public static class BottleEmptierConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(GameObject go)
            {
                Configure(go);
            }
        }

        [HarmonyPatch(typeof(BottleEmptierGasConfig), nameof(BottleEmptierGasConfig.ConfigureBuildingTemplate))]
        public static class BottleEmptierGasConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(GameObject go)
            {
                Configure(go);
            }
        }
    }
}
