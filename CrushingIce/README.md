# Crushing Ice

Adds Ice, Polluted Ice and Brine Ice recipes to the Rock Crusher. Each one crushes the ice into Snow.

## What changes

| Building | Input | Output | Time |
|---|---|---|---|
| Rock Crusher | 100 kg Ice | 100 kg Snow | 40 s |
| Rock Crusher | 100 kg Polluted Ice | 99 kg Snow and 1 kg Dirt | 40 s |
| Rock Crusher | 100 kg Brine Ice | 70 kg Snow and 30 kg Salt | 40 s |

The outputs come out at the temperature of the ice, like the vanilla Rock Crusher recipes.

The byproduct shares are the game's own: boiling Polluted Water leaves 1% Dirt, and Brine is a 30% salt solution, the same split the Desalinator uses.

Snow melts at the same temperature as Ice (272.5 K) and has the same heat capacity, but a quarter of its thermal conductivity (0.545 vs 2.18), so it makes a cheap insulating coolant or building material. Polluted Ice and Brine Ice melt colder than that (252.5 K and 256.65 K), so the Snow they make keeps longer than the ice did.

## How it works

| Patch | Target | Kind |
|---|---|---|
| `RockCrusherConfig_ConfigureBuildingTemplate_Patch` | `RockCrusherConfig.ConfigureBuildingTemplate` | Postfix: adds the three recipes |

The recipe descriptions use the game's own `Crushes {0} into {1}` and `Crushes {0} into {1} and {2}` strings and element names, so they follow the game's language.

## Notes

- Germs on Polluted Ice are not carried over to the outputs.
