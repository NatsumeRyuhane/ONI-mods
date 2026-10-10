# Irradiation Sterilization

Radiation kills germs faster. One option multiplies how many germs each rad kills per second, for every germ type that radiation can kill.

Radiation only exists with Spaced Out, so this mod does nothing in the base game.

## What changes

Radiation kills a fixed number of germs per second, set by the rads in the cell and the germ type's kill rate. The number of germs present doesn't change it. With the default 10x multiplier:

| Germ | Vanilla (germs per rad per second) | With this mod (default) | At 100 rads, vanilla | At 100 rads, with this mod |
|---|---|---|---|---|
| Food Poisoning | 2.5 | 25 | 250/s | 2,500/s |
| Slimelung | 2.5 | 25 | 250/s | 2,500/s |
| Zombie Spores | 1 | 10 | 100/s | 1,000/s |
| Floral Scent | 0 (immune) | 0 (immune) | none | none |

This applies to germs in tiles and pipes, and to germs on items and buildings. The germ details panel shows the new rates in its Radiation line.

## Options

Set in the game's mod menu (saved to `config.json` in the mod folder). Changes need a restart.

| Option | Default | Range | Effect |
|---|---|---|---|
| Radiation Sensitivity Multiplier | 10 | 0 to 1000 | Multiplies every germ type's radiation kill rate. 1 is vanilla, and 0 makes all germs immune to radiation |

## How it works

| Patch | Target | Kind |
|---|---|---|
| `Diseases_Constructor_Patch` | `Database.Diseases` constructor | Postfix: multiplies `Disease.radiationKillRate` for every germ type |

The game reads `radiationKillRate` in three places. `SimMessages.CreateDiseaseTable` sends it to the native sim, which handles germs in tiles and pipes. `DiseaseContainers.CalculateDelta` uses it for germs on items and buildings. The germ details panel uses it for its Radiation line. Changing the rate where the germ types are built covers all three.

## Known issues

- A multiplier can't make Floral Scent sensitive, because its vanilla rate is 0.
- In vanilla, germs on items and buildings lose the per-second amount every 200 ms tick, so they die 5 times faster than the details panel says. The mod keeps that behavior.
- Patch targets were checked against game build 744825.
