using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace CheapWiresAndPipes
{
    public static class Patch
    {
        private const float MaterialKg = 1f;

        // Wires, pipes, rails and their bridges. Configs that inherit one of these methods (such as the Insulated
        // Conductive Wire Bridge) are changed too, because they call the patched method.
        private static readonly Type[] ConfigTypes =
        {
            typeof(WireConfig),
            typeof(WireHighWattageConfig),
            typeof(WireBridgeConfig),
            typeof(WireBridgeHighWattageConfig),
            typeof(WireRefinedConfig),
            typeof(WireRefinedHighWattageConfig),
            typeof(WireRefinedBridgeConfig),
            typeof(WireRefinedBridgeHighWattageConfig),
            typeof(LiquidConduitConfig),
            typeof(InsulatedLiquidConduitConfig),
            typeof(LiquidConduitRadiantConfig),
            typeof(LiquidConduitBridgeConfig),
            typeof(GasConduitConfig),
            typeof(InsulatedGasConduitConfig),
            typeof(GasConduitRadiantConfig),
            typeof(GasConduitBridgeConfig),
            typeof(SolidConduitConfig),
            typeof(LogicWireConfig)
        };

        [HarmonyPatch]
        public static class CreateBuildingDef_Patch
        {
            public static IEnumerable<MethodBase> TargetMethods()
            {
                return ConfigTypes.Select(type => (MethodBase)AccessTools.Method(type, nameof(IBuildingConfig.CreateBuildingDef), Type.EmptyTypes));
            }

            // Every listed building takes a single construction material, so one mass entry replaces the vanilla one.
            // Decor is set to 0 as well, which removes the decor penalty many of them have (-5 to -25 in vanilla).
            public static void Postfix(BuildingDef __result)
            {
                ModLoad.Log("Reducing mass of {" + __result.Name + "} to " + MaterialKg + "kg");
                __result.Mass = new[] { MaterialKg };
                __result.BaseDecor = 0;
            }
        }
    }
}
