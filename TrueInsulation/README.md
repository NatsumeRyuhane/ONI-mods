# True Insulation

Insulated pipes, Insulation Tiles and pressure doors become perfect insulators: no heat passes through them at all.

## What changes

| Building | Vanilla `ThermalConductivity` | With this mod |
|---|---|---|
| Insulated Gas Pipe | 1/32 | 0 |
| Insulated Liquid Pipe | 1/32 | 0 |
| Insulation Tile | 0.01 | 0 |
| Pressure Door (mechanized) | 1 | 0, and its cells are insulated |
| Manual Pressure Door | 1 | 0, and its cells are insulated |

## How it works

Pipes and tiles use their `BuildingDef.ThermalConductivity` directly, so setting it to 0 is enough. Doors don't. Their cells' insulation is a separate sim value, so the mod adds a small component, `InsulatorModified`, to each pressure door. It sets the sim insulation of the door's cells to the door's conductivity (0) when the door spawns and resets it to 1 when the door is removed.

| Patch | Target | Kind |
|---|---|---|
| `CreateBuildingDef_Patch` | The parameterless `CreateBuildingDef` of the five configs above | Postfix: `ThermalConductivity = 0` |
| `BuildingComplete_OnSpawn_Patch` | `BuildingComplete.OnSpawn` (protected) | Postfix: adds `InsulatorModified` to pressure doors and applies it |

## Notes

- Door cells stay insulated while the door is open, too. `Door` only changes cell insulation for doors with an `insulationModifier` other than 1 (such as the vanilla Insulated Door), so nothing resets it.
- The `BuildingComplete.OnSpawn` postfix runs for every building that spawns, and returns at once unless it is a pressure door.
- Older versions defined `InsulatorModified` outside any namespace. It now lives in `TrueInsulation`. Saves store components by type name, but this one has no saved fields, and the game skips stored components it no longer recognizes. Existing doors get the component again when they spawn.
- `BuildingComplete.OnSpawn` is protected and named by string, so a game update that renames it breaks the patch at load time.
- Patch targets were checked against game build 744825.
