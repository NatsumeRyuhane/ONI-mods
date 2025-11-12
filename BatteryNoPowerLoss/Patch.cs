using System;
using System.Collections.Generic;
using HarmonyLib;
using KMod;
using STRINGS;
using UnityEngine;

namespace BatteryNoPowerLoss
{
    public class BatteryNoPowerLoss : UserMod2
    {
        public static List<Type> GetBuildingConfigTypes()
        {
            var targetBuildingConfigTypes = new List<Type>
            {
                typeof(BatteryConfig),
                typeof(BatteryMediumConfig),
                typeof(BatterySmartConfig),
            };
            return targetBuildingConfigTypes;
        }
        
        public override void OnLoad(Harmony harmony)
        {
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(BatteryConfig))]
        [HarmonyPatch("DoPostConfigureComplete")]
        public static class BatteryConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(GameObject go)
            {
                Battery b = go.AddOrGet<Battery>();
                b.joulesLostPerSecond = 0f;
                STRINGS.BUILDINGS.PREFABS.BATTERY.EFFECT = (LocString) ("Stores " + UI.FormatAsLink("Power", "POWER") + " from generators, then provides that power to buildings.");
            }
        }
        
        [HarmonyPatch(typeof(BatteryMediumConfig))]
        [HarmonyPatch("DoPostConfigureComplete")]
        public static class BatteryMediumConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(GameObject go)
            {
                Battery b = go.AddOrGet<Battery>();
                b.joulesLostPerSecond = 0f;
                STRINGS.BUILDINGS.PREFABS.BATTERYMEDIUM.EFFECT = (LocString) ("Stores " + UI.FormatAsLink("Power", "POWER") + " from generators, then provides that power to buildings.");
            }
        }


        [HarmonyPatch(typeof(BatterySmartConfig))]
        [HarmonyPatch("DoPostConfigureComplete")]
        public static class BatterySmartConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(GameObject go)
            {
                BatterySmart b = go.AddOrGet<BatterySmart>();
                b.joulesLostPerSecond = 0f;
                STRINGS.BUILDINGS.PREFABS.BATTERYSMART.EFFECT = (LocString) (LocString) ("Stores " + UI.FormatAsLink("Power", "POWER") + " from generators, then provides that power to buildings.\n\nSends a " + UI.FormatAsAutomationState("Green Signal", UI.AutomationState.Active) + " or " + UI.FormatAsAutomationState("Red Signal", UI.AutomationState.Standby) + " based on the configuration of the Logic Activation Parameters.");
            }
        }
    }
}