using System.Collections.Generic;
using STRINGS;

namespace JoulesAndWatts
{
    // Adds a "Thermal Units" section to the Heat page (Database > Tutorials > Systems > Heat), right after
    // "Thermal Energy". The text is English only.
    public static class ThermalUnitsCodex
    {
        private const string HeatEntryId = "HEAT";

        private const string Header = "Thermal Units";

        private const string SameUnits =
            "Heat is energy, just like electrical power, so the two are measured in the same units. Thermal energy is " +
            "counted in Joules (J), and the rate at which something produces or moves it is counted in Watts (W), where " +
            "1 W is 1 J every second. A building that gives off 1 kW of heat adds exactly as much energy to its " +
            "surroundings as a 1 kW generator adds to a power grid.\n\n";

        // {0} is the Electrolyzer's linked name. 120 W and 1.25 kW are its vanilla power draw and heat.
        private const string Gravitas =
            "This is where the numbers get uncomfortable. Somehow, Gravitas managed to build machines that give off " +
            "much, much more heat than the power they are supplied. An {0} draws 120 W from the grid and releases " +
            "1.25 kW of heat. None of the Gravitas manuals I have recovered mention where the difference comes from.\n\n";

        private const string Dtu =
            "My Duplicants once tried to get around this confusion by inventing a unit of their own, the DTU " +
            "(Duplicant Thermal Unit). One of them then worked out that a DTU is worth 1055.06 Joules, which would make " +
            "the problem a thousand times worse. A few still insist that a DTU feels warmer than a Joule.\n\n" +
            "I have decided to stick to old-fashioned physics. A Joule is a Joule, whether it comes out of a battery or " +
            "out of the back of an Electrolyzer.\n\n";

        public static void Insert()
        {
            if (CodexCache.entries == null || !CodexCache.entries.TryGetValue(HeatEntryId, out CodexEntry entry))
            {
                Debug.LogWarning("[JoulesAndWatts] No Heat codex entry, so the Thermal Units section wasn't added.");
                return;
            }

            var widgets = new List<ICodexWidget>
            {
                new CodexText(Header, CodexTextStyle.Subtitle),
                new CodexText(SameUnits + string.Format(Gravitas, (string)BUILDINGS.PREFABS.ELECTROLYZER.NAME) + Dtu, CodexTextStyle.Body)
            };

            // Vanilla's Heat page is one container of subtitle/body pairs. Insert after the "Thermal Energy" body,
            // or at the end of the last container if the page has changed.
            string thermalEnergy = CODEX.HEAT.PARAGRAPH_3;
            foreach (ContentContainer container in entry.contentContainers)
            {
                int index = container.content.FindIndex(widget => widget is CodexText text && text.text == thermalEnergy);
                if (index >= 0)
                {
                    container.content.InsertRange(index + 1, widgets);
                    return;
                }
            }
            if (entry.contentContainers.Count > 0)
            {
                entry.contentContainers[entry.contentContainers.Count - 1].content.AddRange(widgets);
            }
        }
    }
}
