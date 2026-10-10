# Fast Temp Shift Plate Build

Temperature Shift Plates (Thermal Blocks) build and deconstruct much faster and use less material, so they are practical to place and move around.

## What changes

| Temperature Shift Plate | Vanilla | With this mod |
|---|---|---|
| Build time | 120 s | 6 s |
| Deconstruct time | 30 s | 3 s |
| Material | 800 kg | 500 kg |

## How it works

| Patch | Target | Kind |
|---|---|---|
| `ThermalBlockConfig_CreateBuildingDef_Patch` | `ThermalBlockConfig.CreateBuildingDef` | Postfix: sets `Mass` and `ConstructionTime` |
| `ThermalBlockConfig_DoPostConfigureComplete_Patch` | `ThermalBlockConfig.DoPostConfigureComplete` | Postfix: sets `Deconstructable.customWorkTime` |

Deconstruct time needs its own patch. `Deconstructable.OnPrefabInit` uses half of `ConstructionTime` only for tile pieces (buildings with a `TileLayer`). Every other building, the Temperature Shift Plate included, gets a hard-coded 30 s unless `customWorkTime` is greater than 0.

## Notes

- Thermally Conductive also patches `ThermalBlockConfig.CreateBuildingDef`, but it changes a different field (`ThermalConductivity`), so the two mods combine.
- Patch targets were checked against game build 744825.
