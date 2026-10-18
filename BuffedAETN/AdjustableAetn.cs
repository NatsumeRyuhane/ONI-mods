using System.Collections.Generic;
using HarmonyLib;
using KSerialization;
using PeterHan.PLib.Options;
using UnityEngine;

namespace BuffedAETN
{
    // Per-building settings for the AETN, shown as two sliders in the game's multi slider side screen: cooling power
    // and minimum operating temperature. Hydrogen use follows the cooling power at the configured kDTU per gram, and the
    // Hydrogen buffer holds the same 10 seconds of use as vanilla. The lowest minimum temperature rises with the
    // cooling power, so a strong AETN shuts off before one 200 ms tick of cooling can push a cell below 0 K.
    [SerializationConfig(MemberSerialization.OptIn)]
    public class AdjustableAetn : KMonoBehaviour, IMultiSliderControl
    {
        public const float VanillaCoolingKw = 80f;

        public const float VanillaMinTemperature = 100f;

        // 80 kW for 10 g/s. A kW is a kDTU per second, so this is kDTU per gram.
        public const float VanillaCoolingKdtuPerGram = 8f;

        // Same split between self heat and exhaust as vanilla (64 / 16 of 80 kW).
        public const float SelfHeatShare = 0.8f;

        public const float ExhaustShare = 0.2f;

        // Vanilla buffers 100 g at 10 g/s.
        public const float BufferSeconds = 10f;

        // The lowest minimum temperature for each kW of cooling. The fastest cooling is the exhaust into a cell of
        // low heat capacity, about 0.0035 K per kW per tick for 1.5 kg of Chlorine. The AETN takes a few ticks to stop
        // after it gets too cold, so this leaves room for several ticks with margin.
        public const float FloorKelvinPerKw = 0.05f;

        // The top of the minimum temperature slider, unless the floor for the cooling power is higher.
        public const float MaxMinTemperature = 300f;

        private const float GramsPerKg = 1000f;

        public const string TitleKeyString = "STRINGS.UI.UISIDESCREENS.BUFFEDAETN.TITLE";

        public const string CoolingTitleKeyString = "STRINGS.UI.UISIDESCREENS.BUFFEDAETN.COOLING.TITLE";

        public const string CoolingTooltipKeyString = "STRINGS.UI.UISIDESCREENS.BUFFEDAETN.COOLING.TOOLTIP";

        public const string MinTemperatureTitleKeyString = "STRINGS.UI.UISIDESCREENS.BUFFEDAETN.MINTEMPERATURE.TITLE";

        public const string MinTemperatureTooltipKeyString =
            "STRINGS.UI.UISIDESCREENS.BUFFEDAETN.MINTEMPERATURE.TOOLTIP";

        private static readonly EventSystem.IntraObjectHandler<AdjustableAetn> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<AdjustableAetn>((component, data) => component.OnCopySettings(data));

        private static readonly AccessTools.FieldRef<DetailsScreen, List<DetailsScreen.SideScreenRef>> SideScreens =
            AccessTools.FieldRefAccess<DetailsScreen, List<DetailsScreen.SideScreenRef>>("sideScreens");

        private static readonly AccessTools.FieldRef<MultiSliderSideScreen, List<SliderSet>> SliderSets =
            AccessTools.FieldRefAccess<MultiSliderSideScreen, List<SliderSet>>("sliderSets");

        // The game assigns [MyCmpGet] fields by reflection, which the compiler can't see.
#pragma warning disable CS0649
        [MyCmpGet]
        private Storage storage;

        [MyCmpGet]
        private ConduitConsumer conduitConsumer;

        [MyCmpGet]
        private ElementConverter elementConverter;

        [MyCmpGet]
        private MinimumOperatingTemperature minimumOperatingTemperature;
#pragma warning restore CS0649

        // Starts at the vanilla values, so AETNs from a save made without the mod keep working as before.
        [Serialize]
        private float coolingKw = VanillaCoolingKw;

