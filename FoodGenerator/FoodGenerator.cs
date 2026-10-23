using System.Collections.Generic;
using KSerialization;
using STRINGS;
using UnityEngine;

namespace FoodGenerator
{
    // Burns the order in progress on FoodBurner for power, like EnergyGenerator burns coal. Every tick it spends the
    // food's energy, at 4,184 J per kcal, on the power it makes plus the heat it gives off, so an order's calories
    // become exactly that much energy. The order's gases come out in proportion to the share burned.
    //
    // Like the Coal Generator, it burns whatever the batteries hold, and its slider is the vanilla Fuel Request
    // Threshold: while every battery on the circuit is at or above it, FoodBurner fetches no food for new orders.
    //
    // The heat is the def's exhaust and self heat. Engie's Tune-Up raises the power, and the food pays for the extra.
    // With the Efficient Tune-Up mod, which tags the buildings it changes, a tune-up takes its extra power out of the
    // heat instead, so the food is charged for the lower heat and burns at the untuned rate.
    [SerializationConfig(MemberSerialization.OptIn)]
    public class FoodGenerator : Generator, ISingleSliderControl, IGameObjectEffectDescriptor
    {
        // EnergyGenerator's slider strings, the Coal Generator's Fuel Request Threshold.
        private const string SliderTitleKeyString = "STRINGS.UI.UISIDESCREENS.MANUALDELIVERYGENERATORSIDESCREEN.TITLE";

        private const string SliderTooltipKeyString = "STRINGS.UI.UISIDESCREENS.MANUALDELIVERYGENERATORSIDESCREEN.TOOLTIP";

        public const string FoodDescriptorKey = "STRINGS.BUILDINGS.PREFABS.FOODGENERATOR.FOOD_DESCRIPTOR";

        public const string FoodDescriptorTooltipKey = "STRINGS.BUILDINGS.PREFABS.FOODGENERATOR.FOOD_DESCRIPTOR_TOOLTIP";

        // Gas waits until this much has built up, so a slow burn doesn't send the sim a few milligrams every tick.
        private const float MinEmitKg = 0.01f;

        private static readonly Tag Co2Tag = SimHashes.CarbonDioxide.CreateTag();

        private static readonly Tag SteamTag = SimHashes.Steam.CreateTag();

        // Set by the Efficient Tune-Up mod on buildings whose tune-up takes its power out of the heat.
        private static readonly Tag EfficientTuneUpTag = TagManager.Create("EfficientTuneUp");

        private static readonly EventSystem.IntraObjectHandler<FoodGenerator> OnActiveChangedDelegate =
            new EventSystem.IntraObjectHandler<FoodGenerator>((component, data) => component.OnActiveChanged());

        private static readonly EventSystem.IntraObjectHandler<FoodGenerator> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<FoodGenerator>((component, data) => component.OnCopySettings(data));

        // The game assigns [MyCmpGet] fields by reflection, which the compiler can't see.
#pragma warning disable CS0649
        [MyCmpGet]
        private FoodBurner burner;

        [MyCmpGet]
        private PrimaryElement primaryElement;

        [MyCmpGet]
        private KBatchedAnimController animController;
#pragma warning restore CS0649

        [Serialize]
        private float batteryRefillPercent = 0.5f;

        [Serialize]
        private float pendingCo2Kg;

        [Serialize]
        private float pendingSteamKg;

        private MeterController meter;

        public string SliderTitleKey
        {
            get { return SliderTitleKeyString; }
        }

        public string SliderUnits
        {
            get { return UI.UNITSUFFIXES.PERCENT; }
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
            return 100f;
        }

        public float GetSliderValue(int index)
        {
            return batteryRefillPercent * 100f;
        }

        public void SetSliderValue(float value, int index)
        {
            batteryRefillPercent = value / 100f;
        }

        public string GetSliderTooltipKey(int index)
        {
            return SliderTooltipKeyString;
        }

        public string GetSliderTooltip(int index)
        {
            return string.Format(Strings.Get(SliderTooltipKeyString), Strings.Get(Patch.FoodNameKey),
                batteryRefillPercent * 100f);
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            EnergyGenerator.EnsureStatusItemAvailable();
            Subscribe((int)GameHashes.ActiveChanged, OnActiveChangedDelegate);
            Subscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            // The Coal Generator's fuel meter, here showing what is left of the order in progress.
            meter = new MeterController(animController, "meter_target", "meter", Meter.Offset.Behind,
                Grid.SceneLayer.NoLayer, "meter_target", "meter_fill", "meter_frame", "meter_OL");
            UpdateMeter();
        }

