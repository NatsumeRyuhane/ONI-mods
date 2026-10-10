using System.Collections.Generic;
using HarmonyLib;
using KSerialization;
using PeterHan.PLib.Options;
using UnityEngine;

namespace RadiationFieldLamp
{
    // Keeps an even field of radiation in the 4x3 cells made of the lamp's own line and the 3 lines in front of it.
    // Each cell gets its own native emitter
    // with radius 1: the sim's falloff (1-|dx|/rX)(1-|dy|/rY) is zero for every neighbour, so a radius-1 emitter only
    // reaches its own cell, at full strength. emitRate 0 makes the sim run exactly one pass per tick, and with the
    // sim's linger rate of 1.1 one pass per tick settles at emitRads. The lamp points the way it was placed. With the
    // Adjustable Direction option, the Radbolt Reflector's side screen can re-point it, diagonals included. Each lamp
    // has its own strength, set with the game's single slider side screen, and burns fuel in proportion to it.
    [SerializationConfig(MemberSerialization.OptIn)]
    public class RadiationFieldLamp : KMonoBehaviour, IHighEnergyParticleDirection, ISingleSliderControl
    {
        public const string SliderTitleKeyString = "STRINGS.UI.UISIDESCREENS.RADIATIONFIELDLAMP.TITLE";

        public const string SliderTooltipKeyString = "STRINGS.UI.UISIDESCREENS.RADIATIONFIELDLAMP.TOOLTIP";

        public const int FieldCells = 12;

        private static readonly EventSystem.IntraObjectHandler<RadiationFieldLamp> OnStateChangedDelegate =
            new EventSystem.IntraObjectHandler<RadiationFieldLamp>((component, data) => component.UpdateEmitting());

        private static readonly EventSystem.IntraObjectHandler<RadiationFieldLamp> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<RadiationFieldLamp>((component, data) => component.OnCopySettings(data));

        private static readonly EventSystem.IntraObjectHandler<RadiationFieldLamp> OnRotatedDelegate =
            new EventSystem.IntraObjectHandler<RadiationFieldLamp>((component, data) => component.OnRotated());

        // The meter's tracker only redraws when the lamp's symbol or position changes, not its rotation. Clearing its
        // cached matrix makes it pick up a new rotation on the next frame.
        private static readonly AccessTools.FieldRef<KBatchedAnimTracker, Matrix2x3> TrackerPreviousMatrix =
            AccessTools.FieldRefAccess<KBatchedAnimTracker, Matrix2x3>("previousMatrix");

        // The game assigns [MyCmpGet] fields by reflection, which the compiler can't see.
#pragma warning disable CS0649
        [MyCmpGet]
        private Operational operational;

        [MyCmpGet]
        private Storage storage;

        [MyCmpGet]
        private ElementConverter elementConverter;

        [MyCmpGet]
        private KBatchedAnimController animController;

        [MyCmpGet]
        private RangeVisualizer rangeVisualizer;

        [MyCmpGet]
        private Rotatable rotatable;
#pragma warning restore CS0649

        [Serialize]
        private bool hasCustomDirection;

        [Serialize]
        private EightDirection customDirection = EightDirection.Right;

        // Rads per cell. Negative until the lamp spawns for the first time, which then uses the default.
        [Serialize]
        private float radsPerCell = -1f;

        private RadiationEmitter[] emitters;

        private MeterController meter;

        private bool emitting;

        private static bool AdjustableDirection
        {
            get { return SingletonOptions<Config>.Instance.AdjustableDirection; }
        }

        private bool UseCustomDirection
        {
            get { return hasCustomDirection && AdjustableDirection; }
        }

        // The direction from the placement rotation. The lamp art faces right when unrotated.
        private EightDirection PlacedDirection
        {
            get
            {
                switch (rotatable != null ? rotatable.GetOrientation() : Orientation.Neutral)
                {
                    case Orientation.R90: return EightDirection.Down;
                    case Orientation.R180: return EightDirection.Left;
                    case Orientation.R270: return EightDirection.Up;
                    case Orientation.FlipH: return EightDirection.Left;
                    default: return EightDirection.Right;
                }
            }
        }

        public EightDirection Direction
        {
            get { return UseCustomDirection ? customDirection : PlacedDirection; }
            set
            {
                if (!AdjustableDirection)
                {
                    return;
                }
                customDirection = value;
                hasCustomDirection = true;
                if (isSpawned)
                {
                    ApplyDirection();
                }
            }
        }

