# BuffedAETN

Makes the Anti Entropy Thermo-Nullifier (AETN) configurable: cooling power, minimum operating temperature and Hydrogen use. It also can't be flooded any more.

With the default options, cooling, minimum temperature and Hydrogen use match vanilla. The only default differences are that the AETN can't be flooded and holds a smaller Hydrogen buffer (see the table). Raise **Cooling Power** to actually buff it.

## What changes

| | Vanilla | With this mod (defaults) |
|---|---|---|
| Cooling | 80 kW (64 kW self, 16 kW exhaust) | Option, default 80 kW, same 80/20 split |
| Minimum operating temperature | 100 K | Option, default 100 K |
| Hydrogen use | 10 g/s | Option, default 10 g/s |
| Hydrogen buffer (storage and intake) | 100 g | The larger of the option (default 10 g) and 3 s of use, so 30 g by default |
| Can be flooded | Yes | No |

## Options

Set in the game's mod menu (saved to `config.json` in the mod folder). Changes need a restart.

| Option | Default | Range | Effect |
|---|---|---|---|
| Cooling Power (kW) | 80 | 0 to 10000 | Total heat removed while active, split 80% self heat and 20% exhaust |
| Minimum Operating Temperature (K) | 100 | 0 to 300 | Below this the AETN stops |
| Hydrogen Consumption Rate (g/s) | 10 | 0 to 1000 | Hydrogen burned while active |
| Hydrogen Storage Capacity (g) | 10 | 10 to 1000 | Hydrogen buffer. Never less than 3 seconds of consumption |

## How it works

| Patch | Target | Kind |
|---|---|---|
| `MassiveHeatSinkConfig_CreateBuildingDef_Patch` | `MassiveHeatSinkConfig.CreateBuildingDef` | Postfix: `Floodable`, self heat and exhaust (negative, which is cooling) |
| `MassiveHeatSinkConfig_DoPostConfigureComplete_Patch` | `MassiveHeatSinkConfig.DoPostConfigureComplete` | Postfix: `MinimumOperatingTemperature`, `Storage` and `ConduitConsumer` capacity, `ElementConverter` Hydrogen rate |

## Notes

- A very low minimum temperature with high cooling power can push a cell's temperature toward 0 K, which the option tooltip warns may crash the game.
- Heat Reducer leaves the AETN alone: it is on Heat Reducer's untouched list, and its heat values are negative anyway.
- Patch targets were checked against game build 744825.
