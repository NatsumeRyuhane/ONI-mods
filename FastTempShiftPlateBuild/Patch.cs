using System;
using HarmonyLib;
using Klei.AI;
using KMod;
using UnityEngine;


namespace FastTempShiftPlateBuild
{
    public class FastTempShiftPlateBuild
    {
        // Build time in seconds. Deconstruction takes half as long, the same rule the game uses for tile pieces.
        private const float CONSTRUCTION_TIME = 6f;
        private const float DECONSTRUCT_TIME = CONSTRUCTION_TIME * 0.5f;

        [HarmonyPatch(typeof(ThermalBlockConfig))]
        [HarmonyPatch("CreateBuildingDef")]
        public class ThermalBlockConfig_Patch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.Mass = new float[] { 500f };
                __result.ConstructionTime = CONSTRUCTION_TIME;
            }
        }

        // Thermal Blocks are not tile pieces, so Deconstructable hard-codes their deconstruct time to 30 s
        // and ignores ConstructionTime. customWorkTime overrides that for the prefab.
        [HarmonyPatch(typeof(ThermalBlockConfig), nameof(ThermalBlockConfig.DoPostConfigureComplete))]
        public class ThermalBlockConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(GameObject go)
            {
                Deconstructable deconstructable = go.GetComponent<Deconstructable>();
                if (deconstructable != null)
                {
                    deconstructable.customWorkTime = DECONSTRUCT_TIME;
                }
            }
        }
    }
}