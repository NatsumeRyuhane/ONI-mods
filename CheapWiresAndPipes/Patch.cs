using System;
using System.Collections.Generic;
using HarmonyLib;
using KMod;


namespace CheapWiresAndPipes
{
    public class CheapWiresAndPipes : UserMod2
    {
        private static void Log(string msg)
        {
            Debug.Log("[CheapWiresAndPipes] " + msg);
        }

        private static List<Type> GetBuildingConfigTypes()
        {
            var targetBuildingConfigTypes = new List<Type>
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

            return targetBuildingConfigTypes;
        }

        public override void OnLoad(Harmony harmony)
        {
            CheapWiresAndPipes.Log("Loaded");
            var targetBuildingConfigTypes = GetBuildingConfigTypes();

            // apply patches
            foreach (var buildingConfigType in targetBuildingConfigTypes)
            {
                try
                {
                    var method = buildingConfigType.GetMethod("CreateBuildingDef", Type.EmptyTypes);
                    if (method != null)
                    {
                        CheapWiresAndPipes.Log("Patching " + buildingConfigType.Name);
                        var postfix =
                            new HarmonyMethod(
                                typeof(CheapWiresAndPipes).GetMethod("BuildingMassReduction"));
                        harmony.Patch(method, null, postfix);
                    }

                    CheapWiresAndPipes.Log("Ignored " + buildingConfigType.Name + ": no `CreateBuildingDef` method");
                }
                catch (Exception e)
                {
                    CheapWiresAndPipes.Log("Failed to patch " + buildingConfigType.Name + ": " + e.Message);
                }
            }

            CheapWiresAndPipes.Log("Complete");
        }

        public static void BuildingMassReduction(BuildingDef __result)
        {
            const float newConstructionCost = 1f;
            CheapWiresAndPipes.Log("Reducing mass of {" + __result.Name + "} to " + newConstructionCost + "kg");
            __result.Mass = new float[1] { newConstructionCost };
            __result.BaseDecor = 0;
        }
    }
}