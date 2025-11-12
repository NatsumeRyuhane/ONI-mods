using System;
using System.Collections.Generic;
using HarmonyLib;
using KMod;


namespace HeatReducer
{
    public class HeatReducer : UserMod2
    {
        public static void Log(string msg)
        {
            Debug.Log("[HeatReducer] " + msg);
        }
        
        public static List<Type> GetBuildingConfigTypes()
        {
            var targetBuildingConfigTypes = new List<Type>
            {
                // Base
                // Oxygen
                // Production
                typeof(MineralDeoxidizerConfig),
                typeof(SublimationStationConfig),
                typeof(ElectrolyzerConfig),
                typeof(RustDeoxidizerConfig),
                // Purification
                typeof(AlgaeHabitatConfig),
                // Power
                // Generators
                typeof(ManualGeneratorConfig),
                typeof(HydrogenGeneratorConfig),
                typeof(PetroleumGeneratorConfig),
                // Batteries
                typeof(BatteryConfig),
                typeof(BatteryMediumConfig),
                typeof(BatterySmartConfig),
                // Food
                // Storage
                typeof(RefrigeratorConfig),
                // Cooking
                typeof(MicrobeMusherConfig),
                typeof(CookingStationConfig),
                // Plumbing
                // Pumps
                typeof(LiquidPumpConfig),
                // Valves
                typeof(LiquidFilterConfig),
                typeof(LiquidValveConfig),
                // Ventilation
                // Pumps
                typeof(GasPumpConfig),
                // Valves
                typeof(GasFilterConfig),
                typeof(GasValveConfig),
                // Refinement
                // Organic
                typeof(CompostConfig),
                typeof(AlgaeDistilleryConfig),
                typeof(EthanolDistilleryConfig),
                typeof(SludgePressConfig),
                // Materials
                typeof(WaterPurifierConfig),
                typeof(RockCrusherConfig),
                typeof(MetalRefineryConfig),
                typeof(PolymerizerConfig),
                // Medicine
                // Furniture
                // Lights
                typeof(FloorLampConfig),
                typeof(CeilingLightConfig),
                // Recreation
                typeof(ArcadeMachineConfig),
                // Stations
                // Farming
                typeof(FarmStationConfig),
                // Ranching
                typeof(RanchStationConfig),
                typeof(ShearingStationConfig),
                // Research
                typeof(ResearchCenterConfig),
                typeof(AdvancedResearchCenterConfig),
                typeof(NuclearResearchCenterConfig),
                typeof(CosmicResearchCenterConfig),
                // Industrial
                typeof(PowerControlStationConfig),
                typeof(ResetSkillsStationConfig),
                // Manufacturing
                typeof(CraftingTableConfig)
            };

            return targetBuildingConfigTypes;
        }

        public override void OnLoad(Harmony harmony)
        {
            HeatReducer.Log("Loaded");
            var targetBuildingConfigTypes = GetBuildingConfigTypes();

            // apply patches
            foreach (var buildingConfigType in targetBuildingConfigTypes)
            {
                try
                {
                    var method = buildingConfigType.GetMethod("CreateBuildingDef", Type.EmptyTypes);
                    if (method != null)
                    {
                        HeatReducer.Log("Patching " + buildingConfigType.Name);
                        var postfix =
                            new HarmonyMethod(
                                typeof(HeatReducer).GetMethod("MatchBuildingHeatGenerationToPowerConsumption"));
                        harmony.Patch(method, null, postfix);
                    }

                    HeatReducer.Log("Ignored " + buildingConfigType.Name + ": no `CreateBuildingDef` method");
                }
                catch (Exception e)
                {
                    HeatReducer.Log("Failed to patch " + buildingConfigType.Name + ": " + e.Message);
                }
            }

            HeatReducer.Log("Complete");
        }

        public static void MatchBuildingHeatGenerationToPowerConsumption(BuildingDef __result)
        {
            var powerConsumption = __result.GetType().GetField("EnergyConsumptionWhenActive");
            var selfHeatGeneration = __result.GetType().GetField("SelfHeatKilowattsWhenActive");
            var exhaustHeatGeneration = __result.GetType().GetField("ExhaustKilowattsWhenActive");

            if (selfHeatGeneration != null)
            {
                if (powerConsumption == null)
                {
                    HeatReducer.Log("Reducing {" + __result.Name + "} heat generation: " +
                                    __result.SelfHeatKilowattsWhenActive * 1000 + " W -> 0 W");
                    __result.SelfHeatKilowattsWhenActive = 0;
                }
                else if (__result.SelfHeatKilowattsWhenActive * 1000 > __result.EnergyConsumptionWhenActive)
                {
                    HeatReducer.Log("Reducing {" + __result.Name + "} heat generation: " +
                                    __result.SelfHeatKilowattsWhenActive * 1000 + " W -> " +
                                    __result.EnergyConsumptionWhenActive + " W");
                    __result.SelfHeatKilowattsWhenActive = __result.EnergyConsumptionWhenActive / 1000;
                }

                if (exhaustHeatGeneration == null) return;
                HeatReducer.Log("Reducing {" + __result.Name + "} exhaust heat generation to 0");
                __result.ExhaustKilowattsWhenActive = 0;
            }
        }
    }
}