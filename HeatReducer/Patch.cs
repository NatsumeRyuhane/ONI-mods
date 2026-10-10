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
        
        // Buildings the mod leaves alone, because their heat is part of what they do.
        private static readonly HashSet<Type> UntouchedConfigTypes = new HashSet<Type>
        {
            // Temperature regulation: heaters, coolers, heat pumps, ice and temperature-held storage
            typeof(SpaceHeaterConfig),
            typeof(LiquidHeaterConfig),
            typeof(LiquidCooledFanConfig),
            typeof(IceCooledFanConfig),
            typeof(AirConditionerConfig),
            typeof(LiquidConditionerConfig),
            typeof(MassiveHeatSinkConfig),
            typeof(HeatCompressorConfig),
            typeof(IceMachineConfig),
            typeof(IceKettleConfig),
            typeof(RefrigeratorConfig),
            typeof(MiniFridgeConfig),
            typeof(EggIncubatorConfig),
            // Heat to power
            typeof(SteamTurbineConfig),
            typeof(SteamTurbineConfig2),
            // Industrial machines with 8 kW or more of total heat (self + exhaust). Metal Refinery also has a coolant module.
            typeof(MetalRefineryConfig),
            typeof(GlassForgeConfig),
            typeof(KilnConfig),
            typeof(SupermaterialRefineryConfig),
            typeof(RubberMakerConfig),
            typeof(DiamondPressConfig),
            typeof(PolymerizerConfig),
            typeof(OilRefineryConfig),
            typeof(OxyliteRefineryConfig),
            typeof(RockCrusherConfig),
            typeof(MissileFabricatorConfig),
            typeof(DesalinatorConfig)
        };

        public static List<Type> GetBuildingConfigTypes()
        {
            // The game registers every non-abstract IBuildingConfig in Assembly-CSharp (GeneratedBuildings.LoadGeneratedBuildings).
            var targetBuildingConfigTypes = new List<Type>();
            foreach (var type in typeof(BuildingConfigManager).Assembly.GetTypes())
            {
                if (typeof(IBuildingConfig).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface &&
                    !UntouchedConfigTypes.Contains(type))
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
            // Buildings with no power draw keep their vanilla heat.
            var powerKw = __result.EnergyConsumptionWhenActive / 1000f;
            if (powerKw <= 0) return;

            // Negative values are cooling, not heat to cap.
            var selfKw = __result.SelfHeatKilowattsWhenActive;
            var exhaustKw = __result.ExhaustKilowattsWhenActive;
            if (selfKw < 0 || exhaustKw < 0) return;

            var totalKw = selfKw + exhaustKw;
            if (totalKw <= powerKw) return;

            // Cap the total at the power draw and keep the vanilla self and exhaust split.
            var scale = powerKw / totalKw;
            HeatReducer.Log("Reducing {" + __result.Name + "} heat generation: " + totalKw * 1000 + " W -> " +
                            powerKw * 1000 + " W");
            __result.SelfHeatKilowattsWhenActive = selfKw * scale;
            __result.ExhaustKilowattsWhenActive = exhaustKw * scale;
        }
    }
}