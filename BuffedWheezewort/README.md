# BuffedWheezewort

Makes the Wheezewort a configurable cooler. You can set how much gas it breathes, how much it cools that gas, whether it may cool gas below the gas's condensation point, how much Phosphorite it uses, its growth temperature range and its radiation. Every option defaults to the vanilla value, so an unchanged install plays like vanilla.

## What changes

| | Vanilla | With this mod (defaults) |
|---|---|---|
| Gas breathed, planted | 1 kg/s | Option, default 1 kg/s |
| Gas breathed, wild | 0.25 kg/s (a quarter) | Option, default a quarter of the planted rate |
| Cooling of the gas it breathes | 5 degrees | Option, default 5 degrees |
| Coldest output gas | 5 degrees above the gas's condensation point | Option, default the same. When on, no floor (above absolute zero) |
| Phosphorite use | 4000 g/cycle | Option, default 4000 g/cycle. At 0 the plant needs no fertilizer |
| Grows between | 213.15 K and 368.15 K (-60 C and 95 C) | Option, default the same |
| Dies below / above | 183.15 K / 463.15 K (-90 C / 190 C) | 30 K below / 95 K above the growth range, the same margins as vanilla |
| Radiation (Spaced Out only) | 480 rads | Option, default 480 rads |

The heat a Wheezewort removes is its gas throughput × the cooling delta × the gas's specific heat capacity. For example, 1 kg/s of Hydrogen cooled by 5 degrees removes about 12 kDTU/s.

## Options

Set in the game's mod menu (saved to `config.json` in the mod folder). Changes need a restart.

| Option | Default | Range | Effect |
|---|---|---|---|
| Radiation Emission (rads) | 480 | 0 to 10000 | Radiation the plant emits. Has no effect without Spaced Out |
| Phosphorite Use (g/cycle) | 4000 | 0 to 10000 | Phosphorite fertilizer a planted Wheezewort uses per cycle. At 0 it has no fertilizer delivery and never starves |
| Emission Temperature Delta | -5 | -500 to 0 | How much the plant cools the gas it breathes |
| Cool Below Condensation | Off | | Lets the plant cool gas past its condensation point, so it comes out as liquid |
| Gas Throughput (kg/s) | 1 | 0.1 to 100 | Gas a planted Wheezewort breathes |
| Wild Throughput Multiplier | 0.25 | 0 to 1 | Share of the throughput a wild Wheezewort breathes |
| Minimum Growth Temperature (K) | 213.15 | 35 to 373.15 | The plant dies 30 K below this |
| Maximum Growth Temperature (K) | 368.15 | 73.15 to 1273.15 | The plant dies 95 K above this. Raised to 1 K above the minimum if set lower |

## How it works

| Patch | Target | Kind |
|---|---|---|
| `ColdBreatherConfig_CreatePrefab_Patch` | `ColdBreatherConfig.CreatePrefab` | Postfix: radiation, the existing Phosphorite `ManualDeliveryKG` and `FertilizationMonitor.Def.consumedElements` (or, at 0 g/cycle, removes both), `TemperatureVulnerable` range, `ColdBreather.deltaEmitTemperature` and `consumptionRate`, and the `ElementConsumer` wild rate and buffer (2 seconds of throughput, at least the vanilla 2 kg) |
| `ColdBreather_OnReplanted_Patch` | `ColdBreather.OnReplanted` (private) | Postfix: sets the wild rate and the radiation again after the game resets them to a quarter and 480 rads |
| `ColdBreather_Exhale_Patch` | `ColdBreather.Exhale` (private) | Transpiler: replaces the `Mathf.Max(element.lowTemp + 5, temperature + delta)` clamp, so Cool Below Condensation can drop the floor |

## Known issues

- With Cool Below Condensation on, the gas comes out as liquid in the tile above the plant. It can fall back onto the plant and drown it, so give it somewhere to drain.
- Versions before this one called `EntityTemplates.ExtendPlantToFertilizable` a second time, which gave each plant two Phosphorite delivery requests and added the fertilizer attribute twice. The mod now changes the existing delivery and fertilizer list instead.
- Versions before the null check threw a `NullReferenceException` while building the Wheezewort prefab when Spaced Out was not active, because the vanilla plant only has a `RadiationEmitter` with radiation enabled.
- The Phosphorite option used to be `PhosphorusConsumptionRate` in kg/s (its tooltip once said kg/cycle). It is now `PhosphoriteGramsPerCycle` in g/cycle, so a saved `config.json` falls back to 4000 g/cycle until it is set again.
- With Phosphorite Use at 0, Phosphorite already delivered to a plant in an existing save stays in the plant's storage. It is dropped when the plant is uprooted.
- `ColdBreather.OnReplanted` and `ColdBreather.Exhale` are named by string, so a game update that renames them breaks the patches at load time. The transpiler throws if `Exhale` no longer calls `Mathf.Max(float, float)`.
- Patch targets were checked against the `Assembly-CSharp.dll` with SHA-256 `a58e04d0…f5ae599`.
