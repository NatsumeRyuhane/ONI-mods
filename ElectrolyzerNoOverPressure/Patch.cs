using System;
using System.Collections.Generic;
using HarmonyLib;
using KMod;
using UnityEngine;

namespace ElectrolyzerNoOverPressure
{
    public class Patch : UserMod2
    {
        [HarmonyPatch(typeof(Electrolyzer), "OverPressure")]
        public static class Electrolyzer_OverPressure_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                __result = false;
                return false;
            }
        }
    }
}