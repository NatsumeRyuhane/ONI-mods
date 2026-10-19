using HarmonyLib;

namespace RoboMinerNoFoundation
{
    public static class Patch
    {
        // Vanilla builds the Robo-Miner with BuildLocationRule.OnFoundationRotatable, so BuildingDef.IsValidBuildLocation
        // wants solid tiles along whichever side is down after rotation. BuildingTemplates.CreateBuildingDef also turns on
        // ContinuouslyCheckFoundation for that rule, which makes a placed miner shut off with a Missing Foundation status
        // once its floor is dug out. Switching to Anywhere and clearing the flag lifts both. PermittedRotations is separate
        // from the rule, so the miner still rotates freely.
        [HarmonyPatch(typeof(AutoMinerConfig), nameof(AutoMinerConfig.CreateBuildingDef))]
        public static class AutoMinerConfig_CreateBuildingDef_Patch
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.BuildLocationRule = BuildLocationRule.Anywhere;
                __result.ContinuouslyCheckFoundation = false;
            }
        }
    }
}
