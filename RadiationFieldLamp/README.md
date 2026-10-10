# Radiation Field Lamp

Adds the Radiation Field Lamp, a new building that keeps an even, adjustable amount of radiation in every cell of a 4x3 area. Requires Spaced Out.

The field covers the lamp's own cell, the cells beside it and the 3x3 area in front of it, in the direction the lamp was placed. Unlike the vanilla Radiation Lamp, whose radiation fades with distance across a wide cone, every cell in the field gets the same amount.

## The building

| | Radiation Lamp (vanilla) | Radiation Field Lamp (defaults) |
|---|---|---|
| Size | 1x1 | 1x1 |
| Placement | On a wall, facing left or right | Anywhere, facing up, down, left or right |
| Material | 50 kg of any metal | 50 kg of any metal |
| Power | 60 W | Option, default 10 W |
| Heat | 500 W | Equal to the power draw |
| Radiation | 240 rads, fading across a 90-degree cone up to 16 cells long | Slider per lamp, default 500 rads, the same in each of its 12 cells |
| Uranium Ore use | 10 kg/cycle | 9.75 mg/s for each rad: about 2.9 kg/cycle at 500 rads |
| Depleted Uranium | 50% of the ore used | 90% of the ore used |
| Uranium Ore storage | 50 kg, refilled at 5 kg | 50 kg, refilled at 5 kg |

The lamp appears in the Radiation build menu after the Radiation Lamp and is unlocked by the same research, Agriculture. Depleted Uranium drops in 5 kg lumps.

The default ore per rad matches the vanilla Radiation Lamp's ore per rad delivered. The vanilla lamp's steady field sums to about 20,520 rads over its 86 cells, or 0.812 mg/s of ore per rad in one cell. The Field Lamp keeps its strength in each of 12 cells, so the same rate is 12 x 0.812 = 9.75 mg/s per rad of strength.

### Using it

- Select a built lamp to set its radiation per cell with a slider, from 0 to the maximum option. The tooltip shows the Uranium Ore it burns at that strength.
- Copy Settings copies the strength, and the direction when Adjustable Direction is on.
- The lamp emits only while it's powered, enabled, has fuel and is set above 0 rads.

## Options

Set in the game's mod menu (saved to `config.json` in the mod folder). Changes need a restart.

| Option | Default | Range | Effect |
|---|---|---|---|
| Default Radiation per Cell | 500 | 0 to 10000 | Strength of a newly built lamp, in rads. Capped at the maximum |
| Maximum Radiation per Cell | 1000 | 1 to 10000 | The top of each lamp's slider |
| Power Use | 10 W | 0 to 1000 | Power drawn while emitting, all of it turned into heat. 0 means the lamp needs no power |
| Uranium Ore per Rad | 9.75 mg/s | 0 to 100 | Ore burned per second for each rad of strength. 0 means the lamp needs no fuel and has no storage |
| Adjustable Direction | Off | | Lets a built lamp be re-pointed, diagonals included, from the Radbolt Reflector's direction panel |

Pointed diagonally, the lamp uses its up or down field moved one cell to the side, with the lamp in a corner. With Adjustable Direction off, every lamp uses the direction it was placed in. A direction set earlier is kept in the save and comes back if the option is turned on again.

## How it works

Each lamp has 12 native `RadiationEmitter`s, one per cell. Each has radius 1, which reaches only its own cell at full strength, and `emitRate` 0, which makes the sim run one pass per tick so the cell settles at the emitter's rads. Pointing the lamp moves the emitters' offsets.

| Patch | Target | Kind |
|---|---|---|
| `Db_Initialize_Patch` | `Db.Initialize` | Postfix: adds the strings, the build menu entry and the research unlock |
| `HighEnergyParticleDirectionSideScreen_IsValidForTarget_Patch` | `HighEnergyParticleDirectionSideScreen.IsValidForTarget` | Postfix: with Adjustable Direction on, shows the Radbolt Reflector's direction panel for the lamp |

## Notes

- The building and slider text is English only.
- `KBatchedAnimTracker.previousMatrix` is private and named by string, so the indicator follows a custom direction. A game update that renames it breaks the mod at load.
- Saves store the `RadiationFieldLamp.RadiationFieldLamp` component and its `radsPerCell`, `hasCustomDirection` and `customDirection` fields by name.
- Patch targets were checked against game build 744825.
