# ONI-mods

A collection of mods for [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included). Each folder is an independent mod, and `ONI-mods.sln` builds all of them.

## Mods

Each mod's `README.md` lists the vanilla values it changes, its options and the game methods it patches.

| Mod | What it does |
|---|---|
| [Abyssalite to Tungsten](AbyssaliteToTungsten) | Adds an Abyssalite to Tungsten recipe to the Metal Refinery and the Molecular Forge |
| [Battery No Power Loss](BatteryNoPowerLoss) | Batteries, Jumbo Batteries and Smart Batteries stop leaking charge |
| [Better Emptier](BetterEmptier) | Configurable capacity and release speed for Bottle and Canister Emptiers |
| [BuffedAETN](BuffedAETN) | Configurable AETN cooling, minimum temperature and Hydrogen use; can't be flooded |
| [BuffedWheezewort](BuffedWheezewort) | Configurable Wheezewort radiation, cooling and fertilizer, and a colder temperature range |
| [Cheap Wires And Pipes](CheapWiresAndPipes) | Wires, pipes, rails and their bridges cost 1 kg and have no decor penalty |
| [Conservation of Mass](ConservationOfMass) | Dug or destroyed tiles drop all of their mass instead of half |
| [DTU Not Included](DTUNotIncluded) | Shows heat in joules (J) and watts (W) instead of DTU and DTU/s, and simplifies the heat units of materials |
| [Electrolyzer No Overpressure](ElectrolyzerNoOverPressure) | Electrolyzers never stop for overpressure |
| [Fast Temp Shift Plate Build](FastTempShiftPlateBuild) | Temperature Shift Plates build in 6 s, deconstruct in 3 s and cost 500 kg |
| [Heat Reducer](HeatReducer) | Caps powered buildings' heat at their power draw |
| [Irradiation Sterilization](IrradiationSterilization) | Radiation kills a share of the germs every cycle, in tiles, pipes, items and buildings |
| [Low Power Filters](LowPowerFilters) | Configurable power draw for gas, liquid and solid filters (default 10 W) |
| [No Phase Transition Damage](NoPhaseTransitionDamage) | Pipes take no damage when their contents freeze or boil |
| [Radiation Field Lamp](RadiationFieldLamp) | A new lamp building that keeps an even, adjustable radiation field in a 4x3 area |
| [Storage Tile Instant Switch](StorageTileInstantSwitch) | Changing a Storage Tile's item takes effect right away, with no Duplicant errand |
| [Thermally Conductive](ThermallyConductive) | Radiant pipes and Temperature Shift Plates conduct heat 1000 times better |
| [Thermo Heat Production](ThermoHeatProduction) | Thermo Regulators and Thermo Aquatuners show a Heat Production status, like the Steam Turbine |
| [True Insulation](TrueInsulation) | Insulated pipes, Insulation Tiles and pressure doors fully insulate |

## Requirements

- Oxygen Not Included, installed through Steam
- .NET Framework 4.7.1 targeting pack with MSBuild (Visual Studio or Rider on Windows), or Mono on macOS and Linux (`msbuild` and `nuget`)

## Setup

1. Restore NuGet packages (PLib, Newtonsoft.Json, ILRepack). They are not stored in git:

   ```sh
   nuget restore ONI-mods.sln
   ```

2. The build reads the game's assemblies from its `Managed` folder. The default location is the Steam install for your OS:

   | OS      | Default `Managed` folder |
   |---------|--------------------------|
   | Windows | `%ProgramFiles(x86)%\Steam\steamapps\common\OxygenNotIncluded\OxygenNotIncluded_Data\Managed` |
   | macOS   | `~/Library/Application Support/Steam/steamapps/common/OxygenNotIncluded/OxygenNotIncluded.app/Contents/Resources/Data/Managed` |
   | Linux   | `~/.local/share/Steam/steamapps/common/OxygenNotIncluded/OxygenNotIncluded_Data/Managed` |

   If the game is installed elsewhere (for example, on another Steam library drive), set `ONI_MANAGED_DIR` to that folder, or pass `-p:OniManagedDir=...` to MSBuild. The build stops with a message naming the folder it searched when `Assembly-CSharp.dll` is missing.

## Building

Visual Studio or Rider: open `ONI-mods.sln` and build the `Release` configuration.

Command line:

```sh
msbuild ONI-mods.sln /p:Configuration=Release
```

### Packaging for the game

To build every mod and assemble ready-to-copy mod folders, run:

```sh
msbuild build/package.proj
```

This builds the solution in `Release` and writes one folder per mod to `build/out/`. Each folder holds the mod DLL, a generated `mod.yaml` and a generated `mod_info.yaml`. Copy the folders into the game's `mods/Dev` folder to test them. The per-mod metadata (titles, descriptions, `staticID`s, minimum build) lives in `build/package.proj`. The solution is not changed, so Visual Studio and Rider load it as before.

Each mod's output is written to `<Mod>/bin/Release/`, containing `<Mod>.dll` and `<Mod>.dll.config`. Release builds merge PLib and Newtonsoft.Json into the mod DLL, so each mod is self-contained.

## Notes

- The game's DLLs are read from your installed copy and are never copied into a mod's output or committed to this repo. The mods target the game version you have installed, so they may need updates after a game update.
- Shared build settings live in `Directory.Build.props` and `Directory.Build.targets`. The Release packaging step is in `build/ILRepack.targets`.
- [docs/vanilla-buildings.md](docs/vanilla-buildings.md) maps vanilla building names to their prefab IDs.

## Code conventions

Formatting is set in `.editorconfig`: UTF-8 without a BOM, LF line endings, 4-space indents and braces on their own lines. Beyond that:

- **Language.** Projects compile as C# 7.3 for .NET Framework 4.7.1, so C# 8+ features (switch expressions, `using var`, `??=`, default interface members) are unavailable.
- **Layout.** Every type lives in a namespace named after the mod. A typical mod has:
  - `ModLoad.cs`: `public class ModLoad : UserMod2`. Its `base.OnLoad(harmony)` applies every `[HarmonyPatch]` class in the assembly. Mods with options also register their PLib options here. Mods with no setup still declare an empty `ModLoad`, so the game doesn't log a "no classes inheriting from UserMod" warning.
  - `Patch.cs`: `public static class Patch`, holding one nested class per patch, named `<TargetType>_<Method>_Patch`. When one postfix applies to several configs, a single patch class lists them in `TargetMethods`.
  - `Config.cs`: the PLib options class, if the mod has options.
- **Patch targets.** Name public targets with `nameof`, so a renamed method fails the build. Private members have to be named by string; add a comment saying so, since a game update can break them at load time. Don't patch many configs' `CreateBuildingDef` from `OnLoad` by hand when a registration hook will do (see Heat Reducer's README).
- **Comments.** Explain why, not what, and record the vanilla values a patch changes, so a reader can tell what the mod does without decompiling the game.
- **Naming.** PascalCase for types, members, constants and static readonly fields; camelCase for other private fields.
- **Save data.** Saves store a component by its full type name and its `[Serialize]` fields by name. Don't rename either once a mod is released.

Some newer mods differ in small ways, such as naming the `UserMod2` class after the mod.

## License

GPL-3.0. See [LICENSE](LICENSE).
