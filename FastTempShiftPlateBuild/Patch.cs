using System;
using HarmonyLib;
using Klei.AI;
using KMod;


namespace FastTempShiftPlateBuild
{
    public class FastTempShiftPlateBuild
    {
        [HarmonyPatch(typeof(ThermalBlockConfig))]
        [HarmonyPatch("CreateBuildingDef")]
        public class ThermalBlockConfig_Patch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.Mass = new float[] { 500f };
                __result.ConstructionTime = 6f;
            }
        }
    }
}