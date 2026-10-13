# Battery No Power Loss

Batteries no longer lose stored power over time as heat.

## What changes

| Building | Vanilla leak | Vanilla heat | With this mod |
|---|---|---|---|
| Battery | 1.67 J/s | 1.25 kDTU/s | 0 / 0 |
| Jumbo Battery | 3.33 J/s | 1.25 kDTU/s | 0 / 0 |
| Smart Battery | 0.67 J/s | 0.5 kDTU/s | 0 / 0 |
| Battery Module (Spaced Out) | 0.67 J/s | none | 0 / none |

Vanilla batteries count as active, and give off their heat, whenever they hold any charge.

The same applies to any other building with a `Battery` component, including batteries added by other mods.

In each build menu description, the "loses charge over time" sentence becomes "My Duplicants have improved the schematics of this model, so it no longer leaks power as heat."

## How it works

`Assets.AddBuildingDef` is the last step of registering a building, after the config's `DoPostConfigureComplete`. A postfix on it sets `joulesLostPerSecond` to 0 on every `Battery` component of the building's complete prefab, and spawned buildings copy that value. It also zeroes the def's `ExhaustKilowattsWhenActive` and `SelfHeatKilowattsWhenActive`, which buildings read when they spawn, so existing saves change too. If a battery leaked before, the postfix also swaps the leak sentence in the building's `EFFECT` string.

| Patch | Target | Kind |
|---|---|---|
| `Assets_AddBuildingDef_Patch` | `Assets.AddBuildingDef` | Postfix |

## Notes

- Power transformers are skipped. They use a `Battery` component internally but set their own leak every tick.
- The description swap matches the English vanilla sentences. With a translation installed, the leak stops but the description keeps its translated leak sentence.
- Patch targets were checked against game build 744825.
