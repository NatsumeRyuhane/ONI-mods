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
                Strings.Add(key + ".EFFECT", "Keeps an adjustable amount of "
                    + UI.FormatAsLink("Radiation", "RADIATION") + ", up to " + config.MaxRadsPerCell.ToString("F0")
                    + " rads, in each cell of a 4x3 area: its own cell, the cells beside it and the 3x3 area in front of it."
                    + (config.UraniumOreMgPerRad > 0f
                        ? "\n\nUses " + config.UraniumOreMgPerRad.ToString("0.##") + " mg/s of "
                            + UI.FormatAsLink("Uranium Ore", "URANIUMORE") + " for each rad."
                        : "")
                    + (config.AdjustableDirection
                        ? "\n\nIts direction can be changed after it's built. Pointed diagonally, it covers the 3x3 area off its corner."
                        : ""));
                Strings.Add(RadiationFieldLamp.SliderTitleKeyString, "Radiation per Cell");
                Strings.Add(RadiationFieldLamp.SliderTooltipKeyString,
                    "The lamp keeps <b>{0:F0} rads</b> in each cell of its field. Fuel use scales with it.");
                ModUtil.AddBuildingToPlanScreen("HEP", RadiationFieldLampConfig.ID, "producers", RadiationLightConfig.ID);
                Db.Get().Techs.TryGet("Agriculture")?.unlockedItemIDs.Add(RadiationFieldLampConfig.ID);
            }
        }

        // With the Adjustable Direction option, reuse the Radbolt Reflector's direction side screen. It only accepts
        // HEP buildings, so let it accept the lamp too. The lamp implements IHighEnergyParticleDirection, which is all
        // the screen uses.
        [HarmonyPatch(typeof(HighEnergyParticleDirectionSideScreen),
            nameof(HighEnergyParticleDirectionSideScreen.IsValidForTarget))]
        public static class HighEnergyParticleDirectionSideScreen_IsValidForTarget_Patch
        {
            public static void Postfix(GameObject target, ref bool __result)
            {
                if (!__result && SingletonOptions<Config>.Instance.AdjustableDirection
                    && target.GetComponent<RadiationFieldLamp>() != null)
                {
                    __result = true;
                }
            }
        }
    }
}
