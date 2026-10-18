using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace SolarLuxScaling
{
    // Vanilla Solar Panels reach their 380 W cap at 717,000 lux summed over their 14 light cells. A mod that raises the
    // cap leaves the watts per lux alone, so the panel needs far more light to use it. This scales the panel's output by
    // cap / 380, so the same light that gave 380 W now gives the new cap.
    public static class Patch
    {
        // SolarPanel.EnergySim200ms ends its lux sum with "watts = Mathf.Clamp(watts, 0, cap)". The call is swapped
        // for ScaledClamp, which takes the same arguments, so it scales by whatever cap reaches it at runtime: the
        // vanilla literal, another mod's replacement constant, or a value another mod reads from its own settings.
        // A 380 W cap scales by 1, so without such a mod nothing changes.
        //
        // Harmony rebuilds the method from every mod's transpilers in priority order whenever a patch is added, so
        // Priority.Last runs this after the others no matter which mod loads first, and they still find the
        // vanilla Clamp call to edit.
        [HarmonyPatch(typeof(SolarPanel), nameof(SolarPanel.EnergySim200ms))]
        public static class SolarPanel_EnergySim200ms_Patch
        {
            private static readonly MethodInfo Clamp =
                AccessTools.Method(typeof(Mathf), nameof(Mathf.Clamp), new[] { typeof(float), typeof(float), typeof(float) });

            [HarmonyPriority(Priority.Last)]
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                bool replaced = false;
                foreach (CodeInstruction instruction in instructions)
                {
                    if (!replaced && instruction.Calls(Clamp))
                    {
                        instruction.operand = AccessTools.Method(typeof(SolarPanel_EnergySim200ms_Patch), nameof(ScaledClamp));
                        replaced = true;
                    }
                    yield return instruction;
                }
                if (!replaced)
                {
                    Debug.LogWarning("[SolarLuxScaling] No Mathf.Clamp call in SolarPanel.EnergySim200ms, "
                        + "so Solar Panel output isn't scaled");
                }
            }

            public static float ScaledClamp(float watts, float min, float max)
            {
                return Mathf.Clamp(watts * max / SolarPanelConfig.MAX_WATTS, min, max);
            }
        }
    }
}
