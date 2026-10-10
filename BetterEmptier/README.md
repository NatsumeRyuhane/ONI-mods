# Better Emptier

Bottle Emptiers and Gas Bottle Emptiers (Canister Emptiers) get a configurable storage capacity and release speed. The defaults match vanilla, so the mod changes nothing until you set the options.

## What changes

| | Vanilla | With this mod |
|---|---|---|
| Storage capacity (both emptiers) | 200 kg | Option, default 200 kg |
| Bottle Emptier release rate | 10 kg/s | 10 kg/s x multiplier |
| Gas Bottle Emptier release rate | 0.25 kg/s | 0.25 kg/s x multiplier |

## Options

Set in the game's mod menu (saved to `config.json` in the mod folder). Changes need a restart.

| Option | Default | Range | Effect |
|---|---|---|---|
| Emptier Capacity (kg) | 200 | 10 to 100000 | Storage capacity of both emptiers |
| Release Speed Multiplier | 1.0 | 0.1 to 1000 | Multiplies each emptier's vanilla release rate |

## How it works

Both configs create their `Storage` and `BottleEmptier` components in `ConfigureBuildingTemplate`. A postfix on each sets `Storage.capacityKg` and multiplies `BottleEmptier.emptyRate`.

| Patch | Target | Kind |
|---|---|---|
| `BottleEmptierConfig_ConfigureBuildingTemplate_Patch` | `BottleEmptierConfig.ConfigureBuildingTemplate` | Postfix |
| `BottleEmptierGasConfig_ConfigureBuildingTemplate_Patch` | `BottleEmptierGasConfig.ConfigureBuildingTemplate` | Postfix |

## Notes

- The pipe-output emptiers (`BottleEmptierConduitLiquidConfig`, `BottleEmptierConduitGasConfig`) and the Gunk Emptier are not changed.
- A high multiplier dumps a whole bottle into one cell almost at once, which can make very high pressure.
- Patch targets were checked against game build 744825.
