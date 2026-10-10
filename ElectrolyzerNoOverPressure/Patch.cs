using HarmonyLib;

namespace ElectrolyzerNoOverPressure
{
    public static class Patch
    {
        // The Electrolyzer's state machine leaves its converting state for the overpressure state whenever
        // RoomForPressure is false, which happens when the gas around the output cell is above its maximum pressure.
        // Reporting that there is always room keeps it converting. RoomForPressure is private, so it's named by string.
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
