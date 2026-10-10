using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace ThermallyConductive
{
    public static class Patch
    {
        // BuildingDef.ThermalConductivity scales how fast a building exchanges heat with its cell.
        private const float ConductivityMultiplier = 1000f;

        // Buildings whose job is to move heat: radiant pipes (vanilla 2) and Temperature Shift Plates (vanilla 1).
        private static readonly Type[] ConfigTypes =
        {
            typeof(ThermalBlockConfig),
            typeof(LiquidConduitRadiantConfig),
            typeof(GasConduitRadiantConfig)
        };

        [HarmonyPatch]
        public static class CreateBuildingDef_Patch
        {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                return ConfigTypes.Select(type => (MethodBase)AccessTools.Method(type, nameof(IBuildingConfig.CreateBuildingDef)));
            }

            public static void Postfix(BuildingDef __result)
            {
                __result.ThermalConductivity *= ConductivityMultiplier;
            }
        }
    }
}