        [Serialize]
        private float minTemperature = VanillaMinTemperature;

        private ISliderControl[] sliderControls;

        private static float MaxCoolingKw
        {
            get { return SingletonOptions<Config>.Instance.MaxCoolingPower; }
        }

        private static float CoolingKdtuPerGram
        {
            get { return SingletonOptions<Config>.Instance.CoolingKdtuPerGram; }
        }

        public float SelfHeatKw
        {
            get { return -SelfHeatShare * coolingKw; }
        }

        public float ExhaustKw
        {
            get { return -ExhaustShare * coolingKw; }
        }

        public float HydrogenKgPerSecond
        {
            get
            {
                // kDTU/s divided by kDTU/g is g/s.
                float kdtuPerGram = CoolingKdtuPerGram;
                return kdtuPerGram > 0f ? coolingKw / kdtuPerGram / GramsPerKg : 0f;
            }
        }

        public float MinTemperatureFloor
        {
            get { return FloorFor(coolingKw); }
        }

        public float CoolingKw
        {
            get { return coolingKw; }
            set
            {
                float kw = Mathf.Clamp(Mathf.Round(value), 0f, MaxCoolingKw);
                if (kw == coolingKw)
                {
                    return;
                }
                coolingKw = kw;
                minTemperature = ClampMinTemperature(minTemperature);
                if (isSpawned)
                {
                    Apply();
                    RefreshMinTemperatureSlider();
                }
            }
        }

        public float MinTemperature
        {
            get { return minTemperature; }
            set
            {
                minTemperature = ClampMinTemperature(Mathf.Round(value));
                if (isSpawned)
                {
                    Apply();
                }
            }
        }

        public string SidescreenTitleKey
        {
            get { return TitleKeyString; }
        }

        ISliderControl[] IMultiSliderControl.sliderControls
        {
            get { return sliderControls; }
        }

        public bool SidescreenEnabled()
        {
            return true;
        }

        public static float FloorFor(float kw)
        {
            return Mathf.Ceil(FloorKelvinPerKw * kw);
        }

        private float ClampMinTemperature(float kelvin)
        {
            float floor = MinTemperatureFloor;
            return Mathf.Clamp(kelvin, floor, Mathf.Max(MaxMinTemperature, floor));
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            sliderControls = new ISliderControl[] { new CoolingSlider(this), new MinTemperatureSlider(this) };
            Subscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            // The options may have changed since the save was made.
            coolingKw = Mathf.Clamp(coolingKw, 0f, MaxCoolingKw);
            minTemperature = ClampMinTemperature(minTemperature);
            Apply();
        }

        private void OnCopySettings(object data)
        {
            GameObject source = data as GameObject;
            AdjustableAetn other = source != null ? source.GetComponent<AdjustableAetn>() : null;
            if (other == null)
            {
                return;
            }
            CoolingKw = other.CoolingKw;
            MinTemperature = other.MinTemperature;
        }

        private void Apply()
        {
            float hydrogenKgPerSecond = HydrogenKgPerSecond;
            float bufferKg = hydrogenKgPerSecond * BufferSeconds;
            storage.capacityKg = bufferKg;
            conduitConsumer.capacityKG = bufferKg;
            // Each instance has its own copy of the array, cloned from the prefab.
            elementConverter.consumedElements[0].MassConsumptionRate = hydrogenKgPerSecond;
            minimumOperatingTemperature.minimumTemperature = minTemperature;

            // The sim only reads the self heat when the structure temperature entry is marked dirty.
            HandleVector<int>.Handle handle = GameComps.StructureTemperatures.GetHandle(gameObject);
            if (handle.IsValid())
            {
                StructureTemperatureHeader header = GameComps.StructureTemperatures.GetHeader(handle);
                header.dirty = true;
                GameComps.StructureTemperatures.SetHeader(handle, header);
            }
        }

