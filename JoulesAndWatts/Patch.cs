using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using STRINGS;
using UnityEngine;

namespace JoulesAndWatts
{
    public static class Patch
    {
        // Mods load before Localization.Initialize applies the selected language, so the text changes go after it.
        [HarmonyPatch(typeof(Localization), nameof(Localization.Initialize))]
        public static class Localization_Initialize_Patch
        {
            public static void Postfix()
            {
                HeatText.Apply();
            }
        }

        // Vanilla builds the Specific Heat Capacity and Thermal Conductivity tooltips by pasting the whole label
        // ("Specific Heat Capacity: 0.790") plus the unit into a sentence, giving "Specific Heat Capacity: 0.790 (DTU/g)/ K
        // is required to heat 1 g ... by 1  K". This sets both labels again with the same text and new tooltips; SetLabel
        // updates a label that already exists. The element and conductivity are resolved the same way as vanilla.
        // RefreshDetailsPanel is private, so it's named by string.
        [HarmonyPatch(typeof(AdditionalDetailsPanel), "RefreshDetailsPanel")]
        public static class AdditionalDetailsPanel_RefreshDetailsPanel_Patch
        {
            public static void Postfix(CollapsibleDetailContentPanel targetPanel, GameObject targetEntity)
            {
                Element element = SelectedElement(targetEntity);
                if (element == null || element.id == SimHashes.Vacuum || element.id == SimHashes.Void)
                {
                    return;
                }
                float conductivity = element.thermalConductivity;
                Building building = targetEntity.GetComponent<Building>();
                if (building != null)
                {
                    conductivity *= building.Def.ThermalConductivity;
                }
                string shc = GameUtil.GetDisplaySHC(element.specificHeatCapacity).ToString("0.000");
                string tc = GameUtil.GetDisplayThermalConductivity(conductivity).ToString("0.000");
                targetPanel.SetLabel("shc", string.Format(UI.ELEMENTAL.SHC.NAME, shc), HeatText.ShcTooltip(shc));
                targetPanel.SetLabel("tc", string.Format(UI.ELEMENTAL.THERMALCONDUCTIVITY.NAME, tc),
                    HeatText.ThermalConductivityTooltip(tc));
            }

            private static Element SelectedElement(GameObject target)
            {
                PrimaryElement primaryElement = target.GetComponent<PrimaryElement>();
                if (primaryElement != null)
                {
                    return primaryElement.Element;
                }
                CellSelectionObject cell = target.GetComponent<CellSelectionObject>();
                if (cell != null)
                {
                    return cell.element;
                }
                BackwallSelectionObject backwall = target.GetComponent<BackwallSelectionObject>();
                return backwall != null ? backwall.element : null;
            }
        }

        // The codex builds its entries here, from the YAML in StreamingAssets/codex and from generated pages.
        [HarmonyPatch(typeof(CodexCache), nameof(CodexCache.CodexCacheInit))]
        public static class CodexCache_CodexCacheInit_Patch
        {
            public static void Postfix()
            {
                ThermalUnitsCodex.InsertHeatSection();
            }
        }

        // Builds the Tutorials sub-categories (Videos, Systems, Tips) and returns them by ID.
        [HarmonyPatch(typeof(CodexEntryGenerator), nameof(CodexEntryGenerator.GenerateTutorialNotificationEntries))]
        public static class CodexEntryGenerator_GenerateTutorialNotificationEntries_Patch
        {
            public static void Postfix(Dictionary<string, CodexEntry> __result)
            {
                ThermalUnitsCodex.AddDtuTip(__result);
            }
        }

        // Heat rates use the power formatter. Vanilla appends the raw float ("1234.5677 kDTU/s"); power rounds to two
        // decimals ("1234.57 kW").
        [HarmonyPatch(typeof(GameUtil), nameof(GameUtil.AppendFormattedHeatEnergyRate))]
        public static class GameUtil_AppendFormattedHeatEnergyRate_Patch
        {
            public static bool Prefix(StringBuilder builder, float dtu_s, GameUtil.HeatEnergyFormatterUnit unit)
            {
                GameUtil.WattageFormatterUnit wattageUnit;
                switch (unit)
                {
                    case GameUtil.HeatEnergyFormatterUnit.DTU_S:
                        wattageUnit = GameUtil.WattageFormatterUnit.Watts;
                        break;
                    case GameUtil.HeatEnergyFormatterUnit.KDTU_S:
                        wattageUnit = GameUtil.WattageFormatterUnit.Kilowatts;
                        break;
                    default:
                        wattageUnit = GameUtil.WattageFormatterUnit.Automatic;
                        break;
                }
                GameUtil.AppendFormattedWattage(builder, dtu_s, wattageUnit);
                return false;
            }
        }

        [HarmonyPatch(typeof(GameUtil), nameof(GameUtil.GetSHCSuffix))]
        public static class GameUtil_GetSHCSuffix_Patch
        {
            public static bool Prefix(ref string __result)
            {
                __result = HeatText.ShcSuffix();
                return false;
            }
        }

