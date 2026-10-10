using HarmonyLib;

namespace NoPhaseTransitionDamage
{
    // The sim reports pipe contents that freeze or boil, and ConduitFlow raises an event on the pipe. Conduit's
    // handlers for those events deal 1 damage to the pipe and empty its contents. Skipping the handlers removes both.
    // The handlers are private, so they're named by string.
    public static class Patch
    {
        [HarmonyPatch(typeof(Conduit), "OnConduitFrozen")]
        public static class Conduit_OnConduitFrozen_Patch
        {
            public static bool Prefix()
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(Conduit), "OnConduitBoiling")]
        public static class Conduit_OnConduitBoiling_Patch
        {
            public static bool Prefix()
            {
                return false;
            }
        }
    }
}
