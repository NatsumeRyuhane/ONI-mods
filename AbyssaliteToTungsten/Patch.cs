using System.Collections.Generic;
using HarmonyLib;
using KMod;
using UnityEngine;

namespace AbyssaliteToTungsten
{
    public class AbyssaliteToTungsten : UserMod2
    {
        // Same 1:1 mass ratio as the vanilla metal recipes.
        private const float INPUT_KG = 100f;

        public static void Log(string msg)
        {
            Debug.Log("[AbyssaliteToTungsten] " + msg);
        }

        public override void OnLoad(Harmony harmony)
        {
            AbyssaliteToTungsten.Log("Loaded");

            // Each building adds its own recipes in ConfigureBuildingTemplate, so the new recipe goes in a postfix there.
            // Only these two configs are patched, so the static constructors of the other configs don't run at load.
            var metalRefinery = AccessTools.Method(typeof(MetalRefineryConfig), nameof(MetalRefineryConfig.ConfigureBuildingTemplate));
            harmony.Patch(metalRefinery, null, new HarmonyMethod(AccessTools.Method(typeof(AbyssaliteToTungsten), nameof(AddMetalRefineryRecipe))));

            var molecularForge = AccessTools.Method(typeof(SupermaterialRefineryConfig), nameof(SupermaterialRefineryConfig.ConfigureBuildingTemplate));
            harmony.Patch(molecularForge, null, new HarmonyMethod(AccessTools.Method(typeof(AbyssaliteToTungsten), nameof(AddMolecularForgeRecipe))));

            AbyssaliteToTungsten.Log("Complete");
        }

        public static void AddMetalRefineryRecipe()
        {
            AddRecipe("MetalRefinery", 40f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature);
        }

        public static void AddMolecularForgeRecipe()
        {
            // The Molecular Forge heats its outputs, the same as its own recipes.
            AddRecipe("SupermaterialRefinery", 80f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated);
        }

        private static void AddRecipe(string fabricatorId, float time, ComplexRecipe.RecipeElement.TemperatureOperation outputTemperature)
        {
            var inputs = new ComplexRecipe.RecipeElement[]
            {
                new ComplexRecipe.RecipeElement(SimHashes.Katairite.CreateTag(), INPUT_KG)
            };
            var outputs = new ComplexRecipe.RecipeElement[]
            {
                new ComplexRecipe.RecipeElement(SimHashes.Tungsten.CreateTag(), INPUT_KG, outputTemperature)
            };
            // The ComplexRecipe constructor registers the recipe with ComplexRecipeManager.
            new ComplexRecipe(ComplexRecipeManager.MakeRecipeID(fabricatorId, inputs, outputs), inputs, outputs)
            {
                time = time,
                nameDisplay = ComplexRecipe.RecipeNameDisplay.IngredientToResult,
                description = "Refines Abyssalite into Tungsten.",
                fabricators = new List<Tag> { TagManager.Create(fabricatorId) }
            };
        }
    }
}
