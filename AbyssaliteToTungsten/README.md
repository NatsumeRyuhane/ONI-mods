# Abyssalite to Tungsten

Adds an Abyssalite to Tungsten recipe to the Metal Refinery and the Molecular Forge.

## What changes

| Building | Input | Output | Time | Output temperature |
|---|---|---|---|---|
| Metal Refinery | 100 kg Abyssalite | 100 kg Tungsten | 40 s | The refinery's output temperature; the heat goes into its coolant |
| Molecular Forge | 100 kg Abyssalite | 100 kg Tungsten | 80 s | Heated, like the Molecular Forge's own recipes |

The Metal Refinery is faster but needs coolant. As with its other recipes, the coolant absorbs the heat of cooling the molten metal, worked out from Tungsten's very high melting point (3695 K). The Molecular Forge is slower and has no coolant step.

With **Balance heat production** on (the default), the Metal Refinery recipe sends three times the vanilla heat into the coolant. Abyssalite is common, so this keeps it from making Wolframite obsolete. The Metal Refinery's description also gets a line saying that refining Abyssalite releases a great deal of heat.

## Options

Set in the game's mod menu (saved to `config.json` in the mod folder). Changes need a restart.

| Option | Default | Effect |
|---|---|---|
| Balance heat production | On | Triples the coolant heat of the Metal Refinery recipe and adds the heat line to the Metal Refinery description. The Molecular Forge recipe is not affected |

## How it works

The patches are applied by hand in `OnLoad`, so only these two configs are patched and the other building configs' static constructors don't run at load.

| Patch | Target | Kind |
|---|---|---|
| `AddMetalRefineryRecipe` | `MetalRefineryConfig.ConfigureBuildingTemplate` | Postfix: adds the 40 s recipe |
| `AddMolecularForgeRecipe` | `SupermaterialRefineryConfig.ConfigureBuildingTemplate` | Postfix: adds the 80 s recipe |
| `TripleCoolantHeat` | `LiquidCooledRefinery.SpawnOrderProduct` (protected) | Prefix: for this recipe only, adds twice the vanilla coolant heat before the vanilla code adds its own |
| `AppendHeatLine` | `BuildingDef.Desc` getter | Postfix: adds the heat line to the Metal Refinery description |

The prefix computes the vanilla heat the same way the game does (`thermalFudge` x (Tungsten's melting point - output temperature) x mass x heat capacity) and splits the extra heat across the coolant by heat capacity, so every coolant element warms by the same amount.

## Notes

- The recipe description ("Refines Abyssalite into Tungsten.") and the heat line are English only.
- `LiquidCooledRefinery.SpawnOrderProduct` is protected and named by string, so a game update that renames it breaks the mod at load.
- Patch targets were checked against game build 744825.
