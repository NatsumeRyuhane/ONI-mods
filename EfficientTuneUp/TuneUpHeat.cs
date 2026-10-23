using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace EfficientTuneUp
{
    // Engie's Tune-Up raises a generator's power by half for the same fuel. This takes that extra power out of the
    // heat the generator gives off, so the tune-up makes it waste less instead of making energy from nothing.
    //
    // The heat comes from the building's def, which every building of a kind shares, so it can't be lowered for one
    // tuned building. Instead, every Sim200ms while the building is active, this takes the cut back out through the
    // same calls StructureTemperatureComponents uses to give the heat off: exhaust heat from the cells the building
    // covers, self heat from the building. The cut is the tuned share of the energy the generator made since the
    // last tick, never more than the heat given off over the tick. It is split between exhaust and self heat as the
    // def splits them, and shows as its own line under "Heat Production".
    public class TuneUpHeat : KMonoBehaviour, ISim200ms
    {
        // Other mods can look for this tag to know a building's tune-up takes its power out of the heat. The Food
        // Generator does, to charge food for the lower heat.
        public static readonly Tag EfficientTuneUpTag = TagManager.Create("EfficientTuneUp");

        // Every spawned building with this component, for the GenerateJoules patch, which runs for every generator.
        public static readonly Dictionary<Generator, TuneUpHeat> ByGenerator = new Dictionary<Generator, TuneUpHeat>();

        private const string SourceNameKey = "STRINGS.DUPLICANTS.MODIFIERS.POWERTINKER.NAME";

        // Private, named by string. Adds a sample to the heat sources the "Heat Production" status lists.
        private static readonly Func<StructureTemperatureComponents, List<StructureTemperaturePayload.EnergySource>,
            float, string, List<StructureTemperaturePayload.EnergySource>> AccumulateProducedEnergyKW =
            AccessTools.MethodDelegate<Func<StructureTemperatureComponents, List<StructureTemperaturePayload.EnergySource>,
                float, string, List<StructureTemperaturePayload.EnergySource>>>(
                AccessTools.Method(typeof(StructureTemperatureComponents), "AccumulateProducedEnergyKW"));

        // The game assigns [MyCmpGet] fields by reflection, which the compiler can't see.
#pragma warning disable CS0649
        [MyCmpGet]
        private Generator generator;

        [MyCmpGet]
        private Operational operational;

        [MyCmpGet]
        private Building building;
#pragma warning restore CS0649

        private HandleVector<int>.Handle structureTemperature;

        private float generatedJoules;

        protected override void OnSpawn()
        {
            base.OnSpawn();
            if (generator != null)
            {
                ByGenerator[generator] = this;
            }
            structureTemperature = GameComps.StructureTemperatures.GetHandle(gameObject);
        }

        protected override void OnCleanUp()
        {
            if (generator != null)
            {
                ByGenerator.Remove(generator);
            }
            base.OnCleanUp();
        }

        public void AddGeneratedJoules(float joules)
        {
            generatedJoules += joules;
        }

        public void Sim200ms(float dt)
        {
            float generated = generatedJoules;
            generatedJoules = 0f;
            // StructureTemperatureComponents gives off the def's heat only while the building is active.
            if (generator == null || operational == null || !operational.IsActive || !structureTemperature.IsValid())
            {
                return;
            }
            float exhaustWatts = building.Def.ExhaustKilowattsWhenActive * 1000f;
            float selfWatts = building.Def.SelfHeatKilowattsWhenActive * 1000f;
            float heatWatts = exhaustWatts + selfWatts;
            if (heatWatts <= 0f)
            {
                return;
            }
            StructureTemperatureComponents temperatures = GameComps.StructureTemperatures;
            StructureTemperaturePayload payload = temperatures.GetPayload(structureTemperature);
            string source = Strings.Get(SourceNameKey);
            float baseWatts = generator.BaseWattageRating;
            float efficiency = baseWatts > 0f ? generator.WattageRating / baseWatts : 1f;
            if (efficiency <= 1f)
            {
                // Not tuned up. Drop the line a tune-up left, which would otherwise keep its last value.
                if (payload.energySourcesKW != null
                    && payload.energySourcesKW.RemoveAll(energySource => energySource.source == source) > 0)
                {
                    temperatures.SetPayload(structureTemperature, ref payload);
                }
                return;
            }
            // The tune-up multiplies the power, so the tuned share of what was made is (efficiency - 1) / efficiency.
            float cutJoules = Mathf.Min(generated * (efficiency - 1f) / efficiency, heatWatts * dt);
            if (cutJoules > 0f)
            {
                float exhaustCutJoules = cutJoules * exhaustWatts / heatWatts;
                float selfCutJoules = cutJoules - exhaustCutJoules;
                if (exhaustCutJoules > 0f)
                {
                    StructureTemperatureComponents.ExhaustHeat(payload.GetExtents(), -exhaustCutJoules / dt / 1000f,
                        payload.maxTemperature, dt);
                }
                if (selfCutJoules > 0f && Sim.IsValidHandle(payload.simHandleCopy))
                {
                    SimMessages.ModifyBuildingEnergy(payload.simHandleCopy, -selfCutJoules / 1000f, 0f, 10000f);
                }
            }
            // A sample every active tuned tick, 0 when nothing was made, like the def's own heat sources, so the
            // status averages them over the same ticks.
            payload.energySourcesKW = AccumulateProducedEnergyKW(temperatures, payload.energySourcesKW,
                -cutJoules / dt / 1000f, source);
            temperatures.SetPayload(structureTemperature, ref payload);
        }
    }
}
