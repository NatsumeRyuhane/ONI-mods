# Efficient Tune-Up

Engie's Tune-Up makes a generator waste less instead of making energy from nothing. In vanilla, a tune-up raises a generator's power by half for the same fuel and the same heat. With this mod the power still rises by half for the same fuel, and the generator gives off that much less heat: the extra power comes out of what used to be waste heat.

## What changes

A tune-up cuts the heat by the extra power it gives, never below none:

| Generator | Power, untuned → tuned | Heat, vanilla tuned | Heat, tuned with this mod |
|---|---|---|---|
| Coal Generator | 600 → 900 W | 9 kW | 8.7 kW |
| Wood Burner | 300 → 450 W | 9 kW | 8.85 kW |
| Peat Burner | 480 → 720 W | 4.5 kW | 4.26 kW |
| Hydrogen Generator | 800 → 1,200 W | 4 kW | 3.6 kW |
| Natural Gas Generator | 800 → 1,200 W | 10 kW | 9.6 kW |
| Petroleum Generator | 2,000 → 3,000 W | 20 kW | 19 kW |
| Tidal Turbine | 300 → 450 W | 0.625 kW | 0.475 kW |
| Steam Turbine | up to 850 → 1,275 W | 4 kW | 3.575 kW at full power |

The Steam Turbine and Tidal Turbine often run below their rated power. The cut follows the power they actually make: a tuned turbine making 600 W gives off 200 W less heat.

Untuned generators, the fuel they burn and the power they make are unchanged. Generators from other mods that take a tune-up are covered too.

### In game

- The heat cut shows as its own line, "Engie's Tune-Up", in the "Heat Production" tooltip of a tuned generator that is running.
- The cut is split between exhaust heat and self heat as the building splits them: a Coal Generator gives off 8/9 of its heat as exhaust, so 8/9 of the cut comes off the exhaust.

## How it works

A building's exhaust and self heat come from its def, which every building of a kind shares, so the heat can't be lowered for one tuned building. Instead, the mod takes the cut back out through the calls the game uses to give the heat off.

`Tinkerable.MakePowerTinkerable`, the helper every power tune-up building calls in its config, gets a postfix that adds a `TuneUpHeat` component and the `EfficientTuneUp` tag to the building. A postfix on `Generator.GenerateJoules` adds up the energy each such generator makes. Every `Sim200ms` while the building is active, `TuneUpHeat` works out the tuned share of that energy, (efficiency − 1) / efficiency, caps it at the heat the def gives off over the tick, and takes it out:

- exhaust heat from the cells the building covers, through `StructureTemperatureComponents.ExhaustHeat` with a negative rate, so it scales with the gas in each cell exactly as the game's own exhaust does;
- self heat from the building, through `SimMessages.ModifyBuildingEnergy`.

It also records the cut as a heat source for the "Heat Production" status, through the private `StructureTemperatureComponents.AccumulateProducedEnergyKW`, and drops that line once the tune-up wears off.

| Patch | Target | Kind |
|---|---|---|
| `Tinkerable_MakePowerTinkerable_Patch` | `Tinkerable.MakePowerTinkerable` | Postfix: adds `TuneUpHeat` and the `EfficientTuneUp` tag |
| `Generator_GenerateJoules_Patch` | `Generator.GenerateJoules` | Postfix: counts the energy a tuned-up building's generator makes |

## For other mods

Buildings this mod changes have the `EfficientTuneUp` tag. A mod that charges fuel for a building's heat can check it and subtract the tune-up's extra power from the heat, down to none. The [Food Generator](../FoodGenerator) does this.

## Notes

- `AccumulateProducedEnergyKW` is private and named by string, so a game update that renames it breaks the mod at load time. Check the game log after updating.
- Patch targets were checked against game build 744825.
