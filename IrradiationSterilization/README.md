# Irradiation Sterilization

Radiation kills a share of the germs every cycle instead of a fixed number, so it can sterilize large germ populations. It works on germs in tiles, pipes, items and buildings.

Radiation only exists with Spaced Out, so this mod does nothing in the base game.

## What changes

In vanilla, radiation kills a fixed number of germs per second: the rads times the germ's kill rate. The number of germs present doesn't matter, so a large population barely notices. Germs in pipes aren't affected at all.

With this mod, germs die by target theory, `N = N0 * e^(-k * D)`, where `D` is the dose they absorb. A cell's radiation is a dose rate in rads per cycle, the same way duplicants absorb it. Each germ has a D50, the radiation that kills half of it per cycle, so `k = ln 2 / D50`. Twice the D50 kills three quarters per cycle, three times kills seven eighths, and so on.

Germs killed per cycle with the default D50s:

| Germ | D50 | 60 rads (Shine Bug) | 100 rads | 240 rads (Radiation Lamp) | 480 rads (Wheezewort) |
|---|---|---|---|---|---|
| Food Poisoning | 50 | 57% | 75% | 96% | 99.9% |
| Slimelung | 50 | 57% | 75% | 96% | 99.9% |
| Zombie Spores | 125 | 28% | 43% | 74% | 93% |
| Floral Scent | none | immune | immune | immune | immune |

The radiation values are the sources' emitter strengths; cells further away get less. The default D50s keep vanilla's ratio between germs: Zombie Spores' vanilla kill rate is 1 against 2.5 for the others, so its D50 is 2.5 times theirs.

The vanilla kill stays as a minimum, so small clusters of germs still die as fast as in vanilla instead of trailing off. The exponential kill only beats it on large populations: with the default D50s, above about 108,000 germs in a cell, at any radiation level and for every germ. Time to clear one cell of Food Poisoning at 100 rads:

| Germs | Vanilla | With this mod |
|---|---|---|
| 100,000 or fewer | Same as with this mod | Up to 0.67 cycles |
| 1,000,000 | 6.7 cycles | About 2.3 cycles |
| 10,000,000 | 67 cycles | About 4 cycles |

Germs in pipes follow the same rule, though vanilla kills nothing there. The germ details panel shows the new half-life in its Radiation line, and its total rate of change includes the new kill.

## Options

Set in the game's mod menu (saved to `config.json` in the mod folder). Changes need a restart.

| Option | Default | Range | Effect |
|---|---|---|---|
| Food Poisoning D50 | 50 | 0 to 100000 | Radiation that kills half the Food Poisoning germs per cycle. 0 keeps vanilla behavior |
| Slimelung D50 | 50 | 0 to 100000 | The same for Slimelung |
| Zombie Spores D50 | 125 | 0 to 100000 | The same for Zombie Spores |
| Floral Scent D50 | 0 | 0 to 100000 | The same for Floral Scent. 0 keeps it immune, as in vanilla |
| Instant-Kill Radiation | 0 | 0 to 100000 | At or above this radiation, all germs with a D50 die at once. 0 turns it off |

The instant kill is a gameplay shortcut, not needed for accuracy: the surviving fraction is computed exactly, so it stays between 0 and 1 at any radiation. Germs added by other mods keep vanilla behavior.

## How it works

The kill model is in `Sterilization.cs`: after `dt` seconds, `2^(-rads * dt / (D50 * 600))` of the germs survive, which is `e^(-k * D)` with `D = rads * dt / 600`.

| Patch or component | Target | What it does |
|---|---|---|
| `DiseaseContainers_CalculateDelta_Patch` | `DiseaseContainers.CalculateDelta` | Postfix: germs on items and buildings. Replaces vanilla's kill with the larger of vanilla's and the new one |
| `ConduitDiseaseManager_Sim200ms_Patch` | `ConduitDiseaseManager.Sim200ms` | Postfix: germs in gas and liquid pipes, using the radiation of each pipe's cell. Vanilla kills nothing there |
| `Game_OnPrefabInit_Patch` and `CellSterilizer` | `Game.OnPrefabInit` (protected) | Adds a component that, once a second, removes germs from irradiated cells with `SimMessages.ModifyDiseaseOnCell` |
| `AdditionalDetailsPanel_BuildFactorsStrings_Patch` | `AdditionalDetailsPanel.BuildFactorsStrings` (private) | Postfix: rewrites the details panel's Radiation line with the new half-life |

Germs in cells live in the native sim, which applies vanilla's fixed kill itself and can't be patched. `CellSterilizer` removes only the germs the new model kills beyond that, so the two together kill the larger amount. Items on conveyor rails count as items.

Germ counts are whole numbers. The pipe and cell kills are rounded up at random, in proportion to the fraction, so small populations still die off instead of being rounded away.

## Known issues

- `CellSterilizer` assumes the native sim kills `radiationKillRate * rads` germs per second, which is what vanilla's details panel shows. The sim's code can't be read, so in cells the total kill could be slightly above the larger of the two rates.
- In vanilla, germs on items and buildings lose the per-second amount every 200 ms tick, so the vanilla minimum on them is 5 times what the details panel says. The mod keeps that.
- `Game.OnPrefabInit`, `AdditionalDetailsPanel.BuildFactorsStrings`, `AdditionalDetailsPanel.GetFormattedHalfLife` and `ConduitFlow.SOAInfo.diseaseHandles` are not public, so they're named by string. A game update that renames them breaks the mod at load time.
- Patch targets were checked against game build 744825.
