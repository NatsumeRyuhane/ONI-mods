# Building lifecycle

How the game turns an `IBuildingConfig` into a building, from startup registration to construction,
save loading and deconstruction, and which methods to patch to change each part.

Read from game build 744825. Private members are named as they appear in the decompiled game, so a
game update can rename them.

## Contents

- [Overview](#overview)
- [Startup: registration](#startup-registration)
- [The three prefabs](#the-three-prefabs)
- [In game: placing and building](#in-game-placing-and-building)
- [Loading a save](#loading-a-save)
- [Deconstruction](#deconstruction)
- [Tile replacement](#tile-replacement)
- [Component callbacks](#component-callbacks)
- [Events](#events)
- [Which hook to patch](#which-hook-to-patch)
- [Pitfalls](#pitfalls)

## Overview

```
Startup (once per game launch)
  Assets.OnPrefabInit
    CreatePrefabs
      Db.Get()                          -> Db.Initialize
      LegacyModMain.Load
        LoadBuildings                   -> every IBuildingConfig is registered
        LoadEntities                    -> IBuildingConfig.ConfigurePost
      Db.PostProcess                    -> Techs.PostProcess

In game (per building)
  BuildTool -> BuildingDef.TryPlace -> "<ID>UnderConstruction" instance
  Constructable: fetch materials, dig, build errand
  Constructable.OnCompleteWork -> FinishConstruction -> BuildingDef.Build -> "<ID>" instance
  Deconstructable.OnCompleteWork -> materials dropped, building deleted
```

## Startup: registration

`Assets.OnPrefabInit` calls `Assets.CreatePrefabs`, which runs in this order:

1. `Db.Get()`, which runs `Db.Initialize` the first time. Techs, status items, building facades and
   the rest of the database exist from here on.
2. `LegacyModMain.Load`, which collects the types of every loaded assembly (mods included), then:
   1. `LoadOre`
   2. `LoadBuildings`: fills the Strings table from the `STRINGS.BUILDINGS.PREFABS` fields
      (`LocString.CreateLocStringKeys`), then calls `GeneratedBuildings.LoadGeneratedBuildings`.
   3. `ConfigElements`
   4. `LoadEntities`: registers every `IEntityConfig`, then calls
      `BuildingConfigManager.ConfigurePost`, which runs `IBuildingConfig.ConfigurePost(def)` for each
      registered building.
   5. `LoadEquipment`
3. `Db.PostProcess` (runs `Techs.PostProcess`), then `ComplexRecipeManager.PostProcess`.

`GeneratedBuildings.LoadGeneratedBuildings` creates one instance of every non-abstract
`IBuildingConfig` type and passes it to `BuildingConfigManager.RegisterBuilding`. An exception
thrown during one registration is logged as `Exception in RegisterBuilding for type <type> from
<assembly>` and that building is skipped, so it is missing from the game. After registration it
drops build menu entries whose building doesn't exist, and moves buildings that were added to
`BUILDINGS.PLANORDER` by hand to an "uncategorized" subcategory, with a warning to use
`ModUtil.AddBuildingToPlanScreen` instead.

### `BuildingConfigManager.RegisterBuilding`

For each config, in order:

| Step | What happens |
|---|---|
| DLC check | If `DlcManager.IsCorrectDlcSubscribed(config)` is false, the config is skipped. None of its methods run, and it's not added to `configTable`. |
| `config.CreateBuildingDef()` | Returns the `BuildingDef`. Most configs build it with `BuildingTemplates.CreateBuildingDef`, which also calls `GenerateOffsets()` for the footprint. The DLC ids are then copied onto the def. |
| Complete template | The base template (see below) is cloned, its `KPrefabID.PrefabTag` set to the def's tag and `Building.Def` set to the def. |
| `config.ConfigureBuildingTemplate(go, tag)` | Runs on that clone, which becomes the completed building. Components added here exist only on the completed building, not on the construction site or preview. |
| `BuildingLoader.CreateBuildingComplete` | Adds and removes components to match the def (see [Def-driven components](#def-driven-components)), registers the prefab with `Assets.AddPrefab`, and sets up logic and radbolt ports. |
| `BuildingLoader.CreateBuildingUnderConstruction` | Builds the construction site prefab from a separate template. Skipped for `Headquarters` (the Printing Pod). |
| `BuildingLoader.CreateBuildingPreview` | Builds the placement preview prefab. Also skipped for `Headquarters`. |
| `def.PostProcess()` | Builds `def.CraftRecipe` from `MaterialCategory` and `Mass`, and adds the building to `Db.Get().TechItems` (unless deprecated). |
| `config.DoPostConfigureComplete(go)` | Runs on the completed building prefab. Most configs add their gameplay components here. |
| Submergible tag | Added when the def is not `Floodable`, not a tile and not a foundation. |
| `config.DoPostConfigurePreview(def, go)` | Runs on the preview prefab. |
| `config.DoPostConfigureUnderConstruction(go)` | Runs on the construction site prefab. |
| `Assets.AddBuildingDef(def)` | Adds the def to `Assets.BuildingDefs`, replacing any def with the same `PrefabID`. |

`IBuildingConfig.ConfigurePost(def)` runs later, after every building and entity is registered, so
it can look up other prefabs.

The base template, made in `BuildingConfigManager.OnPrefabInit`, has `KPrefabID`, `KSelectable`,
`Modifiers`, `PrimaryElement`, `InfraredPrimaryElement`, `BuildingComplete`,
`StateMachineController`, `Deconstructable`, `Reconstructable`, `SaveLoadRoot`, `OccupyArea`,
`DecorProvider`, `Operational`, `BuildingEnabledButton`, `Prioritizable`, `BuildingHP`,
`LoopingSounds` and `InvalidPortReporter`.

### Def-driven components

`BuildingLoader.CreateBuildingComplete` runs after `ConfigureBuildingTemplate` and before
`DoPostConfigureComplete`. It reads these def fields and adds the matching component, or removes it
when the field says it isn't needed:

| Def field | Component |
|---|---|
| `Mass`, `MaterialCategory` | `PrimaryElement.MassPerUnit` (sum of `Mass`) |
| `HitPoints`, `Invincible` | `BuildingHP` |
| `Repairable` | `Repairable` |
| `InputConduitType`, `OutputConduitType` | `BuildingConduitEndpoints`, `ConduitConsumer` (gas or liquid input), `RequireOutputs` |
| `RequiresPowerInput` | `EnergyConsumer`, `RequireInputs` |
| `RequiresPowerInput` and `AddLogicPowerPort` | single logic input port, `LogicOperationalController` |
| `isUtility` | `Operational` is removed for utilities |
| `PermittedRotations` | `Rotatable` |
| `Breakable` | `Breakable` |
| `Floodable` | `Floodable` |
| `Disinfectable` | `AutoDisinfectable`, `Disinfectable` |
| `Overheatable`, `OverheatTemperature`, `FatalHot` | `Overheatable` |
| `Entombable` | `Structure` |
| `BaseDecor`, `BaseDecorRadius` | `DecorProvider` |
| `AttachmentSlotTag` | `AttachableBuilding` |
| `AnimFiles` | `KBatchedAnimController`, `KBoxCollider2D` |
| `AlwaysOperational` | removes `BuildingEnabledButton`, `Operational` and `LogicPorts` |
| `LogicInputPorts`, `LogicOutputPorts` | `LogicPorts` |
| `UseHighEnergyParticleInputPort`, `UseHighEnergyParticleOutputPort` | `HighEnergyParticlePort` |

Because of the removals, a component added in `ConfigureBuildingTemplate` can be destroyed again.
For example, an `EnergyConsumer` added there is removed if `RequiresPowerInput` is false.

## The three prefabs

Each registered building has three prefabs:

| Prefab tag | GameObject name | Made by | Patched by | Key components |
|---|---|---|---|---|
| `<ID>` | `<ID>Complete` | `CreateBuildingComplete` | `ConfigureBuildingTemplate`, `DoPostConfigureComplete` | `BuildingComplete`, `Deconstructable` |
| `<ID>UnderConstruction` | `<ID>UnderConstruction` | `CreateBuildingUnderConstruction` | `DoPostConfigureUnderConstruction` | `BuildingUnderConstruction`, `Constructable`, `Storage` |
| `<ID>Preview` | `<ID>Preview` | `CreateBuildingPreview` | `DoPostConfigurePreview` | `BuildingPreview` |

`BuildingComplete`, `BuildingUnderConstruction` and `BuildingPreview` all derive from `Building`,
which holds `Def`. The def is shared by every instance of the building.

The prefabs are inactive templates. `GameObject.PreInit()` initializes their components without
running `OnPrefabInit`, so component callbacks only ever run on instances.

## In game: placing and building

1. **Placing.** `BuildTool` calls `BuildingDef.TryPlace` (or `TryReplaceTile`, see
   [Tile replacement](#tile-replacement)). If `IsValidPlaceLocation` passes, `BuildingDef.Instantiate`
   creates a `<ID>UnderConstruction` instance with the selected materials in
   `Constructable.SelectedElementsTags`, then applies the orientation and facade.
2. **Construction site.** `Constructable.OnSpawn` marks the area, sets the primary element and
   temperature, submits a `FetchList2` for the materials and places dig errands for solid cells
   (`PlaceDiggables`). When all materials are delivered, `OnFetchListComplete` clears the material
   needs. The work time is `def.ConstructionTime`. Buildings above tech tier 2 set
   `requireMinionToWork`, so only a Duplicant can build them.
3. **Finishing.** `Constructable.OnCompleteWork` computes the building temperature as the mass
   weighted temperature of the delivered materials, capped at 318.15 K (45 °C). Then
   `FinishConstruction`:
   1. unmarks the construction site's area,
   2. calls `BuildingDef.Build`, which creates the `<ID>` instance (see below),
   3. triggers `NewBuilding` on `GameplayEventManager`,
   4. copies rotation and utility connections, and moves the selection to the new building,
   5. triggers `NewConstruction` on the new building, with the `Constructable` as data,
   6. consumes the delivered materials and deletes the construction site.

`BuildingDef.Build` calls `BuildingDef.Create`, which instantiates `def.BuildingComplete`, sets its
element and temperature, and activates it. `Build` then sets the orientation, marks the grid area,
plays the finish sound, copies the selected materials to `Deconstructable.constructionElements`,
sets `BuildingComplete.creationTime`, and triggers `NewBuilding` on `Game.Instance` and on the
building.

Instant build (debug mode or sandbox) calls `BuildingDef.Build` directly from `BuildTool`. No
construction site is made, so `NewConstruction` doesn't fire.

## Loading a save

`SaveLoadRoot.Load` instantiates the saved prefab (`<ID>` or `<ID>UnderConstruction`), activates it,
and only then deserializes its `[Serialize]` fields. So on a loaded building:

- `OnPrefabInit` runs before saved data is restored.
- `OnSpawn` runs after it.
- `BuildingDef.Build` is not called, so `NewBuilding` and `NewConstruction` don't fire.

A per-instance change that should also reach buildings from an existing save belongs in an `OnSpawn`
hook, not in `Build` or the events.

## Deconstruction

1. `Deconstructable.QueueDeconstruction` creates a deconstruct errand (or finishes at once in
   instant build mode). The work time is 30 s, or half of `ConstructionTime` for tiles, unless
   `customWorkTime` is set.
2. `Deconstructable.OnCompleteWork`:
   1. triggers `DeconstructComplete`,
   2. asks `Reconstructable` to place a new construction site if a material swap was requested,
   3. for buildings with a `SimCellOccupier` (tiles), waits for the sim cell to be removed,
   4. drops the materials (`SpawnItemsFromConstruction`), marks the component destroyed and deletes
      the building.
3. `BuildingComplete.OnCleanUp` unmarks the grid area, removes the building from the global lists
   and triggers `BuildingCompleteDestroyed`.

The dropped materials are `constructionElements` with the masses from the current `def.Mass`, at
the building's temperature and germs, split into chunks of at most 400 kg. Nothing drops if
`allowDeconstruction` is false.

## Tile replacement

Building one tile type over another uses `BuildingDef.TryReplaceTile`, which sets
`Constructable.IsReplacementTile` on the site. When construction finishes, `OnCompleteWork` finds
the old building with `GetReplacementCandidate`, drops its materials, triggers `ObjectReplaced` on
it and deletes it. For tiles, the new building is built once the old sim cell is gone. For other
replaceable buildings (such as pipes), it's built when the old one triggers
`BuildingCompleteDestroyed`.

## Component callbacks

Buildings are made of `KMonoBehaviour` components. Their callbacks:

| Callback | Called from | When |
|---|---|---|
| `OnPrefabInit` | Unity `Awake` | When the instance is activated. On load, before saved data is restored. |
| `OnSpawn` | Unity `Start` | Later, before the component's first update. Saved data is available. |
| `OnCleanUp` | Unity `OnDestroy` | When the object is deleted. Not called while the game is exiting. |

`KMonoBehaviour` catches exceptions thrown in `OnPrefabInit` and `OnSpawn` and logs them as
`Error in <object>.<component>.OnSpawn`, so a failing patch there doesn't crash the game but leaves
the component half set up. `KPrefabID` also offers `prefabInitFn` and `prefabSpawnFn` events,
which run once from its own `OnPrefabInit` and `OnSpawn`.

The building classes do this in their callbacks:

- `BuildingComplete.OnPrefabInit`: positions the object, adds `def.attributes` and
  `def.attributeModifiers`, registers structure temperature, and subscribes to `ObjectReplaced` and
  `EntombedChanged`.
- `BuildingComplete.OnSpawn`: marks the grid area and foundation cells, sets a missing mass to
  `def.Mass[0]`, registers with `Components.BuildingCompletes`, `BuildingInventory` and the scene
  partitioner, and adds attribute modifiers from secondary construction materials.
- `BuildingComplete.OnCleanUp`: undoes the above and triggers `BuildingCompleteDestroyed`.
- `BuildingUnderConstruction.OnSpawn`: refreshes tile visuals and registers the blueprint renderer.

## Events

`GameHashes` values used during the lifecycle. Subscribe with `Subscribe((int)GameHashes.X, ...)`.

| Event | Value | Triggered on | When |
|---|---|---|---|
| `NewBuilding` | -1661515756 | `Game.Instance` and the new building | End of `BuildingDef.Build`. `FinishConstruction` also triggers it on `GameplayEventManager`. |
| `NewConstruction` | 2121280625 | The new building | End of `Constructable.FinishConstruction`, data is the `Constructable` |
| `Cancel` | 2127324410 | Construction site or building | Cancelling construction or deconstruction |
| `MarkForDeconstruct` | -790448070 | The building | `DeconstructTool`. `Deconstructable` queues the errand, or cancels it if one exists. |
| `DeconstructStart` | 1830962028 | The building | A Duplicant starts the deconstruct errand |
| `DeconstructComplete` | -702296337 | The building | Start of `Deconstructable.OnCompleteWork`, before it's deleted |
| `ObjectReplaced` | 1606648047 | The replaced building | Tile replacement, before it's deleted |
| `BuildingCompleteDestroyed` | -21016276 | The building | End of `BuildingComplete.OnCleanUp` |

## Which hook to patch

| Change | Hook | Notes |
|---|---|---|
| Def values: mass, materials, size, power, conduits, HP, decor, overheat, flags | Postfix on `<Config>.CreateBuildingDef`, editing `__result` | Must be set here for anything in [Def-driven components](#def-driven-components) or the recipe. Used by Cheap Wires and Pipes, Low Power Filters, True Insulation. |
| A def value on many buildings at once | Postfix on `BuildingConfigManager.RegisterBuilding`, reading the def from the private `configTable` | Runs after every `CreateBuildingDef` postfix. The prefabs and recipe are already built, so only change values the game reads at runtime (such as heat). See Heat Reducer. |
| Components or component fields on the completed building | Postfix on `<Config>.DoPostConfigureComplete(GameObject go)` | The usual place. Used by Battery No Power Loss, Buffed AETN. |
| Components a config creates in `ConfigureBuildingTemplate` | Postfix on `<Config>.ConfigureBuildingTemplate` | Patch where the vanilla config sets the value. Runs before def-driven components are applied. Used by Better Emptier. |
| The construction site or preview | Postfix on `DoPostConfigureUnderConstruction` or `DoPostConfigurePreview` | Many configs don't override these, see [Pitfalls](#pitfalls). |
| Links to other prefabs | Postfix on `<Config>.ConfigurePost` | Runs after all buildings and entities exist. |
| Strings, build menu entry and research unlock for a new building | Postfix on `Db.Initialize` | Runs before buildings are registered. Use `ModUtil.AddBuildingToPlanScreen` and add the ID to the tech's `unlockedItemIDs`. Used by Radiation Field Lamp. |
| Each building instance, including ones loaded from a save | Postfix on `BuildingComplete.OnSpawn` (protected, named by string) | Filter by `__instance.Def.PrefabID`. Used by True Insulation. |
| Freshly built instances only | Subscribe to `NewBuilding` or `NewConstruction` | Not raised for loaded buildings. |
| Construction behaviour | `Constructable.OnCompleteWork`, `Constructable.FinishConstruction` (private) | |
| Deconstruction refunds | `Deconstructable.SpawnItemsFromConstruction` (private overload) | |

## Pitfalls

- **Patch the concrete config.** `IBuildingConfig.CreateBuildingDef` and `DoPostConfigureComplete`
  are abstract, so patch each config type's override. A config that inherits a method from another
  config (such as the Insulated Conductive Wire Bridge) runs the parent's patched method. A config
  that doesn't override a virtual method (such as `DoPostConfigureUnderConstruction`) has nothing
  to patch on its own type: `AccessTools.Method` returns the base `IBuildingConfig` method, and
  patching that affects every building that doesn't override it.
- **Patching many configs from `OnLoad` can break registration.** Resolving a config's methods for a
  patch runs its static constructor, and some configs call `Db.Get()` there, before the database
  exists. Heat Reducer patches `RegisterBuilding` once instead.
- **Mass changes belong in `CreateBuildingDef`.** `PrimaryElement.MassPerUnit` is set in
  `CreateBuildingComplete` and the recipe in `PostProcess`, both before `DoPostConfigureComplete`.
- **Refunds use the current def.** Deconstruction drops `def.Mass`, not what the building was built
  with. Changing a building's mass changes the refund for buildings built before the mod was
  installed (and after it's removed).
- **Changing the footprint needs `GenerateOffsets`.** `BuildingTemplates.CreateBuildingDef`
  computes the placement offsets from `WidthInCells` and `HeightInCells`. Call
  `def.GenerateOffsets()` after changing either.
- **The def is shared.** Changing `Def` on one instance at runtime changes it for every building of
  that type.
- **DLC buildings may not register.** When a building's DLC isn't active, none of its config methods
  run, so patches on them never fire. That's expected, but don't assume a def exists:
  `Assets.GetBuildingDef` returns null.
- **Strings are read live.** `BuildingDef.Name`, `Desc` and `Effect` read the Strings table each
  time. The name is also copied to `KSelectable` and the tech item during registration, so set it
  before then (in `Db.Initialize` or earlier). An `EFFECT` change can be made later with
  `Strings.Add`.
- **Don't reuse a vanilla ID.** `Assets.AddPrefab` ignores a second prefab with the same tag (with a
  warning) but `Assets.AddBuildingDef` replaces the def, so the two disagree.
- **Saved fields aren't ready in `OnPrefabInit`.** On load they're restored between `OnPrefabInit`
  and `OnSpawn`.
