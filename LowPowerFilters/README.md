# Low Power Filters

Gas, Liquid and Solid Filters draw a configurable amount of power instead of 120 W each.

## What changes

| Building | Vanilla | With this mod (default) |
|---|---|---|
| Gas Filter | 120 W | 10 W |
| Liquid Filter | 120 W | 10 W |
| Solid Filter | 120 W | 10 W |

Heat output is unchanged. In vanilla the Liquid Filter gives off 4 kW while active and the other two give off none.

## Options

Set in the game's mod menu (saved to `config.json` in the mod folder). Changes need a restart.

| Option | Default | Range |
|---|---|---|
| Gas Filter Power Consumption (W) | 10 | 0 to 1000 |
| Liquid Filter Power Consumption (W) | 10 | 0 to 1000 |
| Solid Filter Power Consumption (W) | 10 | 0 to 1000 |

## How it works

| Patch | Target | Kind |
|---|---|---|
| `GasFilterConfig_CreateBuildingDef_Patch` | `GasFilterConfig.CreateBuildingDef` | Postfix: sets `EnergyConsumptionWhenActive` |
| `LiquidFilterConfig_CreateBuildingDef_Patch` | `LiquidFilterConfig.CreateBuildingDef` | Postfix: sets `EnergyConsumptionWhenActive` |
| `SolidFilterConfig_CreateBuildingDef_Patch` | `SolidFilterConfig.CreateBuildingDef` | Postfix: sets `EnergyConsumptionWhenActive` |

## Notes

- With Heat Reducer installed, the Liquid Filter's heat is capped at its new power draw (4 kW down to 10 W at the default). Heat Reducer runs after `CreateBuildingDef`, so it always sees this mod's value.
- Patch targets were checked against game build 744825.
