# Electrolyzer No Overpressure

Electrolyzers never stop because of overpressure. They keep splitting water into Oxygen and Hydrogen however much gas has built up around them.

## What changes

| | Vanilla | With this mod |
|---|---|---|
| Overpressure check | Stops when any gas cell within 3 cells of the output holds more than 1.8 kg | Never stops for pressure |

Everything else is unchanged: the Electrolyzer still needs water and power, and still stops when it is disabled or out of water.

## How it works

The Electrolyzer's state machine moves from `converting` to `overpressure` while its private `RoomForPressure` property is `false`. `RoomForPressure` flood-fills up to 3 cells from the output cell and looks for a cell heavier than `maxMass` (1.8 kg). A Harmony prefix on that getter always returns `true` and skips the original, so the state machine never enters `overpressure`.

| Patch | Target | Kind |
|---|---|---|
| `Electrolyzer_RoomForPressure_Patch` | `Electrolyzer.RoomForPressure` (getter, private) | Prefix, skips original |

## Notes

- Gas keeps being emitted into cells that are already dense, so pressure around a sealed Electrolyzer can climb without limit.
- Older versions patched an `Electrolyzer.OverPressure` method. That method no longer exists in current game builds, so those versions failed to load with Harmony's "Undefined target method" error.
- The private getter is named by string, so a game update that renames it breaks the patch at load time with the same error. Check the game log after updating.
- Patch targets were checked against game build 744825.
