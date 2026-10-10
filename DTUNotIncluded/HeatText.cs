using STRINGS;

namespace DTUNotIncluded
{
    // The game already measures heat in joules. Constants.J2DTU and W2DTU_S are both 1, element specific heat
    // capacities are real J/(g*K) values (water is 4.179), and thermal conductivities are real W/(m*K) values. "DTU"
    // is only a label, so this mod changes text and never converts a number.
    public static class HeatText
    {
        // Templates for the heat rates that the patched call sites now print as power. Vanilla formats those rates
        // as energy and adds "/s" or "per second" in the template. These copies drop that wording, and only the
        // patched call sites use them, so other mods that use the vanilla templates still get the vanilla text.
        // Apply sets them rather than field initializers: mono can run this class's static initializer when Harmony
        // compiles a patched method, at mod load, which is too early to initialize the STRINGS classes.
        public static LocString HEATGENERATED;
        public static LocString HEATCONSUMED;
        public static LocString TOOLTIP_HEATGENERATED;
        public static LocString TOOLTIP_HEATCONSUMED;

        // True once every per-second template has been rewritten. While a translation keeps wording this mod
        // doesn't recognise, the rates stay energy per second (for example "kJ/s") so values and templates still agree.
        public static bool RatesAsPower { get; private set; }

        private static string writtenOperatingName;
        private static string writtenOperatingTooltip;

        // Called after Localization.Initialize, so translated text is already in place.
        public static void Apply()
        {
            // Heat units copy the power units, so translations that localise J and W (Дж, 焦, ...) carry over.
            UI.UNITSUFFIXES.HEAT.DTU = Set("STRINGS.UI.UNITSUFFIXES.HEAT.DTU", UI.UNITSUFFIXES.ELECTRICAL.JOULE);
            UI.UNITSUFFIXES.HEAT.KDTU = Set("STRINGS.UI.UNITSUFFIXES.HEAT.KDTU", UI.UNITSUFFIXES.ELECTRICAL.KILOJOULE);
            UI.UNITSUFFIXES.HEAT.DTU_S = Set("STRINGS.UI.UNITSUFFIXES.HEAT.DTU_S", UI.UNITSUFFIXES.ELECTRICAL.WATT);
            UI.UNITSUFFIXES.HEAT.KDTU_S = Set("STRINGS.UI.UNITSUFFIXES.HEAT.KDTU_S", UI.UNITSUFFIXES.ELECTRICAL.KILOWATT);

            // Material picker tooltips for Thermally Reactive and Slow Heating materials.
            ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.LOW_SPECIFIC_HEAT_CAPACITY = Set(
                "STRINGS.ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.LOW_SPECIFIC_HEAT_CAPACITY",
                FixShcValue(ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.LOW_SPECIFIC_HEAT_CAPACITY));
            ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.HIGH_SPECIFIC_HEAT_CAPACITY = Set(
                "STRINGS.ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.HIGH_SPECIFIC_HEAT_CAPACITY",
                FixShcValue(ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.HIGH_SPECIFIC_HEAT_CAPACITY));

            // Material picker tooltips.
            ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.HIGH_THERMAL_CONDUCTIVITY = Set(
                "STRINGS.ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.HIGH_THERMAL_CONDUCTIVITY",
                FixConductivityValue(FixConductionRule(ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.HIGH_THERMAL_CONDUCTIVITY)));
            ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.LOW_THERMAL_CONDUCTIVITY = Set(
                "STRINGS.ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.LOW_THERMAL_CONDUCTIVITY",
                FixConductivityValue(FixConductionRule(ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.LOW_THERMAL_CONDUCTIVITY)));

            // Vanilla's DTU tutorial says "1 DTU = 1055.06 J" (a BTU), which the simulation doesn't follow. The game
            // never shows it; ThermalUnitsCodex gives it a page under Tutorials > Tips. Its title is kept and its body
            // retires the unit. Only the English text is replaced; a translation keeps its own wording.
            if (((string)MISC.NOTIFICATIONS.DTU.MESSAGEBODY).Contains("Duplicant Thermal Units"))
            {
                MISC.NOTIFICATIONS.DTU.MESSAGEBODY = Set("STRINGS.MISC.NOTIFICATIONS.DTU.MESSAGEBODY",
                    "My Duplicants used to measure heat energy in Duplicant Thermal Units, or DTU. The DTU is now " +
                    "deprecated.\n\n" +
                    "Heat is energy, just like electrical power, so I have gone back to measuring it in Joules (J), " +
                    "and the rate at which it is produced or moved in Watts (W). No numbers had to change: one DTU was " +
                    "always exactly one Joule. The old note that put it at 1055.06 J has been filed with the rest of " +
                    "the Gravitas paperwork.\n\n" +
                    "See " + UI.FormatAsLink("Heat", ThermalUnitsCodex.HeatEntryId) + " for why the unit was invented " +
                    "in the first place.");
            }

            ApplyRateTemplates();
        }

