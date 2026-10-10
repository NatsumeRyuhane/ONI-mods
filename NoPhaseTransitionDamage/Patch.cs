using HarmonyLib;
using KMod;

namespace NoPhaseTransitionDamage
{
    public class Patch : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
        }
        
        [HarmonyPatch(typeof(Conduit))]
        [HarmonyPatch("OnConduitFrozen")]
        public static class Conduit_OnConduitFrozen_Patch
        {
            public static bool Prefix(ref Conduit __instance)
            {
                return false;
            }
        }
        
        [HarmonyPatch(typeof(Conduit))]
        [HarmonyPatch("OnConduitBoiling")]
        public static class Conduit_OnConduitBoiling_Patch
        {
            public static bool Prefix(ref Conduit __instance)
            {
                return false;
            }
        }
    }
}