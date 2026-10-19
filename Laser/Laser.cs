using System.Collections.Generic;
using KSerialization;
using STRINGS;
using UnityEngine;

namespace Laser
{
    // Points the LASER's light the way it was placed and sets its brightness from a slider. The beam runs to the edge
    // of the LASER's world, and the light stops it at the first solid tile. The power draw follows the brightness, and
    // all of it becomes heat in the building, which the def can't do per building.
    [SerializationConfig(MemberSerialization.OptIn)]
    public class Laser : KMonoBehaviour, ISingleSliderControl, ISim1000ms
    {
        public const string SliderTitleKeyString = "STRINGS.UI.UISIDESCREENS.LASERLIGHT.TITLE";

        public const string SliderTooltipKeyString = "STRINGS.UI.UISIDESCREENS.LASERLIGHT.TOOLTIP";

        public const string SliderPowerTooltipKeyString = "STRINGS.UI.UISIDESCREENS.LASERLIGHT.TOOLTIP_POWER";

        // The slider moves in steps this size, so it lands on round numbers, and starts at one step. At 0 lux the
        // LASER would still look lit and draw its def's power.
        public const int LuxStep = 1000;

        // Light2D also sizes its glow by its range, which would draw a cone across the whole world. Patch.cs draws
        // the glow of every light in this set at this range instead.
        public const float GlowRange = 4f;

        public static readonly HashSet<Light2D> Lights = new HashSet<Light2D>();

        private static readonly EventSystem.IntraObjectHandler<Laser> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<Laser>((component, data) => component.OnCopySettings(data));

        // The game assigns [MyCmpGet] fields by reflection, which the compiler can't see.
#pragma warning disable CS0649
        [MyCmpGet]
        private Light2D light;

        [MyCmpGet]
        private Operational operational;

        [MyCmpGet]
        private EnergyConsumer energyConsumer;

        [MyCmpGet]
        private Rotatable rotatable;
#pragma warning restore CS0649

        // Brightness in lux. Negative until the LASER spawns for the first time, which then uses the default.
        [Serialize]
        private int lux = -1;

        private HandleVector<int>.Handle structureTemperature;

        public int Lux
        {
            get { return lux; }
            set
            {
                lux = Mathf.Clamp(value, LuxStep, LaserConfig.MaxLux);
                if (isSpawned)
                {
                    ApplyLux();
                }
            }
        }

        public float Watts
        {
            get { return lux * LaserConfig.WattsPerLux; }
        }

        public string SliderTitleKey
        {
            get { return SliderTitleKeyString; }
        }

        public string SliderUnits
        {
            get { return UI.UNITSUFFIXES.LIGHT.LUX; }
        }

        public int SliderDecimalPlaces(int index)
        {
            return 0;
        }

        public float GetSliderMin(int index)
        {
            return LuxStep;
        }

        public float GetSliderMax(int index)
        {
            return LaserConfig.MaxLux;
        }

        public float GetSliderValue(int index)
        {
            return lux;
        }

        public void SetSliderValue(float value, int index)
        {
            Lux = Mathf.RoundToInt(value / LuxStep) * LuxStep;
        }

        public string GetSliderTooltipKey(int index)
        {
            return SliderTooltipKeyString;
        }

        public string GetSliderTooltip(int index)
        {
            string tooltip = string.Format(Strings.Get(SliderTooltipKeyString), GameUtil.GetFormattedLux(lux));
            if (LaserConfig.WattsPerLux > 0f)
            {
                tooltip += string.Format(Strings.Get(SliderPowerTooltipKeyString),
                    GameUtil.GetFormattedWattage(Watts));
            }
            return tooltip;
        }

        // The Ceiling Light art points down when unrotated, and each rotation step turns it 90 degrees clockwise.
        public static DiscreteShadowCaster.Direction GetDirection(Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.R90: return DiscreteShadowCaster.Direction.West;
                case Orientation.R180: return DiscreteShadowCaster.Direction.North;
                case Orientation.R270: return DiscreteShadowCaster.Direction.East;
                default: return DiscreteShadowCaster.Direction.South;
            }
        }

        // Cells from the origin to the edge of its world in the direction, counting the origin. The light's scan
        // only stops at solid tiles, and past the side of the grid it would wrap onto the next row, so the world
        // edge has to bound it.
        public static int GetBeamLength(int cell, DiscreteShadowCaster.Direction direction)
        {
            if (!Grid.IsValidCell(cell) || Grid.WorldIdx[cell] == byte.MaxValue)
            {
                return 1;
            }
            WorldContainer world = ClusterManager.Instance.GetWorld(Grid.WorldIdx[cell]);
            if (world == null)
            {
                return 1;
            }
            Vector2I xy = Grid.CellToXY(cell);
            Vector2 min = world.minimumBounds;
            Vector2 max = world.maximumBounds;
            int length;
            switch (direction)
            {
                case DiscreteShadowCaster.Direction.North: length = (int)max.y - xy.y + 1; break;
                case DiscreteShadowCaster.Direction.East: length = (int)max.x - xy.x + 1; break;
                case DiscreteShadowCaster.Direction.West: length = xy.x - (int)min.x + 1; break;
                default: length = xy.y - (int)min.y + 1; break;
            }
            return Mathf.Max(length, 1);
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            structureTemperature = GameComps.StructureTemperatures.GetHandle(gameObject);
            lux = Mathf.Clamp(lux < 0 ? LaserConfig.DefaultLux : lux, LuxStep, LaserConfig.MaxLux);
            DiscreteShadowCaster.Direction direction =
                GetDirection(rotatable != null ? rotatable.GetOrientation() : Orientation.Neutral);
            light.LightDirection = direction;
            light.Range = GetBeamLength(Grid.PosToCell(transform.GetPosition() + (Vector3)light.Offset), direction);
            Lights.Add(light);
            Vector2I forward = DiscreteShadowCaster.DirectionToVector(direction);
            light.Direction = new Vector2(forward.x, forward.y);
            ApplyLux();
        }

        protected override void OnCleanUp()
        {
            Lights.Remove(light);
            base.OnCleanUp();
        }

        private void OnCopySettings(object data)
        {
            GameObject source = data as GameObject;
            Laser other = source != null ? source.GetComponent<Laser>() : null;
            if (other != null)
            {
                Lux = other.Lux;
            }
        }

        private void ApplyLux()
        {
            light.Lux = lux;
            if (energyConsumer != null)
            {
                energyConsumer.BaseWattageRating = Watts;
            }
            // FullRefresh does nothing while the light is off. It picks the new state up when it turns on.
            light.FullRefresh();
        }

        public void Sim1000ms(float dt)
        {
            if (operational.IsActive && Watts > 0f && structureTemperature.IsValid())
            {
                GameComps.StructureTemperatures.ProduceEnergy(structureTemperature, Watts / 1000f * dt,
                    BUILDING.STATUSITEMS.OPERATINGENERGY.OPERATING, dt);
            }
        }
    }
}
