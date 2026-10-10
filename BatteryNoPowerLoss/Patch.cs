using HarmonyLib;
using STRINGS;
using UnityEngine;

namespace BatteryNoPowerLoss
{
    // Sets each battery's joulesLostPerSecond to 0 (vanilla: 1.67 J/s Battery, 3.33 J/s Jumbo Battery, 0.67 J/s Smart
    // Battery), and drops the "loses charge over time" sentence from its build menu description.
    public static class Patch
    {
        private static string StoresPowerText =>
            "Stores " + UI.FormatAsLink("Power", "POWER") + " from generators, then provides that power to buildings.";

        // BuildingDef.Effect reads the Strings table, which LegacyModMain fills from the STRINGS.BUILDINGS.PREFABS fields
        // just before buildings are registered. These postfixes run after that, so they write to the table directly.
        private static void SetEffect(string prefabId, string text)
        {
            Strings.Add("STRINGS.BUILDINGS.PREFABS." + prefabId.ToUpperInvariant() + ".EFFECT", text);
        }

        [HarmonyPatch(typeof(BatteryConfig), nameof(BatteryConfig.DoPostConfigureComplete))]
        public static class BatteryConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(GameObject go)
            {
                go.AddOrGet<Battery>().joulesLostPerSecond = 0f;
                SetEffect(BatteryConfig.ID, StoresPowerText);
            }
        }

        [HarmonyPatch(typeof(BatteryMediumConfig), nameof(BatteryMediumConfig.DoPostConfigureComplete))]
        public static class BatteryMediumConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(GameObject go)
            {
                go.AddOrGet<Battery>().joulesLostPerSecond = 0f;
                SetEffect(BatteryMediumConfig.ID, StoresPowerText);
            }
        }

        [HarmonyPatch(typeof(BatterySmartConfig), nameof(BatterySmartConfig.DoPostConfigureComplete))]
        public static class BatterySmartConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(GameObject go)
            {
                go.AddOrGet<BatterySmart>().joulesLostPerSecond = 0f;
                SetEffect(BatterySmartConfig.ID, StoresPowerText + "\n\nSends a "
                    + UI.FormatAsAutomationState("Green Signal", UI.AutomationState.Active) + " or "
                    + UI.FormatAsAutomationState("Red Signal", UI.AutomationState.Standby)
                    + " based on the configuration of the Logic Activation Parameters.");
            }
        }
    }
}
