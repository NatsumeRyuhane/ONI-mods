# ONI-mods

A collection of mods for [Oxygen Not Included](https://www.klei.com/games/oxygen-not-included). Each folder is an independent mod, and `ONI-mods.sln` builds all of them.

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

Each mod's output is written to `<Mod>/bin/Release/`, containing `<Mod>.dll` and `<Mod>.dll.config`. Release builds merge PLib and Newtonsoft.Json into the mod DLL, so each mod is self-contained.

## Notes

- The game's DLLs are read from your installed copy and are never copied into a mod's output or committed to this repo. The mods target the game version you have installed, so they may need updates after a game update.
- Shared build settings live in `Directory.Build.props` and `Directory.Build.targets`. The Release packaging step is in `build/ILRepack.targets`.

## License

GPL-3.0. See [LICENSE](LICENSE).
