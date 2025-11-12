using System;
using HarmonyLib;
using PeterHan.PLib.Options;
using UnityEngine;

namespace BuffedAETN
{
    public class Patch
    {
        [HarmonyPatch(typeof(MassiveHeatSinkConfig))]
        [HarmonyPatch("CreateBuildingDef")]
        public static class MassiveHeatSinkConfig_CreateBuildingDef_Patch
        {
            public static void Postfix(ref BuildingDef __result)
            {
                __result.Floodable = false;
                __result.SelfHeatKilowattsWhenActive = (float) -0.8 * SingletonOptions<Config>.Instance.CoolingPower; 
                __result.ExhaustKilowattsWhenActive = (float) -0.2 * SingletonOptions<Config>.Instance.CoolingPower;
            }
        }
        
        [HarmonyPatch(typeof(MassiveHeatSinkConfig))]
        [HarmonyPatch("DoPostConfigureComplete")]
        public static class MassiveHeatSinkConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(ref GameObject go)
            {
                go.AddOrGet<MinimumOperatingTemperature>().minimumTemperature = (float) SingletonOptions<Config>.Instance.MinimumOperatingTemperature;
            }
        }
        
        [HarmonyPatch(typeof(MassiveHeatSinkConfig), nameof(MassiveHeatSinkConfig.DoPostConfigureComplete))]
        public static class MassiveHeatSinkConfig_DoPostConfigureComplete_Patch2
        {
            public static void Postfix(ref GameObject go)
            {
                float storageCapacity = Math.Max(
                        (float) SingletonOptions<Config>.Instance.HydrogenStorageCapacity / 1000, 
                        ((float) SingletonOptions<Config>.Instance.HydrogenConsumptionRate / 1000) * 3);
                go.AddOrGet<Storage>().capacityKg = storageCapacity;
                go.AddOrGet<ConduitConsumer>().capacityKG = storageCapacity;
                go.AddOrGet<ElementConverter>().consumedElements = new ElementConverter.ConsumedElement[1]
                {
                    new ElementConverter.ConsumedElement(ElementLoader.FindElementByHash(SimHashes.Hydrogen).tag, (float) SingletonOptions<Config>.Instance.HydrogenConsumptionRate / 1000)
                };
            }
        }
    }
}