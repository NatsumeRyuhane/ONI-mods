# BuffedWheezewort

Makes the Wheezewort a configurable cooler. You can set how much gas it breathes, how much it cools that gas, whether it may cool gas below the gas's condensation point, how much Phosphorite it uses, its growth temperature range and its radiation. By default it emits no radiation and tolerates a much colder range of temperatures.

## What changes

| | Vanilla | With this mod (defaults) |
|---|---|---|
| Gas breathed, planted | 1 kg/s | Option, default 1 kg/s |
| Gas breathed, wild | 0.25 kg/s (a quarter) | Option, default a quarter of the planted rate |
| Cooling of the gas it breathes | 5 degrees | Option, default 5 degrees |
| Coldest output gas | 5 degrees above the gas's condensation point | Option, default the same. When on, no floor (above absolute zero) |
| Phosphorite use | 4 kg/cycle | Option, default 6 kg/cycle (0.01 kg/s) |
| Grows between | 213.15 K and 368.15 K (-60 C and 95 C) | Option, default 173.15 K and 373.15 K (-100 C and 100 C) |
| Dies below / above | 183.15 K / 463.15 K (-90 C / 190 C) | 30 K below / 95 K above the growth range: 143.15 K / 468.15 K by default |
| Radiation (Spaced Out only) | 480 rads | Option, default 0 |

The heat a Wheezewort removes is its gas throughput × the cooling delta × the gas's specific heat capacity. For example, 1 kg/s of Hydrogen cooled by 5 degrees removes about 12 kDTU/s.

## Options

Set in the game's mod menu (saved to `config.json` in the mod folder). Changes need a restart.

| Option | Default | Range | Effect |
|---|---|---|---|
| Radiation Emission (rads) | 0 | 0 to 10000 | Radiation the plant emits. Has no effect without Spaced Out |
| Phosphorus Consumption Rate (kg/s) | 0.01 | 0.0001 to 100 | Phosphorite fertilizer used per second. 0.01 is 6 kg per cycle |
| Emission Temperature Delta | -5 | -500 to 0 | How much the plant cools the gas it breathes |
| Cool Below Condensation | Off | | Lets the plant cool gas past its condensation point, so it comes out as liquid |
| Gas Throughput (kg/s) | 1 | 0.1 to 100 | Gas a planted Wheezewort breathes |
| Wild Throughput Multiplier | 0.25 | 0 to 1 | Share of the throughput a wild Wheezewort breathes |
| Minimum Growth Temperature (K) | 173.15 | 35 to 373.15 | The plant dies 30 K below this |
| Maximum Growth Temperature (K) | 373.15 | 73.15 to 1273.15 | The plant dies 95 K above this. Raised to 1 K above the minimum if set lower |

## How it works

| Patch | Target | Kind |
|---|---|---|
| `ColdBreatherConfig_CreatePrefab_Patch` | `ColdBreatherConfig.CreatePrefab` | Postfix: radiation, the existing Phosphorite `ManualDeliveryKG` and `FertilizationMonitor.Def.consumedElements`, `TemperatureVulnerable` range, `ColdBreather.deltaEmitTemperature` and `consumptionRate`, and the `ElementConsumer` wild rate and buffer (2 seconds of throughput, at least the vanilla 2 kg) |
| `ColdBreather_OnReplanted_Patch` | `ColdBreather.OnReplanted` (private) | Postfix: sets the wild rate and the radiation again after the game resets them to a quarter and 480 rads |
| `ColdBreather_Exhale_Patch` | `ColdBreather.Exhale` (private) | Transpiler: replaces the `Mathf.Max(element.lowTemp + 5, temperature + delta)` clamp, so Cool Below Condensation can drop the floor |

## Known issues

- With Cool Below Condensation on, the gas comes out as liquid in the tile above the plant. It can fall back onto the plant and drown it, so give it somewhere to drain.
- Versions before this one called `EntityTemplates.ExtendPlantToFertilizable` a second time, which gave each plant two Phosphorite delivery requests and added the fertilizer attribute twice. The mod now changes the existing delivery and fertilizer list instead.
- Versions before the null check threw a `NullReferenceException` while building the Wheezewort prefab when Spaced Out was not active, because the vanilla plant only has a `RadiationEmitter` with radiation enabled.
- The Phosphorite option's tooltip used to say kg/cycle. The value has always been kg/s.
- `ColdBreather.OnReplanted` and `ColdBreather.Exhale` are named by string, so a game update that renames them breaks the patches at load time. The transpiler throws if `Exhale` no longer calls `Mathf.Max(float, float)`.
- Patch targets were checked against the `Assembly-CSharp.dll` with SHA-256 `a58e04d0…f5ae599`.
