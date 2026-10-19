# Robo-Miner No Foundation

The Robo-Miner can be built anywhere: in mid-air, in a liquid or gas pocket, or next to tiles you plan to dig out later. It still rotates freely, and it keeps running after the tiles around it are gone.

## What changes

| | Vanilla | With this mod |
|---|---|---|
| Placement | Needs solid tiles along its base, whichever way it is rotated | Any 2x2 area that is otherwise buildable |
| After its floor is dug out | Stops, with a Missing Foundation status | Keeps running |
| Rotation | All four directions | All four directions (unchanged) |

Everything else is unchanged: the dig area, power draw, heat, the automation port and the material cost.

## How it works

`AutoMinerConfig.CreateBuildingDef` builds the Robo-Miner with `BuildLocationRule.OnFoundationRotatable`. `BuildingDef.IsValidBuildLocation` checks that rule against the tiles along the rotated base. `BuildingTemplates.CreateBuildingDef` also sets `ContinuouslyCheckFoundation` for it, so a built miner watches those tiles and turns off its operational flag when they are gone.

A postfix sets the rule to `BuildLocationRule.Anywhere` and clears `ContinuouslyCheckFoundation`. `PermittedRotations` is a separate setting, so rotation still works.

| Patch | Target | Kind |
|---|---|---|
| `AutoMinerConfig_CreateBuildingDef_Patch` | `AutoMinerConfig.CreateBuildingDef` | Postfix |

## Notes

- The usual placement checks still apply: the area can't overlap other buildings or neutronium, and it can't sit outside the map.
- Robo-Miners already built in a save also stop checking their foundation once the mod is on, because the check is set up when the building loads.
- Patch targets were checked against game build 744825.
