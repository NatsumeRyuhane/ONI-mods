# Joules and Watts

Shows heat in joules (J) and heat flow in watts (W) instead of DTU and DTU/s, and simplifies the heat units in material and element info.

## Why no numbers change

The game already measures heat in joules. `Constants.J2DTU` and `Constants.W2DTU_S` are both 1. Element specific heat capacities are real J/(g*K) values (water is 4.179), and thermal conductivities are real W/(m*K) values. "DTU" is only a label, so the mod changes text and never converts a value. The vanilla tutorial's "1 DTU = 1055.06 J" (one BTU) isn't what the simulation does.

## What changes

| Where | Vanilla | With this mod |
|---|---|---|
| Heat energy | `250 DTU`, `1.5 kDTU` | `250 J`, `1.5 kJ` |
| Heat rate | `0.56789 DTU/s`, `1.234568 kDTU/s` | `0.57 W`, `1.23 kW` |
| Building effect | `Heat: +20 kDTU/s` | `Heat: +20 kW` |
| Building effect tooltip | `Generates 20 kDTU per second` | `Generates 20 kW of heat` |
| Heat Production status | `Heat Production: 375 DTU/s` | `Heat Production: 375 W` |
| Specific heat capacity | `4.179 (DTU/g)/ ºC` | `4.179 J/(g*ºC)` |
| Thermal conductivity | `0.609 (DTU/(m*s))/ ºC` | `0.609 W/(m*ºC)` |
| Material tooltip, heat capacity | `Specific Heat Capacity: {1} DTU to raise 1g by 1K` | `Specific Heat Capacity: {1} J/(g*K)` |
| Material tooltip, conductivity | `will be determined by the object with the lowest Thermal Conductivity`, `{1} W per degree K difference (Oxygen: 0.024 W)` | `is usually an average of both objects' Thermal Conductivity`, `{1} W/(m*K) (Oxygen: 0.024 W/(m*K))` |
| Details panel, heat capacity tooltip | `Specific Heat Capacity: 0.790(DTU/g)/ K is required to heat 1 g ... by 1  K` | `0.790 J is required to heat 1 g ... by 1 K` |
| Details panel, conductivity tooltip | `... at a rate of Thermal Conductivity: 3.390(DTU/(m*s))/ K W for each degree  K difference`, plus the lowest-conductivity rule | `... at a rate of 3.390 W for each degree K difference`, plus the "usually an average" rule |
| Tutorial: Duplicant Thermal Units | Never shown; says "1 DTU = 1055.06 J" | A page under Database > Tutorials > Tips saying the DTU is deprecated, linking to Heat |
| Database > Tutorials > Systems > Heat | | A new Thermal Units section after Thermal Energy, linking to the DTU tutorial |

Heat rates now use the power formatter, so they're rounded to two decimals like power.

Vanilla says heat transfer between two objects is set by the lower Thermal Conductivity. That's only true for some pairs. The tooltips keep vanilla's wording and change only that phrase. They now say it's "usually an average of both objects' Thermal Conductivity", and are deliberately vague about which average and when. What the sim does:

- Tile to tile: the geometric mean, sqrt(k1 * k2), with extra factors for solid-gas and liquid-liquid pairs.
- Debris, critters and Duplicants with a tile: the lower of the two. For creatures this is `SimUtil.CalculateEnergyFlow` (`Math.Min`).
- Buildings with the tiles they occupy: the product of the two.

Tile-to-tile and building-to-tile conduction happen in the native sim, so those rules come from the community's measurements on the [Thermal Conductivity](https://oxygennotincluded.wiki.gg/wiki/Thermal_Conductivity) wiki page, not from the game's C# code.

The Thermal Units section explains that heat and power share units because both are energy. It then notes, in the voice of the tutorial notices, that Gravitas machines somehow give off far more heat than the power they draw (the Electrolyzer draws 120 W and releases 1.25 kW), and why the Duplicants' DTU didn't help.

Vanilla has a "Tutorial: Duplicant Thermal Units" string (`STRINGS.MISC.NOTIFICATIONS.DTU`) that nothing in the game displays. The mod keeps its title, rewrites its body to retire the DTU, and gives it a page in the Tips list, laid out like the other tutorial tips. The two pages link to each other.

## How it works

The heat unit suffixes (`STRINGS.UI.UNITSUFFIXES.HEAT`) are set to copies of the power suffixes (`UNITSUFFIXES.ELECTRICAL`). A translation that localises J and W, such as Дж or 焦, therefore also applies to heat. The text is changed in a `Localization.Initialize` postfix, because mods load before the selected language is applied. Each string is written both to its `LocString` field and to the `Strings` table, since code reads both.

