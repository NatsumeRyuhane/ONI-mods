# BuffedAETN

Gives each Anti Entropy Thermo-Nullifier (AETN) two sliders: cooling power and minimum operating temperature. Changes apply right away. Hydrogen use follows the cooling power. The AETN also can't be flooded any more.

A new AETN, or one from a save made without the mod, starts at the vanilla values: 80 kW, 100 K and 10 g/s of Hydrogen.

## What changes

| | Vanilla | With this mod |
|---|---|---|
| Cooling | 80 kW (64 kW self, 16 kW exhaust) | Slider per AETN, 0 kW up to the **Maximum Cooling Power** option, same 80/20 split |
| Minimum operating temperature | 100 K | Slider per AETN, from a floor set by the cooling power (see below) up to 300 K |
| Hydrogen use | 10 g/s | Cooling power ÷ the **Cooling per Gram of Hydrogen** option (default 8 kDTU/g, so 10 g/s at 80 kW) |
| Hydrogen buffer (storage and intake) | 100 g | 10 seconds of use, the same as vanilla |
| Can be flooded | Yes | No |
| Copy settings | No | Yes, copies both sliders |

### Minimum temperature floor

The lowest minimum temperature is `ceil(0.05 K × cooling power in kW)`: 4 K at 80 kW, 50 K at 1000 kW, 100 K at 2000 kW. Raising the cooling power pushes the minimum temperature up to the new floor if it was below it.

The AETN takes a few 200 ms ticks to stop after it gets too cold. The floor keeps it from cooling itself or the cells it covers below 0 K in that time, which can crash the game. The worst case is the exhaust into a thin cell of a gas with low heat capacity: 1.5 kg of Chlorine loses about 0.0035 K per kW each tick, so the floor leaves room for several ticks with margin.

## Options

Set in the game's mod menu (saved to `config.json` in the mod folder). Changes need a restart.

| Option | Default | Range | Effect |
|---|---|---|---|
| Maximum Cooling Power | 2000 kW | 80 to 10000 | The top of the cooling power slider. AETNs above a lowered limit are clamped to it when loaded |
| Cooling per Gram of Hydrogen | 8 kDTU/g | 0 to 10000 | Heat removed for each gram of Hydrogen burned. Higher uses less Hydrogen. 0 makes the AETN free to run |

Hydrogen use in g/s is the cooling power in kW divided by this option. For example, 2000 kW at 400 kDTU/g burns 5 g/s. A gas pipe carries at most 1 kg/s, which feeds 8000 kW at the default.

## How it works

| Patch | Target | Kind |
|---|---|---|
| `Db_Initialize_Patch` | `Db.Initialize` | Postfix: side screen strings |
| `MassiveHeatSinkConfig_CreateBuildingDef_Patch` | `MassiveHeatSinkConfig.CreateBuildingDef` | Postfix: `Floodable`, remembers the def |
| `MassiveHeatSinkConfig_DoPostConfigureComplete_Patch` | `MassiveHeatSinkConfig.DoPostConfigureComplete` | Postfix: adds `AdjustableAetn` and `CopyBuildingSettings` |
| `StructureTemperatureComponents_Heat_Patch` | `StructureTemperatureComponents.Sim200ms`, `UpdateSimState`, `SimRegister` | Transpiler: the self heat and exhaust getters read the AETN's own cooling power |

`AdjustableAetn` holds the two saved slider values and shows them with the game's multi slider side screen. When a value changes it sets the `ElementConverter` Hydrogen rate, the `Storage` and `ConduitConsumer` capacity and the `MinimumOperatingTemperature`, and marks the structure temperature dirty so the sim picks up the new self heat.

The building def is shared by every AETN, so its heat values stay at vanilla. The game reads self heat and exhaust through two small getters on `StructureTemperaturePayload`. The mod rewrites the calls to them, not the getters themselves, because the JIT can inline small getters and skip a patch on them.

## Notes

- Heat Reducer leaves the AETN alone: it is on Heat Reducer's untouched list, and its heat values are negative anyway.
- Patch targets were checked against game build 744825.
