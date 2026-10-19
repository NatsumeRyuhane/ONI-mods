# Sublimation Sconce

Adds the Sublimation Sconce, a new building like the Oxylite Sconce that holds any solid that sublimates and makes it sublimate as fast as Oxylite. Requires The Frosty Planet Pack, which has the sconce art.

Slime, Bleach Stone and Polluted Dirt give off gas on their own, but slowly, and only Polluted Dirt has a building for it, the Sublimation Station in Spaced Out. The Sublimation Sconce gives each of them, and any sublimating solid added by another mod, a place to work at Oxylite's pace.

## The building

| | Oxylite Sconce (vanilla) | Sublimation Sconce (defaults) |
|---|---|---|
| Size | 1x1 | 1x1 |
| Placement | Anywhere | Anywhere |
| Material | 25 kg of any metal | 50 kg of any metal |
| Accepts | Oxylite | Any solid that sublimates, picked per sconce |
| Storage | 240 kg, refilled below 96 kg | Option, default 240 kg, with a slider per sconce. Refilled below 40% of the slider |
| Sublimation | Oxylite's own rate | Oxylite's rate for everything, unless the material's own is faster |

The sconce appears in the Oxygen build menu after the Oxylite Sconce and is unlocked by the same research, Portable Gases.

What sublimates in the vanilla game, and how fast with 240 kg stored:

| Material | Gives off | Vanilla rate | In the sconce |
|---|---|---|---|
| Oxylite | Oxygen | about 464 g/s | unchanged |
| Slime | Polluted Oxygen | 25 g/s | about 464 g/s |
| Bleach Stone | Chlorine | about 3.1 g/s | about 464 g/s |
| Polluted Dirt | Polluted Oxygen | about 0.3 g/s | about 464 g/s |

Oxylite's rate grows with the stored mass (0.01 kg/s times mass to the 0.7), so it's about 244 g/s at the 96 kg refill point. In practice the gas around the sconce limits it more: like vanilla, a material stops sublimating while its cell holds 1.8 kg of gas or more, so the real output is however fast the gas moves away. Mass is kept: every gram of gas is a gram of the material.

Polluted Water and Nuclear Waste also give off gas, but they are liquids and the sconce doesn't take them.

### Using it

- Select a built sconce to tick the materials it accepts. Only discovered materials are listed. A new sconce accepts nothing until you pick something.
- Unticking a material drops what the sconce holds of it.
- The capacity slider sets how much it keeps, up to the storage option.
- Copy Settings copies the materials and the capacity.

## Options

Set in the game's mod menu (saved to `config.json` in the mod folder). Changes need a restart.

| Option | Default | Range | Effect |
|---|---|---|---|
| Storage Capacity | 240 kg | 10 to 2000 | Most a sconce holds, and the top of its slider |
| Oxylite Sublimation Rate | On | | When off, materials keep their vanilla rate, so the sconce only stores them |

## How it works

The vanilla Oxylite Sconce has no logic of its own. It is a storage that isn't sealed, so the stored Oxylite's own `Sublimates` component keeps running. The Sublimation Sconce works the same way. It takes the Oxidizer Tank's `FlatTagFilterable` material list, filled with every solid element whose item has a `Sublimates` component, and fetches the ticked materials itself.

| Patch | Target | Kind |
|---|---|---|
| `Db_Initialize_Patch` | `Db.Initialize` | Postfix: adds the strings, the build menu entry and the research unlock |
| `Sublimates_Sim200ms_Patch` | `Sublimates.Sim200ms` | Prefix and finalizer: for an item in a Sublimation Sconce, swaps in Oxylite's rate, minimum emission and mass scaling for the tick, then puts the item's own back. Not applied when Oxylite Sublimation Rate is off |

## Notes

- The building text is English only.
- Saves store the `SublimationSconce.SublimationSconce` component and its `userMaxCapacity` field by name.
- Patch targets were checked against game build 744825.
