using KSerialization;
using PeterHan.PLib.Options;
using UnityEngine;

namespace AdjustableCoolersPlus
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class AirConditionerAdjustable : KMonoBehaviour, IUserControlledCapacity
    {
        public float MIN_DELTA = SingletonOptions<Config>.Instance.MinTemperatureDelta;
        public float MAX_DELTA = SingletonOptions<Config>.Instance.MaxTemperatureDelta;
        public const float TEMP_F_SCALAR = 1.8f;
        public const float MAX_SIGNIFICANT_FIGURES_SCALAR = 10f;

        private static readonly EventSystem.IntraObjectHandler<AirConditionerAdjustable> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<AirConditionerAdjustable>(OnCopySettings);

        private static void OnCopySettings(AirConditionerAdjustable comp, object data)
        {
            comp.OnCopySettings(data);
        }

        public const string KEY = "STRINGS.UI.UISIDESCREENS.AIRCONDITIONERTEMPERATURESIDESCREEN";

        [MyCmpReq] public AirConditioner airConditioner;

        [MyCmpReq] public EnergyConsumer energyConsumer;

        [MyCmpAdd] public CopyBuildingSettings copyBuildingSettings;

        /// <summary>
        /// The value this slider is currently set to
        /// </summary>
        [Serialize] public float tempDelta = int.MaxValue;

        public float CoolingPower
        {
            get => tempDelta;
            set => tempDelta = ((int)(value * MAX_SIGNIFICANT_FIGURES_SCALAR)) / MAX_SIGNIFICANT_FIGURES_SCALAR;
        }

        public float UserMaxCapacity
        {
            get => CoolingPower * TemperatureMultiplier;
            set => CoolingPower = value / TemperatureMultiplier;
        }

        public float TemperatureMultiplier
        {
            get
            {
                if (GameUtil.temperatureUnit == GameUtil.TemperatureUnit.Fahrenheit)
                    return TEMP_F_SCALAR;
                return 1;
            }
        }

        public float AmountStored => UserMaxCapacity;

        public float MinCapacity => MIN_DELTA * TemperatureMultiplier;

        public float MaxCapacity => 0;

        public bool WholeValues => true;

        public LocString CapacityUnits => GameUtil.GetTemperatureUnitSuffix();

        public int SliderDecimalPlaces(int i)
        {
            return 8;
        }

        public float GetSliderValue(int i)
        {
            return tempDelta * (GameUtil.temperatureUnit == GameUtil.TemperatureUnit.Fahrenheit ? TEMP_F_SCALAR : 1);
        }

        public string GetSliderTooltipKey(int i)
        {
            return KEY + ".TOOLTIP";
        }

        public string GetSliderTooltip()
        {
            return string.Format(UI.UISIDESCREENS.AIRCONDITIONERTEMPERATURESIDESCREEN.TOOLTIP, tempDelta,
                SliderUnits);
        }

        public string SliderTitleKey => KEY + ".TITLE";
        public string SliderUnits => GameUtil.GetTemperatureUnitSuffix();

        public void SetSliderValue(float val, int i)
        {
            tempDelta = GameUtil.temperatureUnit == GameUtil.TemperatureUnit.Fahrenheit ? val / TEMP_F_SCALAR : val;
            Update();
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe<AirConditionerAdjustable>((int)GameHashes.CopySettings, OnCopySettingsDelegate);
        }

        protected override void OnSpawn()
        {
            if (tempDelta == int.MaxValue)
            {
                tempDelta = airConditioner.temperatureDelta;
            }

            Update();
        }

        internal void OnCopySettings(object data)
        {
            AirConditionerAdjustable comp = ((UnityEngine.GameObject)data).GetComponent<AirConditionerAdjustable>();
            if (comp != null)
            {
                this.tempDelta = comp.tempDelta;
            }
        }

        internal void Update()
        {
            var ratio = tempDelta / MIN_DELTA;

#if DEBUG
            Debug.LogFormat("Updated temperature delta: {0}C ({1})", tempDelta, ratio);
#endif
            airConditioner.temperatureDelta = tempDelta;
            // apply watts config based on building type
            if (airConditioner.isLiquidConditioner)
            {
                energyConsumer.BaseWattageRating = SingletonOptions<Config>.Instance.AquatunerWattsPerDegree * Mathf.Abs(tempDelta);
            }
            else
            {
                energyConsumer.BaseWattageRating = SingletonOptions<Config>.Instance.AirConditionerWattsPerDegree * Mathf.Abs(tempDelta);
            }
        }
    }
}