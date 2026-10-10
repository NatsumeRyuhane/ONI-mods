using HarmonyLib;

namespace ConservationOfMass
{
    public class ConservationOfMass
    {
        // Every tile that is dug out or destroyed drops through WorldDamage.OnDigComplete, which keeps half of the mass
        // (mass * 0.5f). Doubling the mass first makes that half the whole tile. Doubling and halving are exact for floats,
        // so the drop equals the mass the tile had.
        [HarmonyPatch(typeof(WorldDamage), nameof(WorldDamage.OnDigComplete))]
        public static class WorldDamage_OnDigComplete_Patch
        {
            public static void Prefix(ref float mass)
            {
                mass *= 2f;
            }
        }
    }
}
