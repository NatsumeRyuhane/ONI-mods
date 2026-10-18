# Solar Lux Scaling

Solar Panel output scales with its power cap, so a panel reaches a modded cap at the same light level as the vanilla 380 W cap.

A vanilla Solar Panel makes 0.00053 W per lux in each of its 14 light cells (the top two rows) and reaches its 380 W cap at 717,000 lux summed over them. Mods that raise the cap usually leave the watts per lux alone, so the higher cap needs much more light. This mod multiplies the output by cap / 380 before the cap is applied.

This mod changes nothing by itself. It needs another mod that changes the cap, such as Customize Buildings.

## What changes

| Total lux on the panel | Vanilla | 2500 W cap, without this mod | 2500 W cap, with this mod |
|---|---|---|---|
| 100,000 | 53 W | 53 W | 349 W |
| 358,500 | 190 W | 190 W | 1250 W |
| 717,000 | 380 W | 380 W | 2500 W |
| 4,717,000 | 380 W | 2500 W | 2500 W |

## Options

None.

## How it works

| Patch | Target | Kind |
|---|---|---|
| `SolarPanel_EnergySim200ms_Patch` | `SolarPanel.EnergySim200ms` | Transpiler, `Priority.Last`: the `Mathf.Clamp(watts, 0, cap)` call becomes `ScaledClamp`, which returns `Clamp(watts * cap / 380, 0, cap)` |

The patch doesn't look for any particular mod. `ScaledClamp` gets the cap as an argument at runtime, so it works whether another mod replaced the 380 literal with a constant or loads the cap from its own settings. With a 380 W cap it multiplies by 1.

Harmony rebuilds a method from every mod's transpilers in priority order each time a patch is added. `Priority.Last` makes this one run after the others whatever the load order, and they still find the vanilla `Clamp` call to edit.

## Notes

- Mods that also raise the watts per lux stack with this one. For example, Customize Buildings' `SolarEnergyMultiplier` of 2 makes the panel reach its cap at 358,500 lux. Set it to 1 to get exactly 717,000 lux.
- The vanilla 1 W floor (a panel with any light on it makes at least 1 W) is unchanged.
- A mod that skips `SolarPanel.EnergySim200ms` with a prefix and runs its own code instead bypasses this patch.
- Patch targets were checked against game build 744825.
