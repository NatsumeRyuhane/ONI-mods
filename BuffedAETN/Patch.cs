using HarmonyLib;
using PeterHan.PLib.Options;
using UnityEngine;

namespace BuffedAETN
{
    // Patches the Anti Entropy Thermo-Nullifier (AETN, MassiveHeatSinkConfig). Vanilla removes 80 kW (64 kW self,
    // 16 kW exhaust), stops below 100 K, burns 10 g/s of Hydrogen, buffers 100 g of it and can be flooded.
    public static class Patch
    {
        // Same split between self heat and exhaust as vanilla (64 / 16 of 80 kW).
        private const float SelfHeatShare = 0.8f;
        private const float ExhaustShare = 0.2f;

        // The Hydrogen buffer always holds at least this many seconds of consumption.
        private const float MinBufferSeconds = 3f;

        private const float GramsPerKg = 1000f;

        [HarmonyPatch(typeof(MassiveHeatSinkConfig), nameof(MassiveHeatSinkConfig.CreateBuildingDef))]
        public static class MassiveHeatSinkConfig_CreateBuildingDef_Patch
        {
            public static void Postfix(BuildingDef __result)
            {
                // Negative heat is cooling.
                int coolingKw = SingletonOptions<Config>.Instance.CoolingPower;
                __result.Floodable = false;
                __result.SelfHeatKilowattsWhenActive = -SelfHeatShare * coolingKw;
                __result.ExhaustKilowattsWhenActive = -ExhaustShare * coolingKw;
            }
        }

        [HarmonyPatch(typeof(MassiveHeatSinkConfig), nameof(MassiveHeatSinkConfig.DoPostConfigureComplete))]
        public static class MassiveHeatSinkConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(GameObject go)
            {
                var config = SingletonOptions<Config>.Instance;
                go.AddOrGet<MinimumOperatingTemperature>().minimumTemperature = config.MinimumOperatingTemperature;

                float consumptionKgPerSecond = config.HydrogenConsumptionRate / GramsPerKg;
                float storageKg = Mathf.Max(config.HydrogenStorageCapacity / GramsPerKg,
                    consumptionKgPerSecond * MinBufferSeconds);
                go.AddOrGet<Storage>().capacityKg = storageKg;
                go.AddOrGet<ConduitConsumer>().capacityKG = storageKg;
                go.AddOrGet<ElementConverter>().consumedElements = new[]
                {
                    new ElementConverter.ConsumedElement(ElementLoader.FindElementByHash(SimHashes.Hydrogen).tag,
                        consumptionKgPerSecond)
                };
            }
        }
    }
}
