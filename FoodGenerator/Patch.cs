using HarmonyLib;
using STRINGS;

namespace FoodGenerator
{
    public class Patch
    {
        public const string RecipeDescKey = "STRINGS.BUILDINGS.PREFABS.FOODGENERATOR.RECIPE_DESC";

        // The fuel the Fuel Request Threshold tooltip names.
        public const string FoodNameKey = "STRINGS.BUILDINGS.PREFABS.FOODGENERATOR.FUEL_NAME";

        // Db.Initialize runs before the game registers buildings (LegacyModMain.Load) and before Techs.PostProcess,
        // so strings, the build menu entry and the research unlock are all in place in time.
        [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
        public static class Db_Initialize_Patch
        {
            public static void Postfix()
            {
                string key = "STRINGS.BUILDINGS.PREFABS." + FoodGeneratorConfig.ID.ToUpperInvariant();
                string food = UI.FormatAsLink("Food", "FOOD");
                Strings.Add(key + ".NAME", UI.FormatAsLink("Food Generator", FoodGeneratorConfig.ID));
                // Worded like the vanilla generators: what it burns, then what it gives off.
                Strings.Add(key + ".DESC", "Every Duplicant runs on calories. So, it turns out, does everything else.");
                Strings.Add(key + ".EFFECT", "Burns " + food + " to produce electrical "
                    + UI.FormatAsLink("Power", "POWER") + ".\n\nProduces "
                    + UI.FormatAsLink("Carbon Dioxide", "CARBONDIOXIDE") + ", " + UI.FormatAsLink("Steam", "STEAM")
                    + " and " + UI.FormatAsLink("Heat", "HEAT") + ".");
                Strings.Add(RecipeDescKey, "Burns one unit of {0} ({1}, {2}) for {3} of energy, which lasts {4}.");
                Strings.Add(FoodGenerator.FoodDescriptorKey, food + ": {0}");
                Strings.Add(FoodGenerator.FoodDescriptorTooltipKey, "Burns {0} of the ordered " + food
                    + " while running, enough for its power and heat at 4,184 J per kcal.");
                Strings.Add(FoodNameKey, "Food");
                string status = "STRINGS.BUILDING.STATUSITEMS." + FoodBurner.BurningStatusId.ToUpperInvariant();
                Strings.Add(status + ".NAME", "Burning {Item}");
                Strings.Add(status + ".TOOLTIP", "This generator is burning {Item} for power.");
                ModUtil.AddBuildingToPlanScreen("Power", FoodGeneratorConfig.ID, "generators", GeneratorConfig.ID);
                Db.Get().Techs.TryGet("Combustion")?.unlockedItemIDs.Add(FoodGeneratorConfig.ID);
            }
        }

        // ComplexRecipeManager.PostProcess runs once at startup, after every building and entity is registered, and
        // turns the recipes made so far into the game's recipe list.
        [HarmonyPatch(typeof(ComplexRecipeManager), nameof(ComplexRecipeManager.PostProcess))]
        public static class ComplexRecipeManager_PostProcess_Patch
        {
            public static void Prefix()
            {
                FoodGeneratorConfig.CreateRecipes();
            }
        }

        // A fabricator no Duplicant works runs its order on a timer here, and sets the building active whenever it
        // has an order. FoodGenerator runs a Food Generator's order instead, so the order lasts exactly as long as
        // its energy, and sets the building active.
        [HarmonyPatch(typeof(ComplexFabricator), nameof(ComplexFabricator.Sim200ms))]
        public static class ComplexFabricator_Sim200ms_Patch
        {
            public static bool Prefix(ComplexFabricator __instance)
            {
                return !(__instance is FoodBurner);
            }
        }

        // The number of a recipe's orders the fabricator fetches for. It is used only to plan fetches, and a drop to
        // 0 also cancels the fetches under way. While the batteries are full enough, a Food Generator asks for no
        // food, like a Coal Generator above its Fuel Request Threshold.
        [HarmonyPatch(typeof(ComplexFabricator), nameof(ComplexFabricator.GetRecipePrefetchCount))]
        public static class ComplexFabricator_GetRecipePrefetchCount_Patch
        {
            public static void Postfix(ComplexFabricator __instance, ref int __result)
            {
                FoodBurner burner = __instance as FoodBurner;
                if (burner != null && burner.FetchPaused)
                {
                    __result = 0;
                }
            }
        }
    }
}
