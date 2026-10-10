using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using KMod;


namespace HeatReducer
{
    public class HeatReducer : UserMod2
    {
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

        // BuildingConfigManager keeps every registered config's BuildingDef in this private table.
        private static readonly FieldInfo ConfigTableField = AccessTools.Field(typeof(BuildingConfigManager), "configTable");

        public static void Log(string msg)
        {
            Debug.Log("[HeatReducer] " + msg);
        }

        public override void OnLoad(Harmony harmony)
        {
            HeatReducer.Log("Loaded");

            // Patch one method, RegisterBuilding, instead of each config's CreateBuildingDef. Patching a config's
            // CreateBuildingDef runs that config's static constructor during mod load. Some of those constructors call
            // Db.Get() before the database is ready, which breaks building registration for the whole game.
            var registerBuilding = AccessTools.Method(typeof(BuildingConfigManager), nameof(BuildingConfigManager.RegisterBuilding));
            var postfix = new HarmonyMethod(typeof(HeatReducer).GetMethod(nameof(RegisterBuildingPostfix)));
            harmony.Patch(registerBuilding, null, postfix);

            HeatReducer.Log("Complete");
        }

        // Runs after the config's CreateBuildingDef has finished, so the BuildingDef holds its final values.
        public static void RegisterBuildingPostfix(IBuildingConfig config)
        {
            if (UntouchedConfigTypes.Contains(config.GetType())) return;

            var configTable = (Dictionary<IBuildingConfig, BuildingDef>)ConfigTableField.GetValue(BuildingConfigManager.Instance);
            BuildingDef buildingDef;
            // A config is missing from the table when its DLC is not owned.
            if (!configTable.TryGetValue(config, out buildingDef)) return;

            CapHeatToPowerDraw(buildingDef);
        }

        public static void CapHeatToPowerDraw(BuildingDef buildingDef)
        {
            // Buildings with no power draw keep their vanilla heat.
            var powerKw = buildingDef.EnergyConsumptionWhenActive / 1000f;
            if (powerKw <= 0) return;

            // Negative values are cooling, not heat to cap.
            var selfKw = buildingDef.SelfHeatKilowattsWhenActive;
            var exhaustKw = buildingDef.ExhaustKilowattsWhenActive;
            if (selfKw < 0 || exhaustKw < 0) return;

            var totalKw = selfKw + exhaustKw;
            if (totalKw <= powerKw) return;

            // Cap the total at the power draw and keep the vanilla self and exhaust split.
            var scale = powerKw / totalKw;
            HeatReducer.Log("Reducing {" + buildingDef.Name + "} heat generation: " + totalKw * 1000 + " W -> " +
                            powerKw * 1000 + " W");
            buildingDef.SelfHeatKilowattsWhenActive = selfKw * scale;
            buildingDef.ExhaustKilowattsWhenActive = exhaustKw * scale;
        }
    }
}
