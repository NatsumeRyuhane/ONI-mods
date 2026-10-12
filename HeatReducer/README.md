# Heat Reducer

Powered buildings give off no more heat than the power they draw. A building that draws 120 W produces at most 120 W of heat, split between self heat and exhaust in the same proportion as vanilla.

In vanilla, building heat isn't tied to power at all. Self heat and exhaust are picked from fixed tiers, and almost every powered building gives off far more heat than it draws (the median self heat alone is about 8 times the power draw). For example, the Electrolyzer draws 120 W but gives off 1250 W.

## What changes

For every powered building except those listed below:

- If self heat plus exhaust is more than the power draw, both are scaled down by the same factor so the total equals the power draw.
- Otherwise nothing changes.

Not changed:

- Buildings without a power draw (toilets, generators, the Kiln and others).
- Buildings with negative heat values, which are cooling.
- Buildings whose heat is part of their job:
  - Temperature control: Space Heater, Liquid Tepidizer, Hydrofan, Ice-E Fan, Air Conditioner, Thermo Aquatuner, Anti Entropy Thermo-Nullifier, Liquid Heatquilizer, Ice Maker, Ice Liquefier, Refrigerator, Mini Fridge, Incubator.
  - Heat to power: Steam Turbine, and the deprecated Steam Turbine.
  - Heavy industry with 8 kW or more of total heat: Metal Refinery, Glass Forge, Kiln, Molecular Forge, Vulcanizer, Diamond Press, Polymer Press, Oil Refinery, Oxylite Refinery, Rock Crusher, Blastshot Maker, Desalinator.

The log lists every building changed: `[HeatReducer] Reducing {<name>} heat generation: <old> W -> <new> W`.

## Output temperature floor

Many buildings give their outputs a minimum temperature. The Electrolyzer's oxygen and hydrogen come out at 70 C or hotter even when its water is at 20 C, and a Petroleum Generator's carbon dioxide comes out at 110 C or hotter. That floor is extra heat on top of the self heat and exhaust above.

With **Remove Output Temperature Floor** on (the default), every building drops its floors, including the ones listed above as not changed:

- Converters (Electrolyzer, Oxygen Diffuser, Algae Terrarium, Ethanol Distiller, Compost and others) release outputs at the mass-weighted temperature of their inputs, as converters without a floor already do in vanilla.
- Generators (Coal, Wood, Peat, Natural Gas, Petroleum) release outputs at the building's temperature.
- Element emitters release outputs at the building's temperature.

An output whose element condenses or freezes above 0 C keeps its floor, since it would otherwise come out in the wrong state. In vanilla that is only the Polymer Press's 200 C steam. Outputs set to a fixed temperature rather than a minimum (the methane from the Fertilizer Synthesizer, the carbon dioxide from the Vulcanizer and others) are not changed.

The building's info panel picks up the change: "at 70 C min." turns into vanilla's input-temperature wording. The log lists each floor: `[HeatReducer] Removing {<name>} <element> output floor of <temperature> C`.

The option is in the mod's options menu and takes effect after a restart.

## How it works

Patching every config's `CreateBuildingDef` during mod load runs each config's static constructor too early, and some of them call `Db.Get()` before the database exists, which breaks building registration for the whole game. So the mod patches one method instead: a postfix on `BuildingConfigManager.RegisterBuilding`. It reads the finished `BuildingDef` from the manager's private `configTable` and caps its heat. `RegisterBuilding` has also run `DoPostConfigureComplete` by then, so the same postfix clears the floors on the finished prefab's `ElementConverter`, `EnergyGenerator` and `ElementEmitter` outputs. Configs whose DLC isn't active are never added to that table and are skipped.

| Patch | Target | Kind |
|---|---|---|
| `RegisterBuildingPostfix` (applied in `OnLoad`) | `BuildingConfigManager.RegisterBuilding` | Postfix |

## Notes

- The cap uses the power draw after every `CreateBuildingDef` postfix has run, including other mods'. With Low Power Filters, for example, the Liquid Filter's 4 kW is capped at its new 10 W.
- Heat is capped, never raised: a building already at or below its power draw is left as is.
- `configTable` is private and named by string. A game update that renames it breaks the mod at load.
- Patch targets were checked against game build 744825.
