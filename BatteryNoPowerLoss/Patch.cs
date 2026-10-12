using HarmonyLib;
using UnityEngine;

namespace BatteryNoPowerLoss
{
    // Sets joulesLostPerSecond to 0 on every building with a Battery component (vanilla: 1.67 J/s Battery, 3.33 J/s
    // Jumbo Battery, 0.67 J/s Smart Battery and Battery Module), and swaps the "loses charge over time" sentence in
    // its build menu description for one saying it no longer leaks.
    public static class Patch
    {
        // Longest first, so "Slightly loses..." doesn't match inside "Very slightly loses...".
        private static readonly string[] LeakSentences =
        {
            "Very slightly loses charge over time.",
            "Slightly loses charge over time.",
            "Loses charge over time.",
        };

        private const string NoLeakSentence =
            "My Duplicants have improved the schematics of this model, so it no longer leaks power as heat.";

        // Assets.AddBuildingDef is the last step of BuildingConfigManager.RegisterBuilding, after the config's
        // DoPostConfigureComplete, so this sees each battery's final leak rate, modded batteries included. Instances
        // copy the field from the prefab when they spawn.
        [HarmonyPatch(typeof(Assets), nameof(Assets.AddBuildingDef))]
        public static class Assets_AddBuildingDef_Patch
        {
            public static void Postfix(BuildingDef def)
            {
                GameObject go = def.BuildingComplete;
                // Power transformers are batteries internally, but set their own leak every tick.
                if (go == null || go.GetComponent<PowerTransformer>() != null)
                {
                    return;
                }

                bool leaked = false;
                foreach (Battery battery in go.GetComponents<Battery>())
                {
                    leaked |= battery.joulesLostPerSecond != 0f;
                    battery.joulesLostPerSecond = 0f;
                }

                if (leaked)
                {
                    ReplaceLeakSentence(def.PrefabID);
                }
            }
        }

        // BuildingDef.Effect reads the Strings table, which LegacyModMain fills from the STRINGS.BUILDINGS.PREFABS fields
        // before buildings are registered, so the table is edited directly. A translated description has no English
        // sentence to match and is left as it is.
        private static void ReplaceLeakSentence(string prefabId)
        {
            string key = "STRINGS.BUILDINGS.PREFABS." + prefabId.ToUpper() + ".EFFECT";
            if (!Strings.TryGet(key, out StringEntry entry))
            {
                return;
            }

            foreach (string sentence in LeakSentences)
            {
                if (entry.String.Contains(sentence))
                {
                    Strings.Add(key, entry.String.Replace(sentence, NoLeakSentence));
                    return;
                }
            }
        }
    }
}
