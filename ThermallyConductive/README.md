# Thermally Conductive

Buildings meant to move heat do it far better: Radiant Gas Pipes, Radiant Liquid Pipes and Temperature Shift Plates get 1000 times their normal thermal conductivity.

## What changes

| Building | Vanilla `ThermalConductivity` | With this mod |
|---|---|---|
| Radiant Gas Pipe | 2 | 2000 |
| Radiant Liquid Pipe | 2 | 2000 |
| Temperature Shift Plate | 1 (default) | 1000 |

`ThermalConductivity` is a multiplier on how fast a building exchanges heat with the cell it sits in, on top of its material's own conductivity.

## How it works

One postfix covers all three configs. It lists them in `TargetMethods` and multiplies `ThermalConductivity` on the returned `BuildingDef`.

| Patch | Targets | Kind |
|---|---|---|
| `CreateBuildingDef_Patch` | `CreateBuildingDef` on `ThermalBlockConfig`, `LiquidConduitRadiantConfig`, `GasConduitRadiantConfig` | Postfix |

## Notes

- Heat moves so fast that pipe contents and plates settle at their surroundings' temperature almost at once. Contents can then freeze or boil in the pipe sooner than in vanilla. No Phase Transition Damage stops the damage that would cause.
- Fast Temp Shift Plate Build also patches the Temperature Shift Plate. It changes different fields, so the two mods combine.
- Patch targets were checked against game build 744825.
