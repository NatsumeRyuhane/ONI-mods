# Thermally Conductive

Buildings meant to move heat do it far better: Radiant Gas Pipes, Radiant Liquid Pipes, Temperature Shift Plates and Conduction Panels exchange heat 1000 times faster.

## What changes

Each of these buildings gets a **Heat Exchange Efficiency** of x1,000. It shows in the building's Properties tab, and its tooltip names where the bonus comes from.

| Building | Modifier | Vanilla `ThermalConductivity` | With this mod |
|---|---|---|---|
| Radiant Gas Pipe | Finned Tubing | 2 | 2000 |
| Radiant Liquid Pipe | Finned Tubing | 2 | 2000 |
| Temperature Shift Plate | Vapor Chamber Core | 1 (default) | 1000 |
| Conduction Panel | Thermal Interface Pads | 2 | 2000 |

`ThermalConductivity` is a multiplier on how fast a building exchanges heat, on top of its material's own conductivity. The Thermal Conductivity line in the Properties tab shows the value with the bonus included.

## How it works

The bonus is a building attribute, not a change to the shared `BuildingDef`. `HeatExchange` registers the attribute with a base of 0 and shows it in the Properties tab. Each boosted building's def gets a named attribute modifier. `BuildingComplete.OnPrefabInit` adds it to every instance before the building's temperature registers with the sim.

Wherever the game passes `BuildingDef.ThermalConductivity` on, a transpiler multiplies it by the building's attribute total. Buildings without the attribute keep their vanilla value.

| Patch | Targets | Kind |
|---|---|---|
| `Db_Initialize_Patch` | `Db.Initialize` | Postfix: registers the attribute |
| `CreateBuildingDef_Patch` | `CreateBuildingDef` on `ThermalBlockConfig`, `LiquidConduitRadiantConfig`, `GasConduitRadiantConfig`, `ContactConductivePipeBridgeConfig` | Postfix: adds the modifier |
| `StructureTemperatureComponents_Patch` | `StructureTemperatureComponents.SimRegister`, `UpdateSimState` | Transpiler: building <-> cell exchange |
| `ConduitTemperatureManager_Allocate_Patch` | `ConduitTemperatureManager.Allocate` | Transpiler: pipe contents <-> pipe exchange |
| `ContactConductivePipeBridge_Exchange_Patch` | `ContactConductivePipeBridge.Instance.ExchangeStorageTemperatureWithBuilding` | Transpiler: liquid <-> Conduction Panel exchange |
| `AdditionalDetailsPanel_Patch` | `AdditionalDetailsPanel.RefreshDetailsPanel` | Transpiler: Thermal Conductivity line |

## Notes

- Heat moves so fast that pipe contents and plates settle at their surroundings' temperature almost at once. Contents can then freeze or boil in the pipe sooner than in vanilla. No Phase Transition Damage stops the damage that would cause.
- The build menu lists building attributes as plain numbers, so it shows "Heat Exchange Efficiency: 1000" without the "x".
- The pipe overlay tint and flow visuals still read the vanilla def value, so radiant pipes look the same as in vanilla.
- The native sim computes the Conduction Panel's direct link to buildings it overlaps. Whether that link uses the conductivity sent at registration hasn't been checked.
- Fast Temp Shift Plate Build also patches the Temperature Shift Plate. It changes different fields, so the two mods combine.
- Patch targets were checked against game build 744825.
