# Battery No Power Loss

Batteries, Jumbo Batteries and Smart Batteries no longer lose stored power over time.

## What changes

| Building | Vanilla leak | With this mod |
|---|---|---|
| Battery | 1.67 J/s | 0 |
| Jumbo Battery | 3.33 J/s | 0 |
| Smart Battery | 0.67 J/s | 0 |

The build menu description of each battery also loses its "loses charge over time" sentence, so the text matches the behavior.

## How it works

Each battery config adds its `Battery` (or `BatterySmart`) component in `DoPostConfigureComplete` and sets `joulesLostPerSecond`. A postfix on each sets it to 0 and replaces the building's `EFFECT` string with the vanilla text minus the leak sentence.

| Patch | Target | Kind |
|---|---|---|
| `BatteryConfig_DoPostConfigureComplete_Patch` | `BatteryConfig.DoPostConfigureComplete` | Postfix |
| `BatteryMediumConfig_DoPostConfigureComplete_Patch` | `BatteryMediumConfig.DoPostConfigureComplete` | Postfix |
| `BatterySmartConfig_DoPostConfigureComplete_Patch` | `BatterySmartConfig.DoPostConfigureComplete` | Postfix |

## Notes

- The Spaced Out Battery Module (rocket battery, 0.67 J/s) still leaks.
- The replacement descriptions are English only. With a translation installed, these three descriptions switch back to English.
- Patch targets were checked against game build 744825.