Several vanilla call sites pass a heat *rate* to `GameUtil.GetFormattedHeatEnergy`, the energy formatter, and put "/s" or "per second" in the template. With joules they would read "kJ/s". A transpiler sends those calls to `HeatText.FormatRate` (watts), and their building effect templates to copies without the per-second wording. Other mods that use the vanilla templates keep the vanilla text. The "Heat Production" status item templates are edited in place, because `StatusItem` reads them from the `Strings` table by key. Only the patched classes use that status item.

If the current language's templates don't have the English per-second wording, the templates are left alone. Rates then keep the energy formatter, for example `20 kJ/s`, so the value and its template still agree. The game log notes this once. The tutorial text is only replaced while it's in English.

| Patch | Target | Kind |
|---|---|---|
| `Localization_Initialize_Patch` | `Localization.Initialize` | Postfix, applies the text |
| `AdditionalDetailsPanel_RefreshDetailsPanel_Patch` | `AdditionalDetailsPanel.RefreshDetailsPanel` (private) | Postfix, resets the heat capacity and conductivity tooltips |
| `CodexCache_CodexCacheInit_Patch` | `CodexCache.CodexCacheInit` | Postfix, adds the Thermal Units section |
| `CodexEntryGenerator_GenerateTutorialNotificationEntries_Patch` | `CodexEntryGenerator.GenerateTutorialNotificationEntries` | Postfix, adds the DTU tutorial page to Tips |
| `GameUtil_AppendFormattedHeatEnergyRate_Patch` | `GameUtil.AppendFormattedHeatEnergyRate` | Prefix, formats as power |
| `GameUtil_GetSHCSuffix_Patch`, `GameUtil_GetFormattedSHC_Patch` | `GameUtil.GetSHCSuffix`, `GetFormattedSHC` | Prefix, skips original |
| `GameUtil_GetThermalConductivitySuffix_Patch`, `GameUtil_GetFormattedThermalConductivity_Patch` | `GameUtil.GetThermalConductivitySuffix`, `GetFormattedThermalConductivity` | Prefix, skips original |
| `HeatRateCallSites_Patch` | `Building.EffectDescriptors`, `DirectVolumeHeater.GetDescriptors`, `LiquidCooledFan.GetDescriptors`, `RefrigeratorController.Def.GetDescriptors`, two `SpaceHeater` and one `StructureTemperatureComponents` status item lambdas | Transpiler |

## Notes

- The status item lambdas are compiler generated, so `HeatRateCallSites_Patch` finds its targets by scanning those classes for calls to `GetFormattedHeatEnergy` instead of naming them. On game build 744825 it patches the 7 methods listed above.
- `AirConditioner` also calls `GetFormattedHeatEnergy`, but for the energy to cool one batch of gas or liquid, so it keeps the energy formatter: `Heat: +X kJ (Approximate Value)`.
- The DTU tutorial page is added when the Tutorials categories are built, before the codex fills in each category's list of pages, so it appears in the Tips list. Its ID is `MISCELLANEOUSTIPSDTU`.
- The Thermal Units section is English only. It's inserted after the Thermal Energy text on the Heat page, or at the end of the page if that text isn't found. Its 120 W and 1.25 kW are the Electrolyzer's vanilla values, written into the text.
- Tooltips keep vanilla's sentences and change only the parts that are wrong. Both details panel tooltips are built from vanilla's own templates (`UI.ELEMENTAL.SHC.TOOLTIP`, `UI.ELEMENTAL.THERMALCONDUCTIVITY.TOOLTIP`), filled with the bare value instead of the whole label plus unit. The details panel keeps vanilla's "W for each degree K difference", which leaves out the meters of W/(m*K). The material tooltips' value lines use unit notation, J/(g*K) and W/(m*K), which is shorter. Every tile is the same size, so the meters only scale every conductivity by the same amount.
- `RefreshDetailsPanel` is private and named by string. The postfix calls `SetLabel` again with the same label IDs (`shc`, `tc`), which updates the labels vanilla just created.
- The compound units use `*` instead of `·` because the game's strings never use a middle dot, so the font may not have one.
- Vanilla shows the Hydrofan's cooling as `-80 DTU/s` from a field named `coolingKilowatts`. The mod shows the same number as `-80 W` and doesn't try to fix that.
- Other mods that format heat themselves, with their own "DTU" text, aren't changed.
- Patch targets were checked against game build 744825.