        // "J/(g*ºC)": specific heat capacity, vanilla "(DTU/g)/ºC".
        public static string ShcSuffix()
        {
            return Symbol(UI.UNITSUFFIXES.ELECTRICAL.JOULE) + "/(g*" + Symbol(GameUtil.GetTemperatureUnitSuffix()) + ")";
        }

        // "W/(m*ºC)": thermal conductivity, vanilla "(DTU/(m*s))/ºC".
        public static string ThermalConductivitySuffix()
        {
            return Symbol(UI.UNITSUFFIXES.ELECTRICAL.WATT) + "/(m*" + Symbol(GameUtil.GetTemperatureUnitSuffix()) + ")";
        }

        // Replaces GameUtil.GetFormattedHeatEnergy at call sites whose value is a rate in DTU/s.
        public static string FormatRate(float dtuPerSecond, GameUtil.HeatEnergyFormatterUnit unit)
        {
            return RatesAsPower
                ? GameUtil.GetFormattedHeatEnergyRate(dtuPerSecond, unit)
                : GameUtil.GetFormattedHeatEnergy(dtuPerSecond, unit);
        }

        private static void ApplyRateTemplates()
        {
            // The status item templates are edited in place: StatusItem reads them from the Strings table by key, and
            // only StructureTemperatureComponents and SpaceHeater, both patched, use the "OperatingEnergy" item.
            string operatingName = StripPerSecond(BUILDING.STATUSITEMS.OPERATINGENERGY.NAME, "{0}/s", "{0}", writtenOperatingName);
            string operatingTooltip = StripPerSecond(BUILDING.STATUSITEMS.OPERATINGENERGY.TOOLTIP, "</b> per second", "</b> of heat", writtenOperatingTooltip);
            string generated = StripPerSecond(UI.BUILDINGEFFECTS.HEATGENERATED, "{0}/s", "{0}", null);
            string consumed = StripPerSecond(UI.BUILDINGEFFECTS.HEATCONSUMED, "{0}/s", "{0}", null);
            string generatedTooltip = StripPerSecond(UI.BUILDINGEFFECTS.TOOLTIPS.HEATGENERATED, " per second\n", " of heat\n", null);
            string consumedTooltip = StripPerSecond(UI.BUILDINGEFFECTS.TOOLTIPS.HEATCONSUMED, " per second\n", " of heat\n", null);

            RatesAsPower = operatingName != null && operatingTooltip != null && generated != null && consumed != null
                && generatedTooltip != null && consumedTooltip != null;
            if (!RatesAsPower)
            {
                HEATGENERATED = UI.BUILDINGEFFECTS.HEATGENERATED;
                HEATCONSUMED = UI.BUILDINGEFFECTS.HEATCONSUMED;
                TOOLTIP_HEATGENERATED = UI.BUILDINGEFFECTS.TOOLTIPS.HEATGENERATED;
                TOOLTIP_HEATCONSUMED = UI.BUILDINGEFFECTS.TOOLTIPS.HEATCONSUMED;
                Debug.Log("[DTUNotIncluded] Heat rate templates have unrecognised wording, so heat rates stay in J/s.");
                return;
            }

            writtenOperatingName = operatingName;
            writtenOperatingTooltip = operatingTooltip;
            BUILDING.STATUSITEMS.OPERATINGENERGY.NAME = Set("STRINGS.BUILDING.STATUSITEMS.OPERATINGENERGY.NAME", operatingName);
            BUILDING.STATUSITEMS.OPERATINGENERGY.TOOLTIP = Set("STRINGS.BUILDING.STATUSITEMS.OPERATINGENERGY.TOOLTIP", operatingTooltip);
            HEATGENERATED = new LocString(generated);
            HEATCONSUMED = new LocString(consumed);
            TOOLTIP_HEATGENERATED = new LocString(generatedTooltip);
            TOOLTIP_HEATCONSUMED = new LocString(consumedTooltip);
        }

