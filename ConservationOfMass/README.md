# Conservation of Mass

Every tile that is dug out or destroyed drops all of its mass instead of half, so no mass is lost when tiles break.

## What changes

| | Vanilla | With this mod |
|---|---|---|
| Mass dropped by a dug or destroyed tile | 50% of the tile's mass | 100% of the tile's mass |

For example, digging a 1000 kg tile of Granite drops 1000 kg of Granite instead of 500 kg. The popup over the dropped resource shows the full amount too. Temperature and germs are unchanged.

## How it works

Every tile that is dug out or destroyed drops its resource through `WorldDamage.OnDigComplete`, which spawns `mass * 0.5f`. A prefix doubles `mass` first, so the drop equals the mass the tile had. Doubling and halving a float are exact, so no rounding is added.

| Patch | Target | Kind |
|---|---|---|
| `WorldDamage_OnDigComplete_Patch` | `WorldDamage.OnDigComplete` | Prefix: doubles `mass` |

## Notes

- Buildings are not affected: deconstructing one already returns all of its construction materials.
- Patch targets were checked against game build 744825.
