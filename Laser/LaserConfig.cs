using PeterHan.PLib.Options;
using TUNING;
using UnityEngine;

namespace Laser
{
    // A Ceiling Light that can be built anywhere, points up, down, left or right, and lights a straight line of cells
    // with no falloff, up to the first solid tile or the edge of the world. It uses the game's Quad light shape with width 1. Each LASER sets its own brightness with a
    // slider and draws power in proportion to it.
    public class LaserConfig : IBuildingConfig
    {
        public const string ID = "LaserLight";

        // Red, a little brighter than the vanilla light colours so the glow reads as a beam.
        public static readonly Color LightColor = new Color(1f, 0.25f, 0.25f, 1f);

        // The middle of the LASER's cell. Light2D doesn't rotate its offset, so it must sit on the pivot.
        public static readonly Vector2 LightOffset = new Vector2(0f, 0.5f);

        public static int MaxLux
        {
            get { return Mathf.Max(SingletonOptions<Config>.Instance.MaxLux, Laser.LuxStep); }
        }

        public static int DefaultLux
        {
            get { return Mathf.Clamp(SingletonOptions<Config>.Instance.DefaultLux, Laser.LuxStep, MaxLux); }
        }

        public static float WattsPerLux
        {
            get { return SingletonOptions<Config>.Instance.WattsPer100kLux / 100000f; }
        }

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 1, 1, "ceilinglight_kanim", 10, 10f,
                BUILDINGS.CONSTRUCTION_MASS_KG.TIER1, MATERIALS.ALL_METALS, 800f, BuildLocationRule.Anywhere,
                noise: NOISE_POLLUTION.NONE, decor: BUILDINGS.DECOR.NONE);
            // The build menu shows the power at the default brightness. Each LASER sets its own from its slider, and
            // makes its own heat to match, so the def has none.
            float watts = DefaultLux * WattsPerLux;
            def.RequiresPowerInput = WattsPerLux > 0f;
            def.EnergyConsumptionWhenActive = watts;
            def.SelfHeatKilowattsWhenActive = 0f;
            def.ExhaustKilowattsWhenActive = 0f;
            def.ViewMode = OverlayModes.Light.ID;
            def.AudioCategory = "Metal";
            def.PermittedRotations = PermittedRotations.R360;
            return def;
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            go.AddOrGet<LaserPreview>();
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.GetComponent<KPrefabID>().AddTag(GameTags.LightSource);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<LoopingSounds>();
            Light2D light = go.AddOrGet<Light2D>();
            light.overlayColour = LIGHT2D.CEILINGLIGHT_OVERLAYCOLOR;
            light.Color = LightColor;
            // Each LASER sets its range from where it is built.
            light.Range = 1f;
            light.Angle = LIGHT2D.CEILINGLIGHT_ANGLE;
            light.Direction = LIGHT2D.DEFAULT_DIRECTION;
            light.Offset = LightOffset;
            light.shape = LightShape.Quad;
            light.Width = 1;
            light.LightDirection = DiscreteShadowCaster.Direction.South;
            light.FalloffRate = 0f;
            light.drawOverlay = true;
            light.Lux = DefaultLux;
            go.AddOrGetDef<LightController.Def>();
            go.AddOrGet<CopyBuildingSettings>();
            go.AddOrGet<Laser>();
        }
    }
}