        // Returns the template without its per-second wording, or null when the wording isn't there. A template this
        // class already rewrote in place counts as a match, so Apply can run more than once.
        private static string StripPerSecond(string template, string perSecond, string replacement, string written)
        {
            if (template == written)
            {
                return template;
            }
            return template.Contains(perSecond) ? template.Replace(perSecond, replacement) : null;
        }

        // The details panel tooltips are vanilla's templates, filled with the bare value. Vanilla fills them with the
        // whole label plus the unit ("Specific Heat Capacity: 0.790 (DTU/g)/ K is required to heat 1 g ..."), but the
        // sentences already spell the unit out. The temperature suffix is trimmed, since vanilla's reads "by 1  K".
        public static string ShcTooltip(string displayValue)
        {
            return ((string)UI.ELEMENTAL.SHC.TOOLTIP)
                .Replace("{SPECIFIC_HEAT_CAPACITY}", displayValue + UI.UNITSUFFIXES.ELECTRICAL.JOULE)
                .Replace("{TEMPERATURE_UNIT}", Symbol(GameUtil.GetTemperatureUnitSuffix()));
        }

        // "This object can conduct heat to other materials at a rate of {THERMAL_CONDUCTIVITY} W for each degree
        // {TEMPERATURE_UNIT} difference", then the conduction rule.
        public static string ThermalConductivityTooltip(string displayValue)
        {
            return FixConductionRule(((string)UI.ELEMENTAL.THERMALCONDUCTIVITY.TOOLTIP)
                .Replace("{THERMAL_CONDUCTIVITY}", displayValue)
                .Replace("{TEMPERATURE_UNIT}", Symbol(GameUtil.GetTemperatureUnitSuffix())));
        }

        // Vanilla says "Between two objects, the rate of heat transfer will be determined by the object with the lowest
        // Thermal Conductivity", which is only true for some pairs. Only that phrase changes, to something deliberately
        // vague: tile to tile uses the geometric mean (sqrt(k1 * k2)) with extra factors for solid-gas and
        // liquid-liquid pairs, creatures and debris use the lower value (SimUtil.CalculateEnergyFlow), and buildings
        // use the product with the tile they occupy. See README for sources. The material tooltips put the word in
        // italics ("<i>lowest</i>"); their keyword styling is kept.
        private static string FixConductionRule(string text)
        {
            return text
                .Replace("will be determined by the object with the lowest ", "is usually an average of both objects' ")
                .Replace("will be determined by the object with the <i>lowest</i> ", "is usually an average of both objects' ");
        }

        // The material tooltips end with "Thermal Conductivity: {1} W per degree K difference (Oxygen: 0.024 W)". A
        // label with a value reads better in unit notation. {1} is the raw value, which is always per kelvin.
        private static string FixConductivityValue(string text)
        {
            string unit = Symbol(UI.UNITSUFFIXES.ELECTRICAL.WATT) + "/(m*K)";
            return text.Replace("{1} W per degree K difference (Oxygen: 0.024 W)", "{1} " + unit + " (Oxygen: 0.024 " + unit + ")");
        }

        // The material tooltips end with "Specific Heat Capacity: {1} DTU to raise 1g by 1K". Like the conductivity value
        // line, a label with a value reads better in unit notation. {1} is the raw value, which is always per kelvin. A
        // translation with other wording only has its "{1} DTU" changed to joules.
        private static string FixShcValue(string text)
        {
            string unit = Symbol(UI.UNITSUFFIXES.ELECTRICAL.JOULE) + "/(g*K)";
            return text
                .Replace("{1} DTU to raise 1g by 1K", "{1} " + unit)
                .Replace("{1} DTU", "{1}" + UI.UNITSUFFIXES.ELECTRICAL.JOULE);
        }

        // Unit suffixes start with a space (" J", " ºC"); inside a compound unit they don't.
        private static string Symbol(string suffix)
        {
            return suffix.Trim();
        }

        // LocString's implicit string conversion reads its own text, while StatusItem and LocString.ToString read
        // the Strings table, so both are updated.
        private static LocString Set(string key, string text)
        {
            Strings.Add(key, text);
            return new LocString(text, key);
        }
    }
}
