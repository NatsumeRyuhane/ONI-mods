using System.Collections.Generic;
using STRINGS;

namespace JoulesAndWatts
{
    // Adds a "Thermal Units" section to the Heat page (Database > Tutorials > Systems > Heat), right after
    // "Thermal Energy", and gives vanilla's unused "Tutorial: Duplicant Thermal Units" a page under Tutorials > Tips.
    // The two pages link to each other. The added text is English only.
    public static class ThermalUnitsCodex
    {
        public const string HeatEntryId = "HEAT";

        // CodexCache.AddEntry uppercases IDs and drops underscores; vanilla's tips are MISCELLANEOUSTIPS0 and up.
        public const string DtuTipId = "MISCELLANEOUSTIPSDTU";

        private const string TipsCategoryId = "MISCELLANEOUSTIPS";

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

        // {0} links to the DTU tutorial page.
        private const string Dtu =
            "My Duplicants once tried to get around this confusion by inventing a unit of their own, the {0}. One of " +
            "them then worked out that a DTU is worth 1055.06 Joules, which would make the problem a thousand times " +
            "worse. A few still insist that a DTU feels warmer than a Joule.\n\n" +
            "I have decided to stick to old-fashioned physics. A Joule is a Joule, whether it comes out of a battery or " +
            "out of the back of an Electrolyzer.\n\n";

        // Called with the categories GenerateTutorialNotificationEntries returns, before CodexCacheInit lists each
        // category's pages, so the new page shows up in the Tips list.
        public static void AddDtuTip(Dictionary<string, CodexEntry> lessonCategories)
        {
            if (lessonCategories == null || !lessonCategories.TryGetValue(TipsCategoryId, out CodexEntry tips)
                || !(tips is CategoryEntry category))
            {
                Debug.LogWarning("[JoulesAndWatts] No Tips codex category, so the DTU tutorial page wasn't added.");
                return;
            }

            // Same layout as vanilla's tutorial tip pages.
            string title = MISC.NOTIFICATIONS.DTU.NAME;
            var containers = new List<ContentContainer>();
            CodexEntryGenerator.GenerateTitleContainers(title, containers);
            containers.Add(new ContentContainer(new List<ICodexWidget>
            {
                new CodexText(MISC.NOTIFICATIONS.DTU.MESSAGEBODY, CodexTextStyle.Body, title)
            }, ContentContainer.ContentLayout.Vertical));
            containers.Add(new ContentContainer(new List<ICodexWidget>
            {
                new CodexSpacer(),
                new CodexSpacer()
            }, ContentContainer.ContentLayout.Vertical));

            var entry = new CodexEntry(TipsCategoryId, containers, title);
            entry.parentId = TipsCategoryId;
            // AddEntry lists the page in its parent category when that category is already registered, which the
            // Tips category is by the time GenerateTutorialNotificationEntries returns. Vanilla's tips are added
            // before their category exists, so this only adds it when AddEntry didn't.
            CodexCache.AddEntry(DtuTipId, entry);
            if (!category.entriesInCategory.Contains(entry))
            {
                category.entriesInCategory.Add(entry);
            }
        }

        public static void InsertHeatSection()
        {
            if (CodexCache.entries == null || !CodexCache.entries.TryGetValue(HeatEntryId, out CodexEntry entry))
            {
                Debug.LogWarning("[JoulesAndWatts] No Heat codex entry, so the Thermal Units section wasn't added.");
                return;
            }

            string body = SameUnits
                + string.Format(Gravitas, (string)BUILDINGS.PREFABS.ELECTROLYZER.NAME)
                + string.Format(Dtu, UI.FormatAsLink("DTU (Duplicant Thermal Unit)", DtuTipId));
            var widgets = new List<ICodexWidget>
            {
                new CodexText(Header, CodexTextStyle.Subtitle),
                new CodexText(body, CodexTextStyle.Body)
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
