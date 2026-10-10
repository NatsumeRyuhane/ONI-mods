using HarmonyLib;
using UnityEngine;

namespace FastTempShiftPlateBuild
{
    public static class Patch
    {
        // Vanilla: 120 s to build, 30 s to deconstruct, 800 kg of material.
        private const float BuildSeconds = 6f;

        // Half the build time, the same rule the game uses for tile pieces.
        private const float DeconstructSeconds = BuildSeconds * 0.5f;

        private const float MaterialKg = 500f;

        [HarmonyPatch(typeof(ThermalBlockConfig), nameof(ThermalBlockConfig.CreateBuildingDef))]
        public static class ThermalBlockConfig_CreateBuildingDef_Patch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.Mass = new[] { MaterialKg };
                __result.ConstructionTime = BuildSeconds;
            }
        }

        // Thermal Blocks are not tile pieces, so Deconstructable hard-codes their deconstruct time to 30 s
        // and ignores ConstructionTime. customWorkTime overrides that for the prefab.
        [HarmonyPatch(typeof(ThermalBlockConfig), nameof(ThermalBlockConfig.DoPostConfigureComplete))]
        public static class ThermalBlockConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(GameObject go)
            {
                var deconstructable = go.GetComponent<Deconstructable>();
                if (deconstructable != null)
                {
                    deconstructable.customWorkTime = DeconstructSeconds;
                }
            }
        }
    }
}
