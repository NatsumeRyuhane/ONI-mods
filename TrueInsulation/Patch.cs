using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace TrueInsulation
{
    public static class Patch
    {
        // Insulating buildings. Vanilla ThermalConductivity: 1/32 for insulated pipes, 0.01 for Insulation Tiles and 1
        // (the default) for pressure doors.
        private static readonly Type[] ConfigTypes =
        {
            typeof(InsulatedGasConduitConfig),
            typeof(InsulatedLiquidConduitConfig),
            typeof(InsulationTileConfig),
            typeof(PressureDoorConfig),
            typeof(ManualPressureDoorConfig)
        };

        // Doors don't insulate their cells through ThermalConductivity alone, so they also get InsulatorModified.
        private static readonly HashSet<string> DoorIds = new HashSet<string>
        {
            PressureDoorConfig.ID,
            ManualPressureDoorConfig.ID
        };

        [HarmonyPatch]
        public static class CreateBuildingDef_Patch
        {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                return ConfigTypes.Select(type => (MethodBase)AccessTools.Method(type, nameof(IBuildingConfig.CreateBuildingDef), Type.EmptyTypes));
            }

            public static void Postfix(BuildingDef __result)
            {
                __result.ThermalConductivity = 0f;
            }
        }

        // Adds InsulatorModified to each pressure door as it spawns, and applies it right away. OnSpawn is protected, so
        // it's named by string.
        [HarmonyPatch(typeof(BuildingComplete), "OnSpawn")]
        public static class BuildingComplete_OnSpawn_Patch
        {
            public static void Postfix(BuildingComplete __instance)
            {
                if (!DoorIds.Contains(__instance.Def.PrefabID)) return;

                __instance.gameObject.AddOrGet<InsulatorModified>().ApplyInsulation();
            }
        }
    }
}
