using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Klei;
using PeterHan.PLib.Options;
using UnityEngine;

namespace SuperMop
{
    public static class Patch
    {
        // Vanilla only lets the Mop tool mark a cell holding 150 kg of liquid or less, and Moppable.IsThereLiquid
        // ignores heavier cells, so a mop errand next to a deep cell cancels itself. Both read MopTool.maxMopAmt, so
        // raising it removes the limit everywhere. The Mop tool still needs a floor under the marked cell.
        public static void RemoveMassLimit()
        {
            MopTool.maxMopAmt = float.PositiveInfinity;
        }

        [HarmonyPatch(typeof(MopPlacerConfig), nameof(MopPlacerConfig.CreatePrefab))]
        public static class MopPlacerConfig_CreatePrefab_Patch
        {
            // The sim consumes at most what a cell holds, so the instant mop asks for the same 1000000 kg as the game's
            // debug instant-build mop and takes each cell whole on the first tick.
            private const float InstantMopAmount = 1000000f;

            // Vanilla sets amountMoppedPerTick to 20 kg. A Duplicant mops that much from each cell in reach every
            // 200 ms, so 100 kg/s per cell.
            public static void Postfix(GameObject __result)
            {
                var config = SingletonOptions<Config>.Instance;
                var moppable = __result.GetComponent<Moppable>();
                moppable.amountMoppedPerTick = config.InstantMop
                    ? InstantMopAmount
                    : moppable.amountMoppedPerTick * config.MopSpeedMultiplier;
            }
        }

        // Moppable mops every cell in its private offsets array: its own cell and the cells 1 to the left and right in
        // vanilla. MopTick mops them and IsThereLiquid keeps the errand alive while any of them holds liquid. Before
        // either runs, this prefix rebuilds the array to reach MopReach cells to each side, stopping at the first solid
        // cell so the mop can't reach through a wall. Walls can be built or dug at any time, so it is rebuilt on every
        // call, but a new array is only allocated when the cells change.
        // With FloodFillEnabled, the array also takes every liquid cell connected to those cells, breadth first so the
        // nearest cells come first, up to FloodFillMaxCells cells in all. Each element still becomes its own bottle.
        // MopTick, IsThereLiquid and offsets are private, so they are named by string.
        [HarmonyPatch]
        public static class Moppable_MopTick_IsThereLiquid_Patch
        {
            private static readonly AccessTools.FieldRef<Moppable, CellOffset[]> Offsets =
                AccessTools.FieldRefAccess<Moppable, CellOffset[]>("offsets");

            // Flood fill is turned off for now, and has no option in the mod menu. Set this to true to mop whole
            // connected liquid bodies. It is readonly rather than const so the disabled branch still compiles cleanly.
            private static readonly bool FloodFillEnabled = false;

            private const int FloodFillMaxCells = 1000;

            private static readonly List<CellOffset> Buffer = new List<CellOffset>();

            private static readonly Queue<int> Frontier = new Queue<int>();

            private static readonly HashSet<int> Visited = new HashSet<int>();

            public static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(Moppable), "MopTick");
                yield return AccessTools.Method(typeof(Moppable), "IsThereLiquid");
            }

            public static void Prefix(Moppable __instance)
            {
                int cell = Grid.PosToCell(__instance);
                if (!Grid.IsValidCell(cell))
                {
                    return;
                }

                var config = SingletonOptions<Config>.Instance;
                Buffer.Clear();
                Buffer.Add(new CellOffset(0, 0));
                AddReachableCells(cell, 1, config.MopReach);
                AddReachableCells(cell, -1, config.MopReach);
                if (FloodFillEnabled)
                {
                    AddConnectedLiquid(cell, FloodFillMaxCells);
                }

                ref CellOffset[] offsets = ref Offsets(__instance);
                if (!SameCells(offsets, Buffer))
                {
                    offsets = Buffer.ToArray();
                }
            }

            private static void AddReachableCells(int cell, int direction, int reach)
            {
                for (int distance = 1; distance <= reach; distance++)
                {
                    var offset = new CellOffset(direction * distance, 0);
                    int target = Grid.OffsetCell(cell, offset);
                    if (!Grid.IsValidCell(target) || !Grid.AreCellsInSameWorld(target, cell) || Grid.Solid[target])
                    {
                        return;
                    }
                    Buffer.Add(offset);
                }
            }

            private static void AddConnectedLiquid(int cell, int maxCells)
            {
                Frontier.Clear();
                Visited.Clear();
                foreach (CellOffset offset in Buffer)
                {
                    int start = Grid.OffsetCell(cell, offset);
                    Visited.Add(start);
                    if (Grid.Element[start].IsLiquid)
                    {
                        Frontier.Enqueue(start);
                    }
                }

                while (Frontier.Count > 0 && Buffer.Count < maxCells)
                {
                    int current = Frontier.Dequeue();
                    VisitLiquid(cell, Grid.CellLeft(current), maxCells);
                    VisitLiquid(cell, Grid.CellRight(current), maxCells);
                    VisitLiquid(cell, Grid.CellAbove(current), maxCells);
                    VisitLiquid(cell, Grid.CellBelow(current), maxCells);
                }
            }

