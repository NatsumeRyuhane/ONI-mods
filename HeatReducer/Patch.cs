using System;
using System.Collections.Generic;
using System.Reflection;
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
        
        // Buildings whose purpose is to heat, cool or move heat. Their heat output is the point of the building, so it is left alone.
        // Grouped by the game's tech tree: Temperature Modulation, HVAC and Liquid Temperature. The Anti Entropy Thermo-Nullifier and
        // Liquid Heatquilizer have no tech.
        private static readonly HashSet<Type> ThermalRegulationConfigTypes = new HashSet<Type>
        {
            // Temperature Modulation
            typeof(SpaceHeaterConfig),
            typeof(LiquidCooledFanConfig),
            typeof(IceCooledFanConfig),
            typeof(IceMachineConfig),
            typeof(IceKettleConfig),
            // HVAC
            typeof(AirConditionerConfig),
            // Liquid Temperature
            typeof(LiquidHeaterConfig),
            typeof(LiquidConditionerConfig),
            // No tech
            typeof(MassiveHeatSinkConfig),
            typeof(HeatCompressorConfig)
        };

        public static List<Type> GetBuildingConfigTypes()
        {
            // The game registers every non-abstract IBuildingConfig in Assembly-CSharp (GeneratedBuildings.LoadGeneratedBuildings).
            var targetBuildingConfigTypes = new List<Type>();
            foreach (var type in typeof(BuildingConfigManager).Assembly.GetTypes())
            {
                if (typeof(IBuildingConfig).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface &&
                    !ThermalRegulationConfigTypes.Contains(type))
                {
                    targetBuildingConfigTypes.Add(type);
                }
            }

            return targetBuildingConfigTypes;
        }

        public override void OnLoad(Harmony harmony)
        {
            HeatReducer.Log("Loaded");
            var targetBuildingConfigTypes = GetBuildingConfigTypes();
            var postfix =
                new HarmonyMethod(
                    typeof(HeatReducer).GetMethod("MatchBuildingHeatGenerationToPowerConsumption"));
            var patchedCount = 0;

            // apply patches
            foreach (var buildingConfigType in targetBuildingConfigTypes)
            {
                try
                {
                    var method = buildingConfigType.GetMethod("CreateBuildingDef",
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, Type.EmptyTypes, null);
                    if (method == null)
                    {
                        HeatReducer.Log("Ignored " + buildingConfigType.Name + ": no `CreateBuildingDef` method");
                        continue;
                    }

                    harmony.Patch(method, null, postfix);
                    patchedCount++;
                }
                catch (Exception e)
                {
                    HeatReducer.Log("Failed to patch " + buildingConfigType.Name + ": " + e.Message);
                }
            }

            HeatReducer.Log("Patched " + patchedCount + " building configs");
            HeatReducer.Log("Complete");
        }

        public static void MatchBuildingHeatGenerationToPowerConsumption(BuildingDef __result)
        {
            if (__result.SelfHeatKilowattsWhenActive > 0 &&
                __result.SelfHeatKilowattsWhenActive * 1000 > __result.EnergyConsumptionWhenActive)
            {
                HeatReducer.Log("Reducing {" + __result.Name + "} heat generation: " +
                                __result.SelfHeatKilowattsWhenActive * 1000 + " W -> " +
                                __result.EnergyConsumptionWhenActive + " W");
                __result.SelfHeatKilowattsWhenActive = __result.EnergyConsumptionWhenActive / 1000;
            }

            // Negative exhaust is cooling, so only positive exhaust is removed.
            if (__result.ExhaustKilowattsWhenActive > 0)
            {
                HeatReducer.Log("Reducing {" + __result.Name + "} exhaust heat generation to 0");
                __result.ExhaustKilowattsWhenActive = 0;
            }
        }
    }
}