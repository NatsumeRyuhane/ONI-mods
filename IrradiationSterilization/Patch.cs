using Database;
using HarmonyLib;
using PeterHan.PLib.Options;

namespace IrradiationSterilization
{
    // Radiation kills a germ type at radiationKillRate germs per rad per second, in a straight line, regardless of how
    // many germs there are. Vanilla rates (TUNING.DISEASE.RADIATION_KILL_RATE): Food Poisoning and Slimelung 2.5,
    // Zombie Spores 1, Floral Scent 0 (immune). Radiation only exists with Spaced Out.
    public static class Patch
    {
        // The Diseases set builds every germ type, both for the game's Db and for worldgen's stats-only copy. The rate
        // is read later by SimMessages.CreateDiseaseTable (germs in cells and pipes, run by the native sim),
        // DiseaseContainers.CalculateDelta (germs on items and buildings) and the germ details panel, so scaling it
        // here changes all three.
        [HarmonyPatch(typeof(Diseases), MethodType.Constructor, typeof(ResourceSet), typeof(bool))]
        public static class Diseases_Constructor_Patch
        {
            public static void Postfix(Diseases __instance)
            {
                float multiplier = SingletonOptions<Config>.Instance.KillRateMultiplier;
                foreach (var disease in __instance.resources)
                {
                    disease.radiationKillRate *= multiplier;
                }
            }
        }
    }
}
