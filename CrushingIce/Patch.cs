using System.Collections.Generic;
using HarmonyLib;

namespace CrushingIce
{
    public static class Patch
    {
        // Same 100 kg batch and 40 s time as the vanilla Rock Crusher recipes.
        private const float INPUT_KG = 100f;
        private const float RECIPE_TIME = 40f;

        // Byproduct shares, taken from the element data for boiling the melted liquid: Polluted Water leaves 1% Dirt,
        // and Brine, a 30% solution, leaves 30% Salt (the Desalinator uses the same split).
        private const float DIRT_SHARE = 0.01f;
        private const float SALT_SHARE = 0.3f;

        // The Rock Crusher adds its own recipes in ConfigureBuildingTemplate, so the ice recipes go in a postfix there.
        [HarmonyPatch(typeof(RockCrusherConfig), nameof(RockCrusherConfig.ConfigureBuildingTemplate))]
        public static class RockCrusherConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix()
            {
                AddRecipe(SimHashes.Ice, 0f, SimHashes.Void);
                AddRecipe(SimHashes.DirtyIce, DIRT_SHARE, SimHashes.Dirt);
                AddRecipe(SimHashes.BrineIce, SALT_SHARE, SimHashes.Salt);
            }
        }

        // Crushes 100 kg of the ice into Snow, plus the byproduct share when there is one. The outputs keep the
        // input's temperature like the vanilla recipes. Snow melts at the same temperature as Ice, and above the other ices.
        private static void AddRecipe(SimHashes ice, float byproductShare, SimHashes byproduct)
        {
            var inputs = new ComplexRecipe.RecipeElement[]
            {
                new ComplexRecipe.RecipeElement(ice.CreateTag(), INPUT_KG)
            };
            var outputs = new List<ComplexRecipe.RecipeElement>
            {
                new ComplexRecipe.RecipeElement(SimHashes.Snow.CreateTag(), INPUT_KG * (1f - byproductShare), ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature)
            };
            if (byproductShare > 0f)
                outputs.Add(new ComplexRecipe.RecipeElement(byproduct.CreateTag(), INPUT_KG * byproductShare, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature));

            // Uses the vanilla "Crushes {0} into {1}" strings, so the description follows the game's language.
            string description = outputs.Count == 1
                ? string.Format(STRINGS.BUILDINGS.PREFABS.ROCKCRUSHER.RECIPE_DESCRIPTION, ElementName(ice), ElementName(SimHashes.Snow))
                : string.Format(STRINGS.BUILDINGS.PREFABS.ROCKCRUSHER.RECIPE_DESCRIPTION_TWO_OUTPUT, ElementName(ice), ElementName(SimHashes.Snow), ElementName(byproduct));

            // The ComplexRecipe constructor registers the recipe with ComplexRecipeManager.
            var results = outputs.ToArray();
            string id = ComplexRecipeManager.MakeRecipeID(RockCrusherConfig.ID, inputs, results);
            new ComplexRecipe(id, inputs, results)
            {
                time = RECIPE_TIME,
                nameDisplay = ComplexRecipe.RecipeNameDisplay.IngredientToResult,
                description = description,
                fabricators = new List<Tag> { TagManager.Create(RockCrusherConfig.ID) }
            };
        }

        private static string ElementName(SimHashes element)
        {
            return ElementLoader.FindElementByHash(element).name;
        }
    }
}
