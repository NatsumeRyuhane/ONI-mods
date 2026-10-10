# Irradiation Sterilization

Radiation kills a share of the germs every cycle instead of a fixed number, so it can sterilize large germ populations. It works on germs in tiles, pipes, items and buildings.

Radiation only exists with Spaced Out, so this mod does nothing in the base game.

## What changes

In vanilla, radiation kills a fixed number of germs per second: the rads times the germ's kill rate. The number of germs present doesn't matter, so a large population barely notices. Germs in pipes aren't affected at all.

With this mod, each Half-Kill Radiation of rads (default 100) halves the germs once per cycle:

| Radiation | Germs killed per cycle | Germ half-life |
|---|---|---|
| 50 rads | 29% | 2 cycles |
| 100 rads | 50% | 1 cycle |
| 200 rads | 75% | 1/2 cycle |
| 300 rads | 87.5% | 1/3 cycle |
| 1000 rads | 99.9% | 1/10 cycle |
| 10000 rads or more | 100%, at once | none |

The vanilla kill stays as a minimum. When a population is small enough that vanilla would kill it faster, vanilla's rate applies:

| Germ | Vanilla minimum (germs per rad per second) |
|---|---|
| Food Poisoning | 2.5 |
| Slimelung | 2.5 |
| Zombie Spores | 1 |
| Floral Scent | 0: immune, unless Affect Floral Scent is on |

The germ details panel shows the new half-life in its Radiation line, and its total rate of change includes the new kill.

## Options

Set in the game's mod menu (saved to `config.json` in the mod folder). Changes need a restart.

| Option | Default | Range | Effect |
|---|---|---|---|
| Half-Kill Radiation | 100 | 1 to 100000 | Radiation that kills half the germs per cycle. Twice as much kills three quarters |
| Instant-Kill Radiation | 10000 | 1 to 100000 | At or above this radiation, all germs die at once |
| Affect Floral Scent | Off | | Makes Floral Scent die to radiation too. Vanilla makes it immune |

## How it works

The kill model is in `Sterilization.cs`: after `dt` seconds, `2^(-rads * dt / (HalfKillRads * 600))` of the germs survive.

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
