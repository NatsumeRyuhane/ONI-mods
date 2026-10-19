using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using PeterHan.PLib.Options;
using UnityEngine;

namespace BuffedWheezewort
{
    // Patches the Wheezewort (ColdBreatherConfig). Vanilla emits 480 rads in Spaced Out, breathes 1 kg/s of gas when
    // planted (a quarter of that when wild), cools it by 5 degrees, takes 4 kg of Phosphorite per cycle, and grows
    // between 213.15 K and 368.15 K (dies below 183.15 K or above 463.15 K).
    public static class Patch
    {
        // Vanilla's lethal limits sit this far outside its growth range. The configured range keeps the same margins.
        private const float LethalLowMargin = 30f;
        private const float LethalHighMargin = 95f;

        // Vanilla buffers 2 kg of gas for 1 kg/s of throughput.
        private const float GasBufferSeconds = 2f;

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

                if (config.PhosphoriteGramsPerCycle > 0f)
                {
                    SetPhosphoriteRate(__result, config.PhosphoriteGramsPerCycle / 1000f / 600f);
                }
                else
                {
                    RemoveFertilizer(__result);
                }

                float minGrowth = config.MinGrowthTemperature;
                float maxGrowth = Mathf.Max(config.MaxGrowthTemperature, minGrowth + 1f);
                __result.AddOrGet<TemperatureVulnerable>().Configure(minGrowth, minGrowth - LethalLowMargin, maxGrowth,
                    maxGrowth + LethalHighMargin);

                var coldBreather = __result.AddOrGet<ColdBreather>();
                coldBreather.deltaEmitTemperature = config.EmissionTemperatureDelta;
                coldBreather.consumptionRate = config.GasThroughput;

                // The prefab starts at the wild rate. ColdBreather.OnReplanted switches it once the plant is planted.
                var consumer = __result.GetComponent<ElementConsumer>();
                consumer.consumptionRate = config.GasThroughput * config.WildThroughputMultiplier;
                consumer.capacityKG = Mathf.Max(consumer.capacityKG, config.GasThroughput * GasBufferSeconds);
            }

            // Changes the Phosphorite delivery and the fertilizer list that the vanilla prefab already has, with the
            // same capacity, refill and minimum ratios as EntityTemplates.ExtendPlantToFertilizable.
            private static void SetPhosphoriteRate(GameObject prefab, float rate)
            {
                var phosphorite = SimHashes.Phosphorite.CreateTag();
                foreach (var delivery in prefab.GetComponents<ManualDeliveryKG>())
                {
                    if (delivery.RequestedItemTag != phosphorite) continue;

                    delivery.capacity = rate * 600f * 3f;
                    delivery.refillMass = rate * 600f * 0.5f;
                    delivery.MinimumMass = rate * 600f * 0.5f;
                }

                prefab.AddOrGetDef<FertilizationMonitor.Def>().consumedElements = new[]
                {
                    new PlantElementAbsorber.ConsumeInfo
                    {
                        tag = phosphorite,
                        massConsumptionRate = rate
                    }
                };
            }

            // A rate of 0 still starves the plant: FertilizationMonitor only counts it as fertilized when it finds
            // Phosphorite in storage, and a delivery capacity of 0 never brings any. Removes the Phosphorite delivery
            // and the FertilizationMonitor instead, so the plant needs no fertilizer, like plants that never had one.
            private static void RemoveFertilizer(GameObject prefab)
            {
                var phosphorite = SimHashes.Phosphorite.CreateTag();
                foreach (var delivery in prefab.GetComponents<ManualDeliveryKG>())
                {
                    if (delivery.RequestedItemTag == phosphorite) Object.DestroyImmediate(delivery);
                }

                var def = prefab.GetDef<FertilizationMonitor.Def>();
                if (def != null) prefab.GetComponent<StateMachineController>().cmpdef.defs.Remove(def);
            }
        }

        // ColdBreather.OnReplanted runs when the plant is planted or replanted. It resets the emitter to the vanilla
        // 480 rads and sets the wild rate to a hard-coded quarter of the throughput. Set both again afterwards.
        // OnReplanted is private, so it's named by string.
        [HarmonyPatch(typeof(ColdBreather), "OnReplanted")]
        public static class ColdBreather_OnReplanted_Patch
        {
            public static void Postfix(ColdBreather __instance)
            {
                var config = SingletonOptions<Config>.Instance;

                var monitor = __instance.GetComponent<ReceptacleMonitor>();
                if (monitor != null && !monitor.Replanted)
                {
                    __instance.GetComponent<ElementConsumer>().consumptionRate =
                        __instance.consumptionRate * config.WildThroughputMultiplier;
                }

                var emitter = __instance.GetComponent<RadiationEmitter>();
                if (emitter == null) return;

                emitter.emitRads = config.Radiation;
                emitter.Refresh();
            }
        }

        // ColdBreather.Exhale emits each gas at Mathf.Max(element.lowTemp + 5, temperature + deltaEmitTemperature), so
        // it never cools gas to within 5 degrees of its condensation point. Replaces that call (the only Mathf.Max in
        // the method) with EmitTemperature. Exhale is private, so it's named by string.
        [HarmonyPatch(typeof(ColdBreather), "Exhale")]
        public static class ColdBreather_Exhale_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                return new CodeMatcher(instructions)
                    .MatchStartForward(new CodeMatch(OpCodes.Call,
                        AccessTools.Method(typeof(Mathf), nameof(Mathf.Max), new[] { typeof(float), typeof(float) })))
                    .ThrowIfInvalid("Mathf.Max not found in ColdBreather.Exhale")
                    .SetOperandAndAdvance(AccessTools.Method(typeof(ColdBreather_Exhale_Patch), nameof(EmitTemperature)))
                    .InstructionEnumeration();
            }

            private static float EmitTemperature(float floor, float cooled)
            {
                // Without the floor, keep the gas above absolute zero.
                return SingletonOptions<Config>.Instance.CoolBelowCondensation
                    ? Mathf.Max(cooled, 1f)
                    : Mathf.Max(floor, cooled);
            }
        }
    }
}