        // The side screen reads a slider's range only when it's set up. When this AETN is selected, set up the
        // minimum temperature slider again so its range follows the new cooling power.
        private void RefreshMinTemperatureSlider()
        {
            DetailsScreen details = DetailsScreen.Instance;
            if (details == null || details.target != gameObject)
            {
                return;
            }
            List<DetailsScreen.SideScreenRef> screens = SideScreens(details);
            if (screens == null)
            {
                return;
            }
            foreach (DetailsScreen.SideScreenRef screen in screens)
            {
                MultiSliderSideScreen sliderScreen = screen.screenInstance as MultiSliderSideScreen;
                if (sliderScreen == null || !sliderScreen.gameObject.activeInHierarchy)
                {
                    continue;
                }
                List<SliderSet> sets = SliderSets(sliderScreen);
                if (sets != null && sets.Count > 1)
                {
                    sets[1].SetTarget(sliderControls[1], 1);
                }
            }
        }

        private sealed class CoolingSlider : ISliderControl
        {
            private readonly AdjustableAetn aetn;

            public CoolingSlider(AdjustableAetn aetn)
            {
                this.aetn = aetn;
            }

            public string SliderTitleKey
            {
                get { return CoolingTitleKeyString; }
            }

            public string SliderUnits
            {
                get { return STRINGS.UI.UNITSUFFIXES.ELECTRICAL.KILOWATT; }
            }

            public int SliderDecimalPlaces(int index)
            {
                return 0;
            }

            public float GetSliderMin(int index)
            {
                return 0f;
            }

            public float GetSliderMax(int index)
            {
                return MaxCoolingKw;
            }

            public float GetSliderValue(int index)
            {
                return aetn.CoolingKw;
            }

            public void SetSliderValue(float value, int index)
            {
                aetn.CoolingKw = value;
            }

            public string GetSliderTooltipKey(int index)
            {
                return CoolingTooltipKeyString;
            }

            public string GetSliderTooltip(int index)
            {
                return string.Format(Strings.Get(CoolingTooltipKeyString), aetn.CoolingKw,
                    GameUtil.GetFormattedMass(aetn.HydrogenKgPerSecond, GameUtil.TimeSlice.PerSecond),
                    GameUtil.GetFormattedMass(aetn.HydrogenKgPerSecond * BufferSeconds),
                    GameUtil.GetFormattedTemperature(aetn.MinTemperatureFloor));
            }
        }

        private sealed class MinTemperatureSlider : ISliderControl
        {
            private readonly AdjustableAetn aetn;

            public MinTemperatureSlider(AdjustableAetn aetn)
            {
                this.aetn = aetn;
            }

            public string SliderTitleKey
            {
                get { return MinTemperatureTitleKeyString; }
            }

            public string SliderUnits
            {
                get { return STRINGS.UI.UNITSUFFIXES.TEMPERATURE.KELVIN; }
            }

            public int SliderDecimalPlaces(int index)
            {
                return 0;
            }

            public float GetSliderMin(int index)
            {
                return aetn.MinTemperatureFloor;
            }

            public float GetSliderMax(int index)
            {
                return Mathf.Max(MaxMinTemperature, aetn.MinTemperatureFloor);
            }

            public float GetSliderValue(int index)
            {
                return aetn.MinTemperature;
            }

            public void SetSliderValue(float value, int index)
            {
                aetn.MinTemperature = value;
            }

            public string GetSliderTooltipKey(int index)
            {
                return MinTemperatureTooltipKeyString;
            }

            public string GetSliderTooltip(int index)
            {
                return string.Format(Strings.Get(MinTemperatureTooltipKeyString),
                    GameUtil.GetFormattedTemperature(aetn.MinTemperature),
                    GameUtil.GetFormattedTemperature(aetn.MinTemperatureFloor), aetn.CoolingKw);
            }
        }
    }
}
