using HarmonyLib;
using PeterHan.PLib.Options;
using STRINGS;
using UnityEngine;

namespace RadiationFieldLamp
{
    public class Patch
    {
        // Db.Initialize runs before the game registers buildings (LegacyModMain.Load) and before Techs.PostProcess,
        // so strings, the build menu entry and the research unlock are all in place in time.
        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public static class Db_Initialize_Patch
        {
            public static void Postfix()
            {
                Config config = SingletonOptions<Config>.Instance;
                string key = "STRINGS.BUILDINGS.PREFABS." + RadiationFieldLampConfig.ID.ToUpperInvariant();
                Strings.Add(key + ".NAME", UI.FormatAsLink("Radiation Field Lamp", RadiationFieldLampConfig.ID));
                Strings.Add(key + ".DESC", "An even glow of radiation, exactly where it's pointed.");
                Strings.Add(key + ".EFFECT", "Keeps " + config.RadsPerCell.ToString("F0") + " rads of "
                    + UI.FormatAsLink("Radiation", "RADIATION") + " in each cell of the 3x3 area in front of it."
                    + "\n\nIts direction can be changed after it's built.");
                ModUtil.AddBuildingToPlanScreen("HEP", RadiationFieldLampConfig.ID, "producers", RadiationLightConfig.ID);
                Db.Get().Techs.TryGet("Agriculture")?.unlockedItemIDs.Add(RadiationFieldLampConfig.ID);
            }
        }

        // Reuse the Radbolt Reflector's direction side screen. It only accepts HEP buildings, so let it accept the
        // lamp too. The lamp implements IHighEnergyParticleDirection, which is all the screen uses.
        [HarmonyPatch(typeof(HighEnergyParticleDirectionSideScreen),
            nameof(HighEnergyParticleDirectionSideScreen.IsValidForTarget))]
        public static class HighEnergyParticleDirectionSideScreen_IsValidForTarget_Patch
        {
            public static void Postfix(GameObject target, ref bool __result)
            {
                if (!__result && target.GetComponent<RadiationFieldLamp>() != null)
                {
                    __result = true;
                }
            }
        }
    }
}
