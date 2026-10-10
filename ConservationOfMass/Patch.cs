using System.Collections.Generic;
using HarmonyLib;

namespace ConservationOfMass
{
    public class ConservationOfMass
    {
        // Cells whose tile a duplicant's dig order finishes. The sim reports the dug mass a frame later, through
        // WorldDamage.OnDigComplete, so a cell waits here until that report arrives.
        private static readonly HashSet<int> fullMassCells = new HashSet<int>();

        // Only Diggable.OnWorkTick calls this overload. Its tick destroys the cell once the damage reaches 1, the same
        // test WorldDamage.ApplyDamage makes, so the finishing tick is worked out here before the tick runs.
        [HarmonyPatch(typeof(Diggable), nameof(Diggable.DoDigTick), new[] { typeof(int), typeof(float) })]
        public static class Diggable_DoDigTick_Patch
        {
            public static void Prefix(int cell, float dt)
            {
                if (!Grid.Solid[cell] && !BackwallManager.HasBackwall(cell)) return;
                float amount = dt / Diggable.GetApproximateDigTime(cell);
                if (Grid.Damage[cell] + amount >= 1f) fullMassCells.Add(cell);
            }
        }

        // The game keeps half of the dug mass (mass * 0.5f). Doubling the mass first makes that half the whole tile.
        // Doubling and halving are exact for floats, so the drop equals the tile's mass.
        [HarmonyPatch(typeof(WorldDamage), nameof(WorldDamage.OnDigComplete))]
        public static class WorldDamage_OnDigComplete_Patch
        {
            public static void Prefix(int cell, ref float mass)
            {
                if (fullMassCells.Remove(cell)) mass *= 2f;
            }
        }
    }
}
