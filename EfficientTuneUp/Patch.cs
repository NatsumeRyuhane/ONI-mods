using HarmonyLib;
using UnityEngine;

namespace EfficientTuneUp
{
    public static class Patch
    {
        // Every building that takes Engie's Tune-Up is set up here, the vanilla generators and any modded one that
        // uses the same helper. Buildings get the component on their completed prefab, and the tag lets other mods
        // know the tune-up takes its power out of the heat. Some configs add their Generator after this call, so the
        // component looks for it when it spawns.
        [HarmonyPatch(typeof(Tinkerable), nameof(Tinkerable.MakePowerTinkerable))]
        public static class Tinkerable_MakePowerTinkerable_Patch
        {
            public static void Postfix(GameObject prefab)
            {
                prefab.AddOrGet<TuneUpHeat>();
                prefab.GetComponent<KPrefabID>().AddTag(TuneUpHeat.EfficientTuneUpTag);
            }
        }

        // Every generator makes its power through GenerateJoules, so this sees what each one actually makes, also
        // when a Steam Turbine or Tidal Turbine runs below its rating.
        [HarmonyPatch(typeof(Generator), nameof(Generator.GenerateJoules))]
        public static class Generator_GenerateJoules_Patch
        {
            public static void Postfix(Generator __instance, float joulesAvailable)
            {
                TuneUpHeat heat;
                if (TuneUpHeat.ByGenerator.TryGetValue(__instance, out heat))
                {
                    heat.AddGeneratedJoules(joulesAvailable);
                }
            }
        }
    }
}