        public override void EnergySim200ms(float dt)
        {
            base.EnergySim200ms(dt);
            operational.SetFlag(wireConnectedFlag, CircuitID != ushort.MaxValue);
            bool active = false;
            if (operational.IsOperational)
            {
                bool needed = CircuitNeedsPower();
                selectable.ToggleStatusItem(EnergyGenerator.BatteriesSufficientlyFull, !needed);
                burner.FetchPaused = !needed;
                ComplexRecipe order = burner.CurrentWorkingOrder;
                // WattageRating includes a tune-up. Efficient Tune-Up cuts the heat by the extra power, down to
                // none, the same as it works out from the power this generator makes.
                float powerWatts = WattageRating;
                float heatWatts = FoodGeneratorConfig.HeatWatts;
                if (gameObject.HasTag(EfficientTuneUpTag))
                {
                    heatWatts -= Mathf.Clamp(powerWatts - BaseWattageRating, 0f, heatWatts);
                }
                float tickJoules = (powerWatts + heatWatts) * dt;
                if (order != null && tickJoules > 0f)
                {
                    // An order with no energy, which no vanilla food makes, finishes at once.
                    float orderJoules = FoodGeneratorConfig.OrderJoules(order);
                    float share = burner.Burn(orderJoules > 0f ? tickJoules / orderJoules : 1f);
                    // The last tick of an order may need less than a whole tick of energy.
                    float tickUsed = share * orderJoules / tickJoules;
                    GenerateJoules(powerWatts * dt * tickUsed);
                    AddExhaust(order, share);
                    // Stay active across the switch to the next order, so the animation doesn't stop and restart.
                    active = share > 0f || burner.CurrentWorkingOrder != null;
                }
            }
            operational.SetActive(active);
            UpdateMeter();
        }

        public List<Descriptor> GetDescriptors(GameObject go)
        {
            float caloriesPerSecond = FoodGeneratorConfig.BurnWatts / FoodGeneratorConfig.JoulesPerKcal * 1000f;
            string rate = GameUtil.GetFormattedCalories(caloriesPerSecond, GameUtil.TimeSlice.PerCycle);
            return new List<Descriptor>
            {
                new Descriptor(string.Format(Strings.Get(FoodDescriptorKey), rate),
                    string.Format(Strings.Get(FoodDescriptorTooltipKey), rate), Descriptor.DescriptorType.Requirement)
            };
        }

        // EnergyGenerator's rule for requesting fuel: while any battery on the circuit is below the threshold, or
        // while there is no battery. At a 0% threshold, while a battery is empty.
        private bool CircuitNeedsPower()
        {
            List<Battery> batteries = Game.Instance.circuitManager.GetBatteriesOnCircuit(CircuitID);
            if (batteries.Count == 0)
            {
                return true;
            }
            foreach (Battery battery in batteries)
            {
                if (batteryRefillPercent <= 0f && battery.PercentFull <= 0f)
                {
                    return true;
                }
                if (battery.PercentFull < batteryRefillPercent)
                {
                    return true;
                }
            }
            return false;
        }

        private void AddExhaust(ComplexRecipe order, float share)
        {
            foreach (ComplexRecipe.RecipeElement result in order.results)
            {
                if (result.material == Co2Tag)
                {
                    pendingCo2Kg += result.amount * share;
                }
                else if (result.material == SteamTag)
                {
                    pendingSteamKg += result.amount * share;
                }
            }
            if (pendingCo2Kg >= MinEmitKg)
            {
                Emit(SimHashes.CarbonDioxide, pendingCo2Kg);
                pendingCo2Kg = 0f;
            }
            if (pendingSteamKg >= MinEmitKg)
            {
                Emit(SimHashes.Steam, pendingSteamKg);
                pendingSteamKg = 0f;
            }
        }

        // Lets gas out where the Coal Generator lets out its Carbon Dioxide, at its temperature or hotter.
        private void Emit(SimHashes element, float massKg)
        {
            int cell = Grid.OffsetCell(Grid.PosToCell(this), FoodGeneratorConfig.ExhaustOffset);
            float temperature = Mathf.Max(primaryElement.Temperature, GeneratorConfig.CO2_OUTPUT_TEMPERATURE);
            SimMessages.ModifyMass(cell, massKg, byte.MaxValue, 0, CellEventLogger.Instance.EnergyGeneratorModifyMass,
                temperature, element);
        }

        private void UpdateMeter()
        {
            if (meter == null)
            {
                return;
            }
            meter.SetPositionPercent(burner.CurrentWorkingOrder != null ? 1f - burner.OrderProgress : 0f);
        }

        private void OnActiveChanged()
        {
            StatusItem statusItem = operational.IsActive
                ? Db.Get().BuildingStatusItems.Wattage
                : Db.Get().BuildingStatusItems.GeneratorOffline;
            selectable.SetStatusItem(Db.Get().StatusItemCategories.Power, statusItem, this);
        }

        private void OnCopySettings(object data)
        {
            GameObject source = data as GameObject;
            FoodGenerator other = source != null ? source.GetComponent<FoodGenerator>() : null;
            if (other != null)
            {
                batteryRefillPercent = other.batteryRefillPercent;
            }
        }
    }
}
