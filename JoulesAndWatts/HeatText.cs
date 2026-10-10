using STRINGS;

namespace JoulesAndWatts
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

            // "{1} DTU to raise 1g by 1K" in the material tooltips.
            ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.LOW_SPECIFIC_HEAT_CAPACITY = Set(
                "STRINGS.ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.LOW_SPECIFIC_HEAT_CAPACITY",
                ReplaceDtu(ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.LOW_SPECIFIC_HEAT_CAPACITY));
            ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.HIGH_SPECIFIC_HEAT_CAPACITY = Set(
                "STRINGS.ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.HIGH_SPECIFIC_HEAT_CAPACITY",
                ReplaceDtu(ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.HIGH_SPECIFIC_HEAT_CAPACITY));

            // Vanilla's tutorial says "1 DTU = 1055.06 J" (a BTU), which the simulation doesn't follow. Only the
            // English text is replaced; a translation keeps its own wording.
            if (((string)MISC.NOTIFICATIONS.DTU.NAME).Contains("Duplicant Thermal Units"))
            {
                MISC.NOTIFICATIONS.DTU.NAME = Set("STRINGS.MISC.NOTIFICATIONS.DTU.NAME", "Tutorial: Joules and Watts");
            }
            if (((string)MISC.NOTIFICATIONS.DTU.MESSAGEBODY).Contains("Duplicant Thermal Units"))
            {
                MISC.NOTIFICATIONS.DTU.MESSAGEBODY = Set("STRINGS.MISC.NOTIFICATIONS.DTU.MESSAGEBODY",
                    "My Duplicants measure heat energy in Joules (J), the same unit as stored power.\n\n" +
                    "Heat that is produced or moved over time is measured in Watts (W).\n\n1 W = 1 J/s");
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
                Debug.Log("[JoulesAndWatts] Heat rate templates have unrecognised wording, so heat rates stay in J/s.");
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

        private static string ReplaceDtu(string text)
        {
            return text.Replace("{1} DTU", "{1}" + UI.UNITSUFFIXES.ELECTRICAL.JOULE);
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
