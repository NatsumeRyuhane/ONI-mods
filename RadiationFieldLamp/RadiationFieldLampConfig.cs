using PeterHan.PLib.Options;
using TUNING;
using UnityEngine;

namespace RadiationFieldLamp
{
    public class RadiationFieldLampConfig : IBuildingConfig
    {
        public const string ID = "RadiationFieldLamp";

        private const float FuelStorageKg = 50f;

        private const float FuelRefillKg = 5f;

        private const float WasteDropKg = 5f;

        public const float WasteFraction = 0.9f;

        private static readonly Tag FuelTag = SimHashes.UraniumOre.CreateTag();

        private const SimHashes WasteElement = SimHashes.DepletedUranium;

        public static float MaxRads
        {
            get { return SingletonOptions<Config>.Instance.MaxRadsPerCell; }
        }

        public static float DefaultRads
        {
            get { return Mathf.Clamp(SingletonOptions<Config>.Instance.RadsPerCell, 0f, MaxRads); }
        }

        // Uranium Ore per second for each rad per cell, in kg. The option is in mg.
        public static float FuelPerRadKg
        {
            get { return SingletonOptions<Config>.Instance.UraniumOreMgPerRad / 1000000f; }
        }

        public override string[] GetRequiredDlcIds()
        {
            return DlcManager.EXPANSION1;
        }

        public override BuildingDef CreateBuildingDef()
        {
            Config config = SingletonOptions<Config>.Instance;
            BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 1, 1, "radiation_lamp_kanim", 10, 10f,
                BUILDINGS.CONSTRUCTION_MASS_KG.TIER1, MATERIALS.ALL_METALS, 800f, BuildLocationRule.Anywhere,
                noise: NOISE_POLLUTION.NONE, decor: BUILDINGS.DECOR.NONE);
            def.RequiresPowerInput = config.PowerWatts > 0f;
            def.EnergyConsumptionWhenActive = config.PowerWatts;
            def.SelfHeatKilowattsWhenActive = config.PowerWatts / 1000f;
            def.ExhaustKilowattsWhenActive = 0f;
            def.ViewMode = OverlayModes.Radiation.ID;
            def.AudioCategory = "Metal";
            def.PermittedRotations = PermittedRotations.R360;
            def.DiseaseCellVisName = "RadiationSickness";
            def.UtilityOutputOffset = CellOffset.none;
            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<LoopingSounds>();
            Prioritizable.AddRef(go);
            if (FuelPerRadKg <= 0f)
            {
                return;
            }
            Storage storage = BuildingTemplates.CreateDefaultStorage(go);
            storage.showInUI = true;
            storage.capacityKg = FuelStorageKg;
            storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
            ManualDeliveryKG delivery = go.AddOrGet<ManualDeliveryKG>();
            delivery.SetStorage(storage);
            delivery.RequestedItemTag = FuelTag;
            delivery.capacity = FuelStorageKg;
            delivery.refillMass = FuelRefillKg;
            delivery.choreTypeIDHash = Db.Get().ChoreTypes.FetchCritical.IdHash;
            // The prefab's rates match the default strength, which is what the build menu shows. Each lamp sets its
            // own rates from its slider.
            float fuelPerSecond = DefaultRads * FuelPerRadKg;
            ElementConverter converter = go.AddOrGet<ElementConverter>();
            converter.consumedElements = new[]
            {
                new ElementConverter.ConsumedElement(FuelTag, fuelPerSecond)
            };
            converter.outputElements = new[]
            {
                new ElementConverter.OutputElement(fuelPerSecond * WasteFraction, WasteElement, 0f,
                    useEntityTemperature: false, storeOutput: true, diseaseWeight: 0.5f)
            };
            ElementDropper dropper = go.AddOrGet<ElementDropper>();
            dropper.emitTag = WasteElement.CreateTag();
            dropper.emitMass = WasteDropKg;
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            AddRangeVisualizer(go);
        }

        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
            AddRangeVisualizer(go);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            float rads = DefaultRads;
            for (int i = 0; i < RadiationFieldLamp.MaxFieldCells; i++)
            {
                RadiationEmitter emitter = go.AddComponent<RadiationEmitter>();
                emitter.emitType = RadiationEmitter.RadiationEmitterType.Constant;
                emitter.radiusProportionalToRads = false;
                emitter.emitRadiusX = 1;
                emitter.emitRadiusY = 1;
                emitter.emitAngle = 360f;
                emitter.emitRate = 0f;
                emitter.emitRads = rads;
            }
            AddRangeVisualizer(go);
            go.AddOrGet<RadiationFieldLamp>();
        }

        // RangeVisualizerEffect rotates the range with the building's Rotatable, so it is given unrotated (facing
        // right) and follows the placement rotation on its own.
        private static void AddRangeVisualizer(GameObject go)
        {
            RangeVisualizer visualizer = go.AddOrGet<RangeVisualizer>();
            visualizer.TestLineOfSight = false;
            RadiationFieldLamp.SetRange(visualizer, RadiationFieldLamp.GetFieldOffsets(EightDirection.Right));
        }
    }
}
