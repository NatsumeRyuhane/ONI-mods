using HarmonyLib;
using PeterHan.PLib.Options;
using STRINGS;
using UnityEngine;

namespace Laser
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
                string key = "STRINGS.BUILDINGS.PREFABS." + LaserConfig.ID.ToUpperInvariant();
                Strings.Add(key + ".NAME", UI.FormatAsLink("LASER", LaserConfig.ID));
                Strings.Add(key + ".DESC", "Light Amplification by Stimulated Emission of Radiation. Mostly the light part.");
                Strings.Add(key + ".EFFECT", "Lights a straight line of " + LaserConfig.BeamLength
                    + " cells, starting at its own, in the direction it was placed, with an adjustable "
                    + UI.FormatAsLink("Light", "LIGHT") + " level up to " + GameUtil.GetFormattedLux(LaserConfig.MaxLux)
                    + ". Every cell gets the full brightness. Solid tiles stop the beam; windows don't."
                    + (LaserConfig.WattsPerLux > 0f
                        ? "\n\nUses " + GameUtil.GetFormattedWattage(LaserConfig.WattsPerLux * 100000f)
                            + " of " + UI.FormatAsLink("Power", "POWER") + " for each "
                            + GameUtil.GetFormattedLux(100000) + "."
                        : "")
                    + "\n\nCan be built anywhere, facing up, down, left or right.");
                Strings.Add(Laser.SliderTitleKeyString, "Brightness");
                Strings.Add(Laser.SliderTooltipKeyString, "The beam lights each of its {1} cells at <b>{0}</b>.");
                Strings.Add(Laser.SliderPowerTooltipKeyString, "\n\nIt draws <b>{0}</b> at this brightness.");
                ModUtil.AddBuildingToPlanScreen("Furniture", LaserConfig.ID, "lights", CeilingLightConfig.ID);
                Db.Get().Techs.TryGet("InteriorDecor")?.unlockedItemIDs.Add(LaserConfig.ID);
            }
        }

        // The game's Quad light only ever points down in vanilla, and its partitioner extents are wrong for left and
        // right: they cover a column instead of a row. A tile built or dug in a sideways beam would then not refresh
        // it. Give sideways Quad lights the row their beam covers.
        [HarmonyPatch(typeof(Light2D), "ComputeExtents")]
        public static class Light2D_ComputeExtents_Patch
        {
            public static void Postfix(Light2D __instance, ref Extents __result)
            {
                DiscreteShadowCaster.Direction direction = __instance.LightDirection;
                if (__instance.shape != LightShape.Quad || (direction != DiscreteShadowCaster.Direction.East
                    && direction != DiscreteShadowCaster.Direction.West))
                {
                    return;
                }
                Vector2I origin = Grid.CellToXY(Grid.PosToCell(__instance.transform.GetPosition()
                    + (Vector3)__instance.Offset));
                int width = __instance.Width;
                int range = (int)__instance.Range;
                // The same split ScanQuad uses for the cells on each side of the origin.
                int behind = width % 2 == 0 ? width / 2 - 1 : (width - 1) / 2;
                int x = direction == DiscreteShadowCaster.Direction.East ? origin.x : origin.x - range + 1;
                __result = new Extents(x, origin.y - behind, range, width);
            }
        }
    }
}
