using System;
using HarmonyLib;
using Klei.AI;
using KMod;


namespace ThermallyConductive
{
    public class Patch : UserMod2
    {
        [HarmonyPatch(typeof(ThermalBlockConfig), nameof(ThermalBlockConfig.CreateBuildingDef))]
        public class ThermalBlockConfig_Patch
        {
            public static void Postfix(BuildingDef __result)
            {
               __result.ThermalConductivity = __result.ThermalConductivity * 1000;
            }
        }
        
        [HarmonyPatch(typeof(LiquidConduitRadiantConfig), nameof(ThermalBlockConfig.CreateBuildingDef))]
        public class LiquidPipeConfig_Patch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.ThermalConductivity = __result.ThermalConductivity * 1000;
            }
        }
        
        [HarmonyPatch(typeof(GasConduitRadiantConfig), nameof(ThermalBlockConfig.CreateBuildingDef))]
        public class GasPipeConfig_Patch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.ThermalConductivity = __result.ThermalConductivity * 1000;
            }
        }
    }
}