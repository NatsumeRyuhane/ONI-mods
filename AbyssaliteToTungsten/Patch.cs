using System.Collections.Generic;
using HarmonyLib;
using KMod;
using UnityEngine;

namespace AbyssaliteToTungsten
{
    public class AbyssaliteToTungsten : UserMod2
    {
        // Same 1:1 mass ratio as the Molecular Forge's own recipes.
        private const float INPUT_KG = 100f;

        public static void Log(string msg)
        {
            Debug.Log("[AbyssaliteToTungsten] " + msg);
        }

        public override void OnLoad(Harmony harmony)
        {
            AbyssaliteToTungsten.Log("Loaded");

            // The Molecular Forge adds its own recipes in ConfigureBuildingTemplate, so the new recipe goes in a postfix there.
            // Only this config is patched, so the static constructors of the other configs don't run at load.
            // The Metal Refinery is left alone. It already turns Wolframite into Tungsten, and Abyssalite would undercut that.
            var molecularForge = AccessTools.Method(typeof(SupermaterialRefineryConfig), nameof(SupermaterialRefineryConfig.ConfigureBuildingTemplate));
            harmony.Patch(molecularForge, null, new HarmonyMethod(AccessTools.Method(typeof(AbyssaliteToTungsten), nameof(AddMolecularForgeRecipe))));

            AbyssaliteToTungsten.Log("Complete");
        }

        public static void AddMolecularForgeRecipe()
        {
            var inputs = new ComplexRecipe.RecipeElement[]
            {
                new ComplexRecipe.RecipeElement(SimHashes.Katairite.CreateTag(), INPUT_KG)
            };
            // The Molecular Forge heats its outputs, the same as its own recipes.
            var outputs = new ComplexRecipe.RecipeElement[]
            {
                new ComplexRecipe.RecipeElement(SimHashes.Tungsten.CreateTag(), INPUT_KG, ComplexRecipe.RecipeElement.TemperatureOperation.Heated)
            };
            // The ComplexRecipe constructor registers the recipe with ComplexRecipeManager.
            new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("SupermaterialRefinery", inputs, outputs), inputs, outputs)
            {
                time = 80f,
                nameDisplay = ComplexRecipe.RecipeNameDisplay.IngredientToResult,
                description = "Refines Abyssalite into Tungsten.",
                fabricators = new List<Tag> { TagManager.Create("SupermaterialRefinery") }
            };
        }
    }
}
