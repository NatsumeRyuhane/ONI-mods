using System;
using HarmonyLib;
using PeterHan.PLib.Options;
using UnityEngine;

namespace BetterEmptier
{
    public class Patch
    {
        [HarmonyPatch(typeof(BottleEmptierConfig))]
        [HarmonyPatch(nameof(BottleEmptierConfig.ConfigureBuildingTemplate))]
        public static class BottleEmptierConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(ref GameObject go, ref Tag prefab_tag)
            {
                Storage s = go.AddOrGet<Storage>();
                if (s == null) return;
                s.capacityKg = SingletonOptions<Config>.Instance.Capacity;
                BottleEmptier be = go.AddOrGet<BottleEmptier>();
                if (be == null) return;
                be.emptyRate *= SingletonOptions<Config>.Instance.EmissionRateMultiplier;
            }
        }
        
        [HarmonyPatch(typeof(BottleEmptierGasConfig))]
        [HarmonyPatch(nameof(BottleEmptierGasConfig.ConfigureBuildingTemplate))]
        public static class BottleEmptierGasConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(ref GameObject go, ref Tag prefab_tag)
            {
                if (go == null) return;
                Storage s = go.AddOrGet<Storage>();
                if (s == null) return;
                s.capacityKg = SingletonOptions<Config>.Instance.Capacity;
                BottleEmptier be = go.AddOrGet<BottleEmptier>();
                if (be == null) return;
                be.emptyRate *= SingletonOptions<Config>.Instance.EmissionRateMultiplier;
            }
        }
    }
}