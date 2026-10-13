# Super Mop

Removes the Mop tool's 150 kg liquid limit, so Duplicants can mop any amount of liquid that sits on a floor. Mopping is also faster and reaches further, with both configurable, and an optional toggle makes it instant.

## What changes

| | Vanilla | With this mod |
|---|---|---|
| Most liquid a cell can hold and still be mopped | 150 kg | No limit |
| Mop speed | 100 kg/s per cell (20 kg every 200 ms) | 100 kg/s x multiplier (default 1000 kg/s), or each cell whole at once with Instant Mop |
| Mop reach | 1 cell to each side | Option, default 3 cells to each side |
| Bottles | A new bottle per cell every 200 ms | Added to the errand's last bottle of that element, up to 25000 kg |
| Floor needed under the marked cell | Yes | Yes (unchanged) |

## Options

Set in the game's mod menu (saved to `config.json` in the mod folder). Changes need a restart. Setting speed and reach to 1, with Instant Mop off, keeps vanilla speed and reach, with only the mass limit removed.

| Option | Default | Range | Effect |
|---|---|---|---|
| Mop Speed Multiplier | 10 | 1 to 100 | Multiplies the vanilla 20 kg each cell gives up every 200 ms |
| Mop Reach (cells to each side) | 3 | 1 to 10 | How far left and right of a mop errand's cell the Duplicant mops |
| Instant Mop | Off | | Each cell in reach is mopped whole on the first tick, ignoring the speed multiplier |

## How it works

`MopTool.maxMopAmt` is a static field read by both the Mop tool (to mark a cell) and `Moppable.IsThereLiquid` (to keep an errand alive). `ModLoad` sets it to infinity.

`MopPlacerConfig.CreatePrefab` sets the mop errand's `amountMoppedPerTick` to 20 kg. A postfix multiplies it, or with Instant Mop sets it to 1000000 kg, the amount the game's debug instant-build mop uses. The sim never takes more than a cell holds.

`Moppable` mops every cell in its private `offsets` array, which is its own cell and the cells 1 to the left and right in vanilla. A prefix on `MopTick` and `IsThereLiquid` rebuilds the array before each call, reaching out to the configured distance and stopping at the first solid cell or world edge, so the mop doesn't reach through walls.

The same prefix can also add every liquid cell connected to those cells (up, down, left and right, through non-solid cells in the same world), breadth first so the nearest cells come first, up to 1000 cells. Each element would still get its own bottle. This flood fill is turned off and has no option yet; `FloodFillEnabled` in `Patch.cs` switches it on.

`Moppable.OnCellMopped` turns each cell's share of a tick into a bottle at the errand's cell. A replacement prefix adds the share to the errand's last bottle of that element instead, while that bottle still lies loose in the same cell, isn't marked for relocation and stays under 25000 kg, the most the game merges into one pile. Temperature and germs mix the way `ElementChunk.OnAbsorb` merges ore piles. Otherwise it drops a new bottle, as vanilla does.

| Patch | Target | Kind |
|---|---|---|
| `MopPlacerConfig_CreatePrefab_Patch` | `MopPlacerConfig.CreatePrefab` | Postfix |
| `Moppable_MopTick_IsThereLiquid_Patch` | `Moppable.MopTick`, `Moppable.IsThereLiquid` (private) | Prefix |
| `Moppable_OnCellMopped_Patch` | `Moppable.OnCellMopped` (private) | Prefix, replaces the original |

## Notes

- All bottles land at the mop errand's cell, including those from the far cells in reach.
- The Duplicant stands in the marked cell, so in a deep pool it works underwater and can run out of breath.
- A wider reach means one Duplicant clears neighbouring mop errands while it works. Errands left with no liquid in reach cancel themselves after 1 s.
- The Sweepy's mopping is not changed.
- Patch targets were checked against game build 744825.