        // Vanilla: "4.179 (DTU/g)/ ºC".
        [HarmonyPatch(typeof(GameUtil), nameof(GameUtil.GetFormattedSHC))]
        public static class GameUtil_GetFormattedSHC_Patch
        {
            public static bool Prefix(float shc, ref string __result)
            {
                __result = GameUtil.GetDisplaySHC(shc).ToString("0.000") + " " + HeatText.ShcSuffix();
                return false;
            }
        }

        [HarmonyPatch(typeof(GameUtil), nameof(GameUtil.GetThermalConductivitySuffix))]
        public static class GameUtil_GetThermalConductivitySuffix_Patch
        {
            public static bool Prefix(ref string __result)
            {
                __result = HeatText.ThermalConductivitySuffix();
                return false;
            }
        }

        // Vanilla: "0.609 (DTU/(m*s))/ ºC".
        [HarmonyPatch(typeof(GameUtil), nameof(GameUtil.GetFormattedThermalConductivity))]
        public static class GameUtil_GetFormattedThermalConductivity_Patch
        {
            public static bool Prefix(float tc, ref string __result)
            {
                __result = GameUtil.GetDisplayThermalConductivity(tc).ToString("0.000") + " " + HeatText.ThermalConductivitySuffix();
                return false;
            }
        }

        // These classes pass a heat rate (DTU/s) to GetFormattedHeatEnergy and put "/s" or "per second" in the
        // template. With joules that would read "kJ/s", so their calls go to HeatText.FormatRate (watts) and their
        // building effect templates to HeatText's copies without the per-second wording. The status item lambdas are
        // compiler generated, so methods are found by scanning each class and its nested classes for the call.
        // AirConditioner also calls GetFormattedHeatEnergy, but for the energy to cool one batch, so it's left alone.
        [HarmonyPatch]
        public static class HeatRateCallSites_Patch
        {
            private static readonly Type[] Owners =
            {
                typeof(Building),                     // EffectDescriptors: building heat
                typeof(DirectVolumeHeater),           // GetDescriptors
                typeof(LiquidCooledFan),              // GetDescriptors: Hydrofan
                typeof(RefrigeratorController.Def),   // GetDescriptors
                typeof(SpaceHeater),                  // "OperatingEnergy" status item lambdas
                typeof(StructureTemperatureComponents) // "OperatingEnergy" status item lambda
            };

            private static readonly MethodInfo EnergyFormatter =
                AccessTools.Method(typeof(GameUtil), nameof(GameUtil.GetFormattedHeatEnergy));

            private static readonly MethodInfo RateFormatter =
                AccessTools.Method(typeof(HeatText), nameof(HeatText.FormatRate));

            private static readonly KeyValuePair<FieldInfo, FieldInfo>[] Templates =
            {
                Template(typeof(UI.BUILDINGEFFECTS), nameof(UI.BUILDINGEFFECTS.HEATGENERATED), nameof(HeatText.HEATGENERATED)),
                Template(typeof(UI.BUILDINGEFFECTS), nameof(UI.BUILDINGEFFECTS.HEATCONSUMED), nameof(HeatText.HEATCONSUMED)),
                Template(typeof(UI.BUILDINGEFFECTS.TOOLTIPS), nameof(UI.BUILDINGEFFECTS.TOOLTIPS.HEATGENERATED), nameof(HeatText.TOOLTIP_HEATGENERATED)),
                Template(typeof(UI.BUILDINGEFFECTS.TOOLTIPS), nameof(UI.BUILDINGEFFECTS.TOOLTIPS.HEATCONSUMED), nameof(HeatText.TOOLTIP_HEATCONSUMED))
            };

            public static IEnumerable<MethodBase> TargetMethods()
            {
                return Owners
                    .SelectMany(WithNestedTypes)
                    .Where(type => !type.ContainsGenericParameters)
                    .SelectMany(AccessTools.GetDeclaredMethods)
                    .Where(method => method.GetMethodBody() != null
                        && PatchProcessor.GetOriginalInstructions(method).Any(ins => ins.Calls(EnergyFormatter)));
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                foreach (CodeInstruction ins in instructions)
                {
                    if (ins.Calls(EnergyFormatter))
                    {
                        ins.operand = RateFormatter;
                    }
                    else if (ins.opcode == OpCodes.Ldsfld)
                    {
                        foreach (KeyValuePair<FieldInfo, FieldInfo> template in Templates)
                        {
                            if (ins.LoadsField(template.Key))
                            {
                                ins.operand = template.Value;
                                break;
                            }
                        }
                    }
                    yield return ins;
                }
            }

            private static IEnumerable<Type> WithNestedTypes(Type type)
            {
                yield return type;
                foreach (Type nested in AccessTools.InnerTypes(type).SelectMany(WithNestedTypes))
                {
                    yield return nested;
                }
            }

            private static KeyValuePair<FieldInfo, FieldInfo> Template(Type vanillaType, string vanillaField, string ownField)
            {
                return new KeyValuePair<FieldInfo, FieldInfo>(
                    AccessTools.Field(vanillaType, vanillaField),
                    AccessTools.Field(typeof(HeatText), ownField));
            }
        }
    }
}
