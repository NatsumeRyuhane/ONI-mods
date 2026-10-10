using System.Collections.Generic;
using KSerialization;
using UnityEngine;

namespace BetterRadiationLamp
{
    // Keeps an even field of radiation in the 3x3 cells in front of the lamp. Each cell gets its own native emitter
    // with radius 1: the sim's falloff (1-|dx|/rX)(1-|dy|/rY) is zero for every neighbour, so a radius-1 emitter only
    // reaches its own cell, at full strength. emitRate 0 makes the sim run exactly one pass per tick, and with the
    // sim's linger rate of 1.1 one pass per tick settles at emitRads. The direction uses the Radbolt Reflector's
    // side screen.
    [SerializationConfig(MemberSerialization.OptIn)]
    public class RadiationFieldLamp : KMonoBehaviour, IHighEnergyParticleDirection
    {
        public const int FieldCells = 9;

        private static readonly EventSystem.IntraObjectHandler<RadiationFieldLamp> OnStateChangedDelegate =
            new EventSystem.IntraObjectHandler<RadiationFieldLamp>((component, data) => component.UpdateEmitting());

        private static readonly EventSystem.IntraObjectHandler<RadiationFieldLamp> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<RadiationFieldLamp>((component, data) => component.OnCopySettings(data));

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
#pragma warning restore CS0649

        [Serialize]
        private EightDirection direction = EightDirection.Right;

        private RadiationEmitter[] emitters;

        private MeterController meter;

        private bool emitting;

        public EightDirection Direction
        {
            get { return direction; }
            set
            {
                direction = value;
                if (isSpawned)
                {
                    ApplyDirection();
                }
            }
        }

        // The 9 cell offsets for a direction. A straight direction gives a 3x3 block whose middle row or column starts
        // next to the lamp; a diagonal gives the 3x3 block that touches the lamp at its corner.
        public static List<CellOffset> GetFieldOffsets(EightDirection direction)
        {
            Vector2I forward = GetForward(direction);
            List<CellOffset> offsets = new List<CellOffset>(FieldCells);
            if (forward.x != 0 && forward.y != 0)
            {
                for (int along = 1; along <= 3; along++)
                {
                    for (int across = 1; across <= 3; across++)
                    {
                        offsets.Add(new CellOffset(along * forward.x, across * forward.y));
                    }
                }
            }
            else
            {
                Vector2I side = new Vector2I(-forward.y, forward.x);
                for (int along = 1; along <= 3; along++)
                {
                    for (int across = -1; across <= 1; across++)
                    {
                        offsets.Add(new CellOffset(along * forward.x + across * side.x,
                            along * forward.y + across * side.y));
                    }
                }
            }
            return offsets;
        }

        public static void SetRange(RangeVisualizer visualizer, EightDirection direction)
        {
            List<CellOffset> offsets = GetFieldOffsets(direction);
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
            if (other != null)
            {
                Direction = other.Direction;
            }
        }

        private void ApplyDirection()
        {
            List<CellOffset> offsets = GetFieldOffsets(direction);
            for (int i = 0; i < emitters.Length && i < offsets.Count; i++)
            {
                emitters[i].emissionOffset = new Vector3(offsets[i].x, offsets[i].y);
                // Re-sending the current state makes the sim move the emitter to its new cell on the next tick.
                emitters[i].SetEmitting(emitting);
            }
            // The lamp art faces right. Rotation is counter-clockwise in degrees, and EightDirection counts
            // counter-clockwise from up in 45 degree steps, with Right at index 6.
            animController.Pivot = new Vector3(0f, 0.5f, 0f);
            animController.Rotation = (EightDirectionUtil.GetAngle(direction) + 90f) % 360f;
            if (rangeVisualizer != null)
            {
                SetRange(rangeVisualizer, direction);
            }
        }

        private void UpdateEmitting()
        {
            if (meter != null)
            {
                meter.SetPositionPercent(Mathf.Clamp01(storage.MassStored() / storage.capacityKg));
            }
            bool hasFuel = elementConverter == null || elementConverter.HasEnoughMassToStartConverting();
            bool on = operational.IsOperational && hasFuel;
            if (on != emitting)
            {
                SetEmitting(on);
            }
        }

        private void SetEmitting(bool on)
        {
            emitting = on;
            operational.SetActive(on);
            foreach (RadiationEmitter emitter in emitters)
            {
                emitter.SetEmitting(on);
            }
            animController.Play(on ? "on" : "off");
        }
    }
}
