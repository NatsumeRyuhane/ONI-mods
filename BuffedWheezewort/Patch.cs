using HarmonyLib;
using PeterHan.PLib.Options;
using UnityEngine;

namespace BuffedWheezewort
{
    public class Patches
    {
        [HarmonyPatch(typeof(ColdBreatherConfig), nameof(ColdBreatherConfig.CreatePrefab))]
        public static class Patch_CreatePrefab
        {
            public static void Postfix(GameObject __result)
            {
                __result.GetComponent<RadiationEmitter>().emitRads = SingletonOptions<Config>.Instance.Radiation;
                EntityTemplates.ExtendPlantToFertilizable(__result, new PlantElementAbsorber.ConsumeInfo[1]
                {
                    new PlantElementAbsorber.ConsumeInfo()
                    {
                        tag = SimHashes.Phosphorite.CreateTag(),
                        massConsumptionRate = SingletonOptions<Config>.Instance.PhosphorusConsumptionRate
                    }
                });
                __result.AddOrGet<TemperatureVulnerable>().Configure(173.15f, 123.15f, 373.15f, 423.15f);
            }
        }
        
        [HarmonyPatch(typeof(ColdBreatherConfig), nameof(ColdBreatherConfig.CreatePrefab))]
        public static class Patch_ColdBreatherConfig_CreatePrefab
        {
            public static void Postfix(GameObject __result)
            {
                __result.AddOrGet<ColdBreather>().deltaEmitTemperature = SingletonOptions<Config>.Instance.EmissionTemperatureDelta;
            }
        }

        [HarmonyPatch(typeof (ColdBreather))]
        [HarmonyPatch("OnReplanted")]
        public static class Patch_OnReplanted
        {
            internal static void Postfix(ColdBreather __instance)
            {
                RadiationEmitter component = __instance.GetComponent<RadiationEmitter>();
                if (!((Object) component != (Object) null))
                    return;
                component.emitRads = SingletonOptions<Config>.Instance.Radiation;
                component.Refresh();
            }
        }
    }
}