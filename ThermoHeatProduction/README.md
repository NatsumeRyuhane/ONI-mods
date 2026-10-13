# Thermo Heat Production

Thermo Regulators and Thermo Aquatuners show a "Heat Production" status while they run, like the Steam Turbine.

## What changes

| | Vanilla | With this mod |
|---|---|---|
| Status while running | No heat status | `Heat Production: X DTU/s` |
| Status tooltip | | `This building is producing X per second`, with one source: `Transferred from pipes: X DTU/s` |

The value is the heat taken out of the gas or liquid passing through the building, which the building takes on itself. Cooling 10 kg/s of Water by 14 °C, for example, shows about 585 kDTU/s. The status is hidden while the building is idle.

Nothing in the simulation changes. With [DTU Not Included](../DTUNotIncluded), the status reads in watts, like the Steam Turbine's.

## How it works

Both buildings use the `AirConditioner` component. Each time it cools a packet, it reports the heat to `StructureTemperatureComponents.ProduceEnergy` under the source "Transferred from pipes", the same call the Steam Turbine uses for its steam heat. The game keeps a running average of those reports for the status item, but only shows the status on buildings with self heat or exhaust heat, and both are 0 kW on these two.

A Harmony postfix on `AirConditioner.UpdateStatus` sets that status item while the building is active, and clears it when the building stops. It doesn't mark the building as a heat source, which would also add a 0 kW "Normal operation" line to the tooltip.

| Patch | Target | Kind |
|---|---|---|
| `AirConditioner_UpdateStatus_Patch` | `AirConditioner.UpdateStatus` (private) | Postfix |

## Notes

- The value is the game's running average over its last 186 reports (about 37 s while running), so it takes a while to settle after the flow or temperature changes.
- `AirConditioner.UpdateStatus` and `StructureTemperatureComponents.operatingEnergyStatusItem` are private and named by string. A game update that renames either breaks the mod at load.
- The status needs the building's sim handle, which isn't ready for a moment after the building spawns. The status appears on the next update after that.
- Patch targets were checked against game build 744825.