            private static void VisitLiquid(int origin, int cell, int maxCells)
            {
                if (Buffer.Count >= maxCells || !Grid.IsValidCell(cell) || !Grid.AreCellsInSameWorld(cell, origin)
                    || Grid.Solid[cell] || !Grid.Element[cell].IsLiquid || !Visited.Add(cell))
                {
                    return;
                }
                Buffer.Add(new CellOffset(Grid.CellColumn(cell) - Grid.CellColumn(origin), Grid.CellRow(cell) - Grid.CellRow(origin)));
                Frontier.Enqueue(cell);
            }

            private static bool SameCells(CellOffset[] offsets, List<CellOffset> cells)
            {
                if (offsets == null || offsets.Length != cells.Count)
                {
                    return false;
                }
                for (int i = 0; i < offsets.Length; i++)
                {
                    if (offsets[i] != cells[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        // Vanilla turns each cell's share of a mop tick into a new bottle at the mop errand's cell, so a wide or
        // flood-filled mop could drop hundreds of bottles a second. This replacement adds each share to the last bottle
        // of that element the errand dropped instead, while it still lies loose in the same cell. It mixes temperature
        // and germs the way ElementChunk.OnAbsorb merges ore piles, and stops a bottle at the same 25000 kg; past that,
        // or once the bottle is picked up, moved or marked for relocation, a new bottle starts. Merging directly instead
        // of through Pickupable.TryAbsorb skips a throwaway bottle and the absorb sound for every share.
        // OnCellMopped, amountMopped and lastElementMopped are private, so they are named by string.
        [HarmonyPatch(typeof(Moppable), "OnCellMopped")]
        public static class Moppable_OnCellMopped_Patch
        {
            private static readonly AccessTools.FieldRef<Moppable, float> AmountMopped =
                AccessTools.FieldRefAccess<Moppable, float>("amountMopped");

            private static readonly AccessTools.FieldRef<Moppable, Element> LastElementMopped =
                AccessTools.FieldRefAccess<Moppable, Element>("lastElementMopped");

            // ElementSplitterComponents.CanFirstAbsorbSecond stops merging piles at this mass.
            private const float MaxBottleMass = 25000f;

            private static readonly ConditionalWeakTable<Moppable, Dictionary<ushort, Pickupable>> Bottles =
                new ConditionalWeakTable<Moppable, Dictionary<ushort, Pickupable>>();

            public static bool Prefix(Moppable __instance, Sim.MassConsumedCallback mass_cb_info)
            {
                if (__instance == null || mass_cb_info.mass <= 0f)
                {
                    return false;
                }

                // Read by Sim1000ms for the "-X kg" popup, as in vanilla.
                AmountMopped(__instance) += mass_cb_info.mass;
                Element element = ElementLoader.elements[mass_cb_info.elemIdx];
                LastElementMopped(__instance) = element;

                int cell = Grid.PosToCell(__instance);
                Dictionary<ushort, Pickupable> bottles = Bottles.GetOrCreateValue(__instance);
                if (bottles.TryGetValue(mass_cb_info.elemIdx, out Pickupable previous) && CanMergeInto(previous, cell, mass_cb_info.mass))
                {
                    PrimaryElement primaryElement = previous.PrimaryElement;
                    float temperature = SimUtil.CalculateFinalTemperature(primaryElement.Mass, primaryElement.Temperature,
                        mass_cb_info.mass, mass_cb_info.temperature);
                    primaryElement.SetMassTemperature(primaryElement.Mass + mass_cb_info.mass, temperature);
                    primaryElement.AddDisease(mass_cb_info.diseaseIdx, mass_cb_info.diseaseCount, "Moppable.OnCellMopped");
                    return false;
                }

                SubstanceChunk chunk = LiquidSourceManager.Instance.CreateChunk(element, mass_cb_info.mass,
                    mass_cb_info.temperature, mass_cb_info.diseaseIdx, mass_cb_info.diseaseCount,
                    Grid.CellToPosCCC(cell, Grid.SceneLayer.Ore));
                // Same small random spread as vanilla, so separate bottles don't stack exactly.
                chunk.transform.SetPosition(chunk.transform.GetPosition() + new Vector3((Random.value - 0.5f) * 0.5f, 0f, 0f));
                bottles[mass_cb_info.elemIdx] = chunk.GetComponent<Pickupable>();
                return false;
            }

            private static bool CanMergeInto(Pickupable bottle, int cell, float mass)
            {
                return bottle != null
                    && !bottle.wasAbsorbed
                    && bottle.storage == null
                    && Grid.PosToCell(bottle) == cell
                    && !bottle.GetComponent<KPrefabID>().HasTag(GameTags.MarkedForMove)
                    && bottle.PrimaryElement.Mass + mass < MaxBottleMass;
            }
        }
    }
}
