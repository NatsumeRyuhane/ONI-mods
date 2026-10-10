using HarmonyLib;
using PeterHan.PLib.Options;
using UnityEngine;

namespace BuffedWheezewort
{
    // Patches the Wheezewort (ColdBreatherConfig). Vanilla emits 480 rads in Spaced Out, cools the gas it breathes by
    // 5 degrees, takes 4 kg of Phosphorite per cycle, and grows between 213.15 K and 368.15 K (dies below 183.15 K or
    // above 463.15 K).
    public static class Patch
    {
        [HarmonyPatch(typeof(ColdBreatherConfig), nameof(ColdBreatherConfig.CreatePrefab))]
        public static class ColdBreatherConfig_CreatePrefab_Patch
        {
            public static void Postfix(GameObject __result)
            {
                var config = SingletonOptions<Config>.Instance;

                // The emitter only exists when radiation is enabled (Spaced Out).
                var emitter = __result.GetComponent<RadiationEmitter>();
                if (emitter != null)
                {
                    emitter.emitRads = config.Radiation;
                }

                // The vanilla prefab is already fertilizable; this call adds a second Phosphorite delivery and replaces
                // the fertilizer list the plant consumes from.
                EntityTemplates.ExtendPlantToFertilizable(__result, new[]
                {
                    new PlantElementAbsorber.ConsumeInfo
                    {
                        tag = SimHashes.Phosphorite.CreateTag(),
                        massConsumptionRate = config.PhosphorusConsumptionRate
                    }
                });

                // Grows between -100 C and 100 C, and dies below -150 C or above 150 C.
                __result.AddOrGet<TemperatureVulnerable>().Configure(173.15f, 123.15f, 373.15f, 423.15f);
                __result.AddOrGet<ColdBreather>().deltaEmitTemperature = config.EmissionTemperatureDelta;
            }
        }

        // ColdBreather.OnReplanted runs when the plant is planted or replanted, and resets the emitter to the vanilla
        // 480 rads. Set it again afterwards. OnReplanted is private, so it's named by string.
        [HarmonyPatch(typeof(ColdBreather), "OnReplanted")]
        public static class ColdBreather_OnReplanted_Patch
        {
            public static void Postfix(ColdBreather __instance)
            {
                var emitter = __instance.GetComponent<RadiationEmitter>();
                if (emitter == null) return;

                emitter.emitRads = SingletonOptions<Config>.Instance.Radiation;
                emitter.Refresh();
            }
        }
    }
}
