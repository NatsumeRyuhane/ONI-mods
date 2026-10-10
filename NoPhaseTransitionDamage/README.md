# No Phase Transition Damage

Pipes no longer take damage when their contents freeze or boil.

## What changes

| | Vanilla | With this mod |
|---|---|---|
| Contents freeze in a pipe | Pipe takes 1 damage and the segment is emptied | Nothing happens |
| Contents boil in a pipe | Pipe takes 1 damage and the segment is emptied | Nothing happens |

This applies to every gas, liquid and insulated or radiant pipe, since they all share the `Conduit` component.

## How it works

The sim reports pipe segments whose contents crossed a phase transition. `ConduitFlow.FreezeConduitContents` and `MeltConduitContents` then raise an event on the pipe, but only when the segment holds more than 10% of the pipe's maximum mass. `Conduit` handles that event in two private methods. Each one deals 1 damage to the pipe and empties the segment. Prefixes skip both methods.

| Patch | Target | Kind |
|---|---|---|
| `Conduit_OnConduitFrozen_Patch` | `Conduit.OnConduitFrozen` (private) | Prefix, skips original |
| `Conduit_OnConduitBoiling_Patch` | `Conduit.OnConduitBoiling` (private) | Prefix, skips original |

## Notes

- Skipping the handlers also skips emptying the segment, so the contents stay in the pipe. How the sim then treats their temperature hasn't been checked in-game.
- Older versions called `harmony.PatchAll()` after `base.OnLoad`, which applied each prefix twice. The result was the same, but it's fixed now.
- The private handlers are named by string, so a game update that renames them breaks the patch at load time. Check the game log after updating.
- Patch targets were checked against game build 744825.
