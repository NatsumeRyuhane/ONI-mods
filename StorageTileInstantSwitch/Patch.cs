using System.Reflection;
using HarmonyLib;

namespace StorageTileInstantSwitch
{
    public static class Patch
    {
        // Changing a Storage Tile's item moves its state machine into change.awaitingSettingsChange, whose enter
        // action calls StartChangeSettingChore. Vanilla queues a 3 s Toggle errand there, and the workable's completion
        // moves the tile to change.complete, which swaps the filter and drops the old items where the Duplicant stood.
        // This prefix skips the errand and moves to change.complete on the next frame instead. The callback is scheduled
        // in the state's scheduler group, so it is cancelled if the tile leaves the state first (for example, when the
        // player switches back to the current item).
        [HarmonyPatch(typeof(StorageTile.Instance), nameof(StorageTile.Instance.StartChangeSettingChore))]
        public static class StorageTile_Instance_StartChangeSettingChore_Patch
        {
            // LastCellWorkerUsed has a private setter. DropUndesiredItems reads it, and it is -1 until a Duplicant
            // has done the errand once.
            private static readonly MethodInfo SetLastCellWorkerUsed = AccessTools.PropertySetter(
                typeof(StorageTileSwitchItemWorkable), nameof(StorageTileSwitchItemWorkable.LastCellWorkerUsed));

            public static bool Prefix(StorageTile.Instance __instance)
            {
                StorageTile.Instance smi = __instance;
                smi.ScheduleNextFrame(_ =>
                {
                    SetLastCellWorkerUsed.Invoke(smi.GetWorkable(), new object[] { GetDropCell(smi) });
                    smi.GoTo(smi.sm.change.complete);
                });
                return false;
            }

            // The tile is solid, so its own cell can't hold the dropped items. Use the first open neighbour, trying
            // the cell above first because the tile is usually a floor. Fall back to the cell above.
            private static int GetDropCell(StorageTile.Instance smi)
            {
                int cell = Grid.PosToCell(smi.gameObject);
                int[] candidates = { Grid.CellAbove(cell), Grid.CellLeft(cell), Grid.CellRight(cell), Grid.CellBelow(cell) };
                foreach (int candidate in candidates)
                {
                    if (Grid.IsValidCell(candidate) && !Grid.Solid[candidate])
                    {
                        return candidate;
                    }
                }
                return candidates[0];
            }
        }
    }
}
