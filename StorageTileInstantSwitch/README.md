# Storage Tile Instant Switch

Changing the item a Storage Tile holds takes effect right away. No Duplicant has to walk over and do the errand.

## What changes

| | Vanilla | With this mod |
|---|---|---|
| Changing the stored item | Queues a 3 s Toggle errand for a Duplicant | Applied on the next frame, with no errand |
| Items that no longer match | Dropped where the Duplicant stood | Dropped in the first open cell next to the tile: above, left, right, then below |

Everything else is unchanged: the tile still keeps only the chosen item, and Duplicants still deliver it with Storage errands.

## How it works

Picking a new item moves the Storage Tile's state machine into `change.awaitingSettingsChange`. Its enter action, `StorageTile.Instance.StartChangeSettingChore`, queues a `WorkChore` on `StorageTileSwitchItemWorkable`. When the errand completes, the state machine moves to `change.complete`, which swaps the filter tag and drops the items that no longer match at the cell in the workable's `LastCellWorkerUsed`.

A Harmony prefix on `StartChangeSettingChore` skips the errand. It schedules a callback for the next frame that sets `LastCellWorkerUsed` to an open cell next to the tile and moves the state machine to `change.complete`. The callback is scheduled in the state's scheduler group, so the game cancels it if the tile leaves the state first, for example when the player switches back to the current item.

| Patch | Target | Kind |
|---|---|---|
| `StorageTile_Instance_StartChangeSettingChore_Patch` | `StorageTile.Instance.StartChangeSettingChore` | Prefix, skips original |

## Notes

- The "Set Storage" status shows for one frame while the change is applied.
- `LastCellWorkerUsed` has a private setter, which the patch calls through reflection. A game update that changes the property breaks the patch. Check the game log after updating.
- If the tile is walled in on all four sides, the dropped items go to the cell above it even though that cell is solid.
- Patch targets were checked against game build 744825.