        public float RadsPerCell
        {
            get { return radsPerCell; }
            set
            {
                radsPerCell = Mathf.Clamp(Mathf.Round(value), 0f, RadiationFieldLampConfig.MaxRads);
                if (isSpawned)
                {
                    ApplyStrength();
                }
            }
        }

        public string SliderTitleKey
        {
            get { return SliderTitleKeyString; }
        }

        public string SliderUnits
        {
            get { return STRINGS.UI.UNITSUFFIXES.RADIATION.RADS; }
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
            return RadiationFieldLampConfig.MaxRads;
        }

        public float GetSliderValue(int index)
        {
            return radsPerCell;
        }

        public void SetSliderValue(float value, int index)
        {
            RadsPerCell = value;
        }

        public string GetSliderTooltipKey(int index)
        {
            return SliderTooltipKeyString;
        }

        public string GetSliderTooltip(int index)
        {
            return string.Format(Strings.Get(SliderTooltipKeyString), radsPerCell);
        }

        // The 12 cell offsets for a direction: a 4x3 block whose middle line starts at the lamp, so it covers the lamp,
        // the cells beside it and the 3x3 block in front. A diagonal takes the block of its up or down part and moves
        // it one cell toward its left or right part, which puts the lamp in a corner of the block.
        public static List<CellOffset> GetFieldOffsets(EightDirection direction)
        {
            Vector2I forward = GetForward(direction);
            bool diagonal = forward.x != 0 && forward.y != 0;
            Vector2I ahead = diagonal ? new Vector2I(0, forward.y) : forward;
            Vector2I side = new Vector2I(-ahead.y, ahead.x);
            int shift = diagonal ? forward.x : 0;
            List<CellOffset> offsets = new List<CellOffset>(FieldCells);
            for (int along = 0; along <= 3; along++)
            {
                for (int across = -1; across <= 1; across++)
                {
                    offsets.Add(new CellOffset(along * ahead.x + across * side.x + shift,
                        along * ahead.y + across * side.y));
                }
            }
            return offsets;
        }

        public static void SetRange(RangeVisualizer visualizer, IEnumerable<CellOffset> offsets)
        {
            Vector2I min = new Vector2I(int.MaxValue, int.MaxValue);
            Vector2I max = new Vector2I(int.MinValue, int.MinValue);
            foreach (CellOffset offset in offsets)
            {
                min = new Vector2I(Mathf.Min(min.x, offset.x), Mathf.Min(min.y, offset.y));
                max = new Vector2I(Mathf.Max(max.x, offset.x), Mathf.Max(max.y, offset.y));
            }
            visualizer.RangeMin = min;
            visualizer.RangeMax = max;
        }

        private static Vector2I GetForward(EightDirection direction)
        {
            switch (direction)
            {
                case EightDirection.Up: return new Vector2I(0, 1);
                case EightDirection.UpLeft: return new Vector2I(-1, 1);
                case EightDirection.Left: return new Vector2I(-1, 0);
                case EightDirection.DownLeft: return new Vector2I(-1, -1);
                case EightDirection.Down: return new Vector2I(0, -1);
                case EightDirection.DownRight: return new Vector2I(1, -1);
                case EightDirection.UpRight: return new Vector2I(1, 1);
                default: return new Vector2I(1, 0);
            }
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
            Subscribe((int)GameHashes.Rotated, OnRotatedDelegate);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            emitters = GetComponents<RadiationEmitter>();
            if (elementConverter != null)
            {
                meter = new MeterController(animController, "meter_target", "meter", Meter.Offset.Infront,
                    Grid.SceneLayer.NoLayer, "meter_target");
            }
            Subscribe((int)GameHashes.OperationalChanged, OnStateChangedDelegate);
            Subscribe((int)GameHashes.OnStorageChange, OnStateChangedDelegate);
            radsPerCell = Mathf.Clamp(radsPerCell < 0f ? RadiationFieldLampConfig.DefaultRads : radsPerCell, 0f,
                RadiationFieldLampConfig.MaxRads);
            SetStrength();
            ApplyDirection();
            SetEmitting(false);
            UpdateEmitting();
        }

