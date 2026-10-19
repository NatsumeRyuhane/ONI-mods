# LASER

Adds the LASER, a new light building that lights a straight line of cells at full brightness. It uses the Ceiling Light art, can be built anywhere and faces up, down, left or right.

The beam starts at the LASER's own cell and runs in the direction it was placed, with no length limit: it goes on until the first solid tile or the edge of the world. Unlike the vanilla lights, whose brightness fades with distance, every cell in the beam gets the same lux. Windows and other transparent tiles don't stop it.

## The building

| | Ceiling Light (vanilla) | LASER (defaults) |
|---|---|---|
| Size | 1x1 | 1x1 |
| Placement | On a ceiling, facing down | Anywhere, facing up, down, left or right |
| Material | 50 kg of any metal | 50 kg of any metal |
| Light | 1,800 lux, fading across a cone 8 cells deep | Slider per LASER, default 100,000 lux, the same in each cell of a 1-wide line up to the first solid tile or the world edge |
| Power | 10 W | Option, 5 W per 100,000 lux: 5 W at the default brightness, 50 W at the maximum |
| Heat | 500 W | Equal to the power draw |

The LASER appears in the Furniture build menu after the Ceiling Light and is unlocked by the same research, Interior Decor.

### Using it

- Select a built LASER to set its brightness with a slider, from 1,000 lux to the maximum option, in steps of 1,000 lux. The tooltip shows the power it draws at that brightness.
- Copy Settings copies the brightness.
- The LASER is lit only while it's powered and enabled. While placing it, the Light overlay shows the beam it will make at the default brightness.

## Options

Set in the game's mod menu (saved to `config.json` in the mod folder). Changes need a restart.

| Option | Default | Range | Effect |
|---|---|---|---|
| Default Brightness | 100,000 lux | 1,000 to 10,000,000 | Brightness of a newly built LASER. Capped at the maximum |
| Maximum Brightness | 1,000,000 lux | 1,000 to 10,000,000 | The top of each LASER's slider |
| Power per 100,000 Lux | 5 W | 0 to 1000 | Power drawn while lit, in proportion to the brightness, all of it turned into heat. 0 means the LASER needs no power |

There is no vanilla building to match, so the defaults are this mod's own.

## How it works

The LASER is a `Light2D` with the game's `Quad` shape, width 1 and falloff rate 0. The Mercury Ceiling Light uses the same shape, 3 cells wide and with falloff. The light's direction follows the placement rotation, and its range is set when the LASER spawns to the distance to its world's edge. The game's scan stops at solid tiles but not at the edge of the grid, where it would wrap onto the next row. Each LASER sets the `EnergyConsumer`'s wattage from its brightness and adds its own heat to the building every second, since the building def's heat is the same for every LASER.

| Patch | Target | Kind |
|---|---|---|
| `Db_Initialize_Patch` | `Db.Initialize` | Postfix: adds the strings, the build menu entry and the research unlock |
| `LightBuffer_LateUpdate_Patch` | `LightBuffer.LateUpdate` | Transpiler: draws a LASER's glow at a 4-cell range. The glow is sized by the light's range, so a world-long range would draw a cone over the whole screen |
| `Light2D_GetDescriptors_Patch` | `Light2D.GetDescriptors` | Postfix: the effects list says the range reaches the first solid tile, instead of a number that depends on where the LASER is built |
| `Light2D_ComputeExtents_Patch` | `Light2D.ComputeExtents` | Postfix: gives a `Quad` light that points left or right the row its beam covers. Vanilla gives it a column, since no vanilla light points sideways, so a tile built or dug in the beam wouldn't update it |

## Notes

- The building and slider text is English only.
- With enough brightness, a LASER can power Solar Panels for much less than they make. That's up to you.
- `Light2D.ComputeExtents` and `LightBuffer.LateUpdate` are private and named by string. A game update that renames them breaks the mod at load. If `LateUpdate` stops reading `Light2D.Range`, the mod logs a warning and LASER glows grow with their beams.
- Saves store the `Laser.Laser` component and its `lux` field by name.
- Patch targets were checked against game build 744825.
