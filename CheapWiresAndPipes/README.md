# Cheap Wires And Pipes

Wires, pipes, conveyor rails, automation wires and their bridges cost only 1 kg of construction material and have no decor penalty. Laying out utilities stops being a resource sink.

## What changes

| Building | Vanilla material | Vanilla decor | With this mod |
|---|---|---|---|
| Wire | 25 kg | -5 | 1 kg, 0 decor |
| Heavi-Watt Wire | 100 kg | -25 | 1 kg, 0 decor |
| Wire Bridge | 25 kg | -5 | 1 kg, 0 decor |
| Heavi-Watt Joint Plate | 200 kg | -25 | 1 kg, 0 decor |
| Conductive Wire | 25 kg | 0 | 1 kg, 0 decor |
| Heavi-Watt Conductive Wire | 100 kg | -20 | 1 kg, 0 decor |
| Conductive Wire Bridge | 25 kg | -5 | 1 kg, 0 decor |
| Heavi-Watt Conductive Joint Plate | 100 kg | -25 | 1 kg, 0 decor |
| Liquid Pipe | 100 kg | 0 | 1 kg, 0 decor |
| Insulated Liquid Pipe | 400 kg | -5 | 1 kg, 0 decor |
| Radiant Liquid Pipe | 50 kg | -5 | 1 kg, 0 decor |
| Liquid Bridge | 100 kg | 0 | 1 kg, 0 decor |
| Gas Pipe | 25 kg | 0 | 1 kg, 0 decor |
| Insulated Gas Pipe | 400 kg | -5 | 1 kg, 0 decor |
| Radiant Gas Pipe | 25 kg | -5 | 1 kg, 0 decor |
| Gas Bridge | 50 kg | 0 | 1 kg, 0 decor |
| Conveyor Rail | 100 kg | 0 | 1 kg, 0 decor |
| Automation Wire | 5 kg | -5 | 1 kg, 0 decor |

The two conductive bridges take their decor from the base bridge configs they extend. A config that calls one of the patched methods through `base.CreateBuildingDef()` is changed too: in current builds the Insulated Conductive Wire Bridge (`WireRubberBridgeConfig`, which extends `WireBridgeConfig`) also costs 1 kg. The Insulated Conductive Wire itself (`WireRubberConfig`) is not changed.

The material type (metal ore, refined metal, and so on) is unchanged; only the amount is.

## How it works

One postfix covers all 18 configs. It lists them in `TargetMethods`, sets `BuildingDef.Mass` to a single 1 kg entry and `BaseDecor` to 0, and logs each building it changes (`[CheapWiresAndPipes] Reducing mass of {...}`).

| Patch | Targets | Kind |
|---|---|---|
| `CreateBuildingDef_Patch` | The parameterless `CreateBuildingDef` of each config in the table | Postfix |

## Notes

- Deconstruction refunds the building's current `BuildingDef.Mass` (`Deconstructable`), not the amount it was built with. Utilities built before installing the mod therefore refund only 1 kg each, and the rest of their vanilla material is lost. Removing the mod has the opposite effect: utilities built with 1 kg refund the full vanilla cost.
- `mod_info.yaml` sets `minimumSupportedBuild: 468097`.
- Older versions logged "Ignored ...: no `CreateBuildingDef` method" for every config even though the patch was applied. That line is gone.
- Patch targets were checked against game build 744825.