        protected override void OnCleanUp()
        {
            Unsubscribe((int)GameHashes.OperationalChanged, OnStateChangedDelegate);
            Unsubscribe((int)GameHashes.OnStorageChange, OnStateChangedDelegate);
            base.OnCleanUp();
        }

        private void OnCopySettings(object data)
        {
            GameObject source = data as GameObject;
            RadiationFieldLamp other = source != null ? source.GetComponent<RadiationFieldLamp>() : null;
            if (other == null)
            {
                return;
            }
            RadsPerCell = other.RadsPerCell;
            if (AdjustableDirection)
            {
                Direction = other.Direction;
            }
        }

        // Rotatable re-applies the placement rotation to the art when it spawns, which can happen after this
        // component spawns. Put a custom direction back on top of it.
        private void OnRotated()
        {
            if (isSpawned && UseCustomDirection)
            {
                ApplyCustomRotation();
            }
        }

        private void ApplyDirection()
        {
            List<CellOffset> offsets = GetFieldOffsets(Direction);
            for (int i = 0; i < emitters.Length && i < offsets.Count; i++)
            {
                emitters[i].emissionOffset = new Vector3(offsets[i].x, offsets[i].y);
            }
            SendEmitters();
            if (!UseCustomDirection)
            {
                // Rotatable already turned the art and the range overlay with the placement rotation.
                return;
            }
            ApplyCustomRotation();
            if (rangeVisualizer != null)
            {
                // RangeVisualizerEffect rotates the range by the placement rotation, so undo that first.
                Orientation undo = Orientation.Neutral;
                if (rotatable != null)
                {
                    switch (rotatable.GetOrientation())
                    {
                        case Orientation.R90: undo = Orientation.R270; break;
                        case Orientation.R270: undo = Orientation.R90; break;
                        default: undo = rotatable.GetOrientation(); break;
                    }
                }
                List<CellOffset> unrotated = new List<CellOffset>(offsets.Count);
                foreach (CellOffset offset in offsets)
                {
                    unrotated.Add(Rotatable.GetRotatedCellOffset(offset, undo));
                }
                SetRange(rangeVisualizer, unrotated);
            }
        }

        // Sets the emitters' rads and the fuel rates from the strength.
        private void SetStrength()
        {
            foreach (RadiationEmitter emitter in emitters)
            {
                emitter.emitRads = radsPerCell;
            }
            if (elementConverter != null)
            {
                float fuelPerSecond = radsPerCell * RadiationFieldLampConfig.FuelPerRadKg;
                elementConverter.consumedElements[0].MassConsumptionRate = fuelPerSecond;
                elementConverter.outputElements[0].massGenerationRate =
                    fuelPerSecond * RadiationFieldLampConfig.WasteFraction;
            }
        }

        private void ApplyStrength()
        {
            SetStrength();
            SendEmitters();
            UpdateEmitting();
        }

        private void ApplyCustomRotation()
        {
            // The lamp art faces right. Rotation is counter-clockwise in degrees, and EightDirection counts
            // counter-clockwise from up in 45 degree steps, with Right at index 6.
            animController.Pivot = new Vector3(0f, 0.5f, 0f);
            animController.Rotation = (EightDirectionUtil.GetAngle(customDirection) + 90f) % 360f;
            KBatchedAnimTracker tracker = meter != null ? meter.gameObject.GetComponent<KBatchedAnimTracker>() : null;
            if (tracker != null)
            {
                TrackerPreviousMatrix(tracker) = default(Matrix2x3);
            }
        }

        private void UpdateEmitting()
        {
            if (meter != null)
            {
                meter.SetPositionPercent(Mathf.Clamp01(storage.MassStored() / storage.capacityKg));
            }
            bool hasFuel = elementConverter == null || elementConverter.HasEnoughMassToStartConverting();
            bool on = operational.IsOperational && hasFuel && radsPerCell > 0f;
            if (on != emitting)
            {
                SetEmitting(on);
            }
        }

        private void SetEmitting(bool on)
        {
            emitting = on;
            operational.SetActive(on);
            SendEmitters();
            animController.Play(on ? "on" : "off");
        }

        // Sends every emitter its state, which also makes the sim pick up a new cell or new rads on the next tick.
        private void SendEmitters()
        {
            foreach (RadiationEmitter emitter in emitters)
            {
                emitter.SetEmitting(emitting);
            }
        }
    }
}
