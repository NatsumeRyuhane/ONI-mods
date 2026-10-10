using System;
using System.Collections.Generic;
using HarmonyLib;
using KMod;
using UnityEngine;

namespace ElectrolyzerNoOverPressure
{
    public class Patch : UserMod2
    {
        [HarmonyPatch(typeof(Electrolyzer), "RoomForPressure", MethodType.Getter)]
        public static class Electrolyzer_RoomForPressure_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                __result = true;
                return false;
            }
        }
    }
}