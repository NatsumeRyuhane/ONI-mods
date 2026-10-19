using System;
using System.Collections.Generic;
using KSerialization;
using UnityEngine;

namespace SublimationSconce
{
    // Keeps the sconce stocked with the materials picked in its filter, up to the capacity set on its slider. Like the
    // vanilla sconce, it only asks for more once the stock drops below 40% of the capacity, then fills it up, so
    // Duplicants don't run an errand for every few grams that sublimate away. The game's FilteredStorage refills after
    // any drop, and its storageFullMargin would make the capacity status round the stock up, so the sconce runs its
    // own fetch.
    [SerializationConfig(MemberSerialization.OptIn)]
    public class SublimationSconce : KMonoBehaviour, IUserControlledCapacity
    {
        // The vanilla sconce holds 240 kg and refills below 96 kg.
        private const float RefillFraction = 0.4f;

        private static readonly EventSystem.IntraObjectHandler<SublimationSconce> OnCopySettingsDelegate =
            new EventSystem.IntraObjectHandler<SublimationSconce>((component, data) => component.OnCopySettings(data));

        private static readonly EventSystem.IntraObjectHandler<SublimationSconce> OnStorageChangeDelegate =
            new EventSystem.IntraObjectHandler<SublimationSconce>((component, data) => component.OnStorageChange());

        // The game assigns [MyCmpGet] fields by reflection, which the compiler can't see.
#pragma warning disable CS0649
        [MyCmpGet]
        private Storage storage;

        [MyCmpGet]
        private TreeFilterable treeFilterable;

        [MyCmpGet]
        private KBatchedAnimController animController;
#pragma warning restore CS0649

        [Serialize]
        private float userMaxCapacity = float.PositiveInfinity;

        private FetchList2 fetchList;

        private MeterController meter;

        public float UserMaxCapacity
        {
            get { return Mathf.Min(userMaxCapacity, storage.capacityKg); }
            set
            {
                userMaxCapacity = value;
                RestartFetch();
                UpdateMeter();
            }
        }

        public float AmountStored
        {
            get { return storage.MassStored(); }
        }

        public float MinCapacity
        {
            get { return 0f; }
        }

        public float MaxCapacity
        {
            get { return storage.capacityKg; }
        }

        public bool WholeValues
        {
            get { return false; }
        }

        public LocString CapacityUnits
        {
            get { return GameUtil.GetCurrentMassUnit(); }
        }

        // The interface has a default for this, but C# 7.3 on .NET Framework can't use default interface members.
        public bool ControlEnabled()
        {
            return true;
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe((int)GameHashes.CopySettings, OnCopySettingsDelegate);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            meter = new MeterController(animController, "meter_target", "meter", Meter.Offset.Infront,
                Grid.SceneLayer.NoLayer, "meter_target", "meter_frame", "meter_level");
            treeFilterable.OnFilterChanged =
                (Action<HashSet<Tag>>)Delegate.Combine(treeFilterable.OnFilterChanged, (Action<HashSet<Tag>>)OnFilterChanged);
            Subscribe((int)GameHashes.OnStorageChange, OnStorageChangeDelegate);
            RefreshFetch();
            UpdateMeter();
        }

        protected override void OnCleanUp()
        {
            Unsubscribe((int)GameHashes.OnStorageChange, OnStorageChangeDelegate);
            if (treeFilterable != null)
            {
                treeFilterable.OnFilterChanged =
                    (Action<HashSet<Tag>>)Delegate.Remove(treeFilterable.OnFilterChanged, (Action<HashSet<Tag>>)OnFilterChanged);
            }
            CancelFetch();
            base.OnCleanUp();
        }

        private void OnCopySettings(object data)
        {
            GameObject source = data as GameObject;
            SublimationSconce other = source != null ? source.GetComponent<SublimationSconce>() : null;
            if (other != null)
            {
                UserMaxCapacity = other.UserMaxCapacity;
            }
        }

        private void OnFilterChanged(HashSet<Tag> tags)
        {
            RestartFetch();
        }

        private void OnStorageChange()
        {
            RefreshFetch();
            UpdateMeter();
        }

        private void OnFetchComplete()
        {
            fetchList = null;
            RefreshFetch();
        }

        private void RestartFetch()
        {
            CancelFetch();
            RefreshFetch();
        }

        private void CancelFetch()
        {
            if (fetchList != null)
            {
                fetchList.Cancel("");
                fetchList = null;
            }
        }

        // Asks for enough of the picked materials to fill the sconce, once the stock is below the refill point. A
        // fetch already under way runs until the sconce is full.
        private void RefreshFetch()
        {
            if (fetchList != null || !isSpawned)
            {
                return;
            }
            HashSet<Tag> tags = treeFilterable.GetTags();
            float capacity = UserMaxCapacity;
            float stored = storage.MassStored();
            if (tags == null || tags.Count == 0 || stored >= capacity * RefillFraction)
            {
                return;
            }
            fetchList = new FetchList2(storage, Db.Get().ChoreTypes.FetchCritical);
            fetchList.Add(tags, amount: capacity - stored);
            fetchList.Submit(OnFetchComplete, false);
        }

        private void UpdateMeter()
        {
            if (meter == null)
            {
                return;
            }
            float capacity = UserMaxCapacity;
            meter.SetPositionPercent(capacity > 0f ? Mathf.Clamp01(storage.MassStored() / capacity) : 0f);
        }
    }
}
