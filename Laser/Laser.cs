using KSerialization;
using STRINGS;
using UnityEngine;

namespace Laser
{
    // Points the LASER's light the way it was placed and sets its brightness from a slider. The power draw follows
    // the brightness, and all of it becomes heat in the building, which the def can't do per building.
    [SerializationConfig(MemberSerialization.OptIn)]
    public class Laser : KMonoBehaviour, ISingleSliderControl, ISim1000ms
    {
        public const string SliderTitleKeyString = "STRINGS.UI.UISIDESCREENS.LASERLIGHT.TITLE";

        public const string SliderTooltipKeyString = "STRINGS.UI.UISIDESCREENS.LASERLIGHT.TOOLTIP";

        public const string SliderPowerTooltipKeyString = "STRINGS.UI.UISIDESCREENS.LASERLIGHT.TOOLTIP_POWER";

        // The slider moves in steps this size, so it lands on round numbers, and starts at one step. At 0 lux the
        // LASER would still look lit and draw its def's power.
        public const int LuxStep = 1000;

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
            string tooltip = string.Format(Strings.Get(SliderTooltipKeyString), GameUtil.GetFormattedLux(lux),
                LaserConfig.BeamLength);
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
            Vector2I forward = DiscreteShadowCaster.DirectionToVector(direction);
            light.Direction = new Vector2(forward.x, forward.y);
            ApplyLux();
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
