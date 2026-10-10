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
| Material tooltip | `{1} DTU to raise 1g by 1K` | `{1} J to raise 1g by 1K` |
| Tutorial | "Duplicant Thermal Units", "1 DTU = 1055.06 J" | Joules and Watts, "1 W = 1 J/s" |

Heat rates now use the power formatter, so they're rounded to two decimals like power.

## How it works

The heat unit suffixes (`STRINGS.UI.UNITSUFFIXES.HEAT`) are set to copies of the power suffixes (`UNITSUFFIXES.ELECTRICAL`). A translation that localises J and W, such as Дж or 焦, therefore also applies to heat. The text is changed in a `Localization.Initialize` postfix, because mods load before the selected language is applied. Each string is written both to its `LocString` field and to the `Strings` table, since code reads both.

Several vanilla call sites pass a heat *rate* to `GameUtil.GetFormattedHeatEnergy`, the energy formatter, and put "/s" or "per second" in the template. With joules they would read "kJ/s". A transpiler sends those calls to `HeatText.FormatRate` (watts), and their building effect templates to copies without the per-second wording. Other mods that use the vanilla templates keep the vanilla text. The "Heat Production" status item templates are edited in place, because `StatusItem` reads them from the `Strings` table by key. Only the patched classes use that status item.

If the current language's templates don't have the English per-second wording, the templates are left alone. Rates then keep the energy formatter, for example `20 kJ/s`, so the value and its template still agree. The game log notes this once. The tutorial text is only replaced while it's in English.

| Patch | Target | Kind |
|---|---|---|
| `Localization_Initialize_Patch` | `Localization.Initialize` | Postfix, applies the text |
| `GameUtil_AppendFormattedHeatEnergyRate_Patch` | `GameUtil.AppendFormattedHeatEnergyRate` | Prefix, formats as power |
| `GameUtil_GetSHCSuffix_Patch`, `GameUtil_GetFormattedSHC_Patch` | `GameUtil.GetSHCSuffix`, `GetFormattedSHC` | Prefix, skips original |
| `GameUtil_GetThermalConductivitySuffix_Patch`, `GameUtil_GetFormattedThermalConductivity_Patch` | `GameUtil.GetThermalConductivitySuffix`, `GetFormattedThermalConductivity` | Prefix, skips original |
| `HeatRateCallSites_Patch` | `Building.EffectDescriptors`, `DirectVolumeHeater.GetDescriptors`, `LiquidCooledFan.GetDescriptors`, `RefrigeratorController.Def.GetDescriptors`, two `SpaceHeater` and one `StructureTemperatureComponents` status item lambdas | Transpiler |

## Notes

- The status item lambdas are compiler generated, so `HeatRateCallSites_Patch` finds its targets by scanning those classes for calls to `GetFormattedHeatEnergy` instead of naming them. On game build 744825 it patches the 7 methods listed above.
- `AirConditioner` also calls `GetFormattedHeatEnergy`, but for the energy to cool one batch of gas or liquid, so it keeps the energy formatter: `Heat: +X kJ (Approximate Value)`.
- The compound units use `*` instead of `·` because the game's strings never use a middle dot, so the font may not have one.
- Vanilla shows the Hydrofan's cooling as `-80 DTU/s` from a field named `coolingKilowatts`. The mod shows the same number as `-80 W` and doesn't try to fix that.
- Other mods that format heat themselves, with their own "DTU" text, aren't changed.
- Patch targets were checked against game build 744825.
