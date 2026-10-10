# BuffedWheezewort

Makes the Wheezewort a better cooler: its radiation, cooling strength and Phosphorite use are configurable, and it tolerates a much colder range of temperatures. By default it emits no radiation.

## What changes

| | Vanilla | With this mod (defaults) |
|---|---|---|
| Radiation (Spaced Out only) | 480 rads | Option, default 0 |
| Cooling of the gas it breathes | 5 degrees | Option, default 5 degrees |
| Phosphorite use | 4 kg/cycle | Option, default 6 kg/cycle (0.01 kg/s) |
| Grows between | -60 C and 95 C | -100 C and 100 C |
| Dies below / above | -90 C / 190 C | -150 C / 150 C |

The temperature range is always applied and has no option. Note that it narrows the upper lethal limit, from 190 C to 150 C.

## Options

Set in the game's mod menu (saved to `config.json` in the mod folder). Changes need a restart.

| Option | Default | Range | Effect |
|---|---|---|---|
| Radiation Emission (rads) | 0 | 0 to 10000 | Radiation the plant emits. Has no effect without Spaced Out |
| Phosphorus Consumption Rate (kg/s) | 0.01 | 0 to 100 | Phosphorite fertilizer used per second. 0.01 is 6 kg per cycle |
| Emission Temperature Delta | -5 | -100 to -5 | How much the plant cools the gas it breathes |

## How it works

| Patch | Target | Kind |
|---|---|---|
| `ColdBreatherConfig_CreatePrefab_Patch` | `ColdBreatherConfig.CreatePrefab` | Postfix: radiation, fertilizer, `TemperatureVulnerable` range, `ColdBreather.deltaEmitTemperature` |
| `ColdBreather_OnReplanted_Patch` | `ColdBreather.OnReplanted` (private) | Postfix: sets the radiation again after the game resets it to 480 |

## Known issues

- The vanilla Wheezewort is already fertilizable (4 kg/cycle of Phosphorite). The mod calls `EntityTemplates.ExtendPlantToFertilizable` a second time, which adds a second `ManualDeliveryKG` for Phosphorite and adds the fertilizer attribute again. The plant consumes at the mod's rate, because the second call replaces the fertilizer list, but it has two delivery requests. Changing the existing delivery and fertilizer list instead would fix this. That would change gameplay, so it hasn't been done without an in-game test.
- Versions before the null check threw a `NullReferenceException` while building the Wheezewort prefab when Spaced Out was not active, because the vanilla plant only has a `RadiationEmitter` with radiation enabled.
- The Phosphorite option's tooltip used to say kg/cycle. The value has always been kg/s.
- `ColdBreather.OnReplanted` is named by string, so a game update that renames it breaks the patch at load time.
- Patch targets were checked against game build 744825.
