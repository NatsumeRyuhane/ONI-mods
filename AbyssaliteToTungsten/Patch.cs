using System.Collections.Generic;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using UnityEngine;

namespace AbyssaliteToTungsten
{
    public class AbyssaliteToTungsten : UserMod2
    {
        // Same 1:1 mass ratio as the vanilla recipes.
        private const float INPUT_KG = 100f;

        // With the toggle on, the Metal Refinery sends this many times the vanilla heat into its coolant.
        private const float TRIPLE_COOLANT_HEAT = 3f;

        // Added to the Metal Refinery description while the toggle is on. It uses the game's own wording for Abyssalite,
        // "resilient, crystalline element" (STRINGS.ELEMENTS.KATAIRITE.DESC), and the Heat link from the building's EFFECT text.
        private const string HEAT_LINE = "Refining <link=\"KATAIRITE\">Abyssalite</link>, a resilient, crystalline element, releases a great deal of <link=\"HEAT\">Heat</link>.";

        // Set when the Metal Refinery recipe is created. The coolant patch matches on it.
        private static string metalRefineryRecipeId;

        public static void Log(string msg)
        {
            Debug.Log("[AbyssaliteToTungsten] " + msg);
        }

        public override void OnLoad(Harmony harmony)
        {
            AbyssaliteToTungsten.Log("Loaded");
            base.OnLoad(harmony);
            PUtil.InitLibrary();
            new POptions().RegisterOptions(this, typeof(Config));

            // The Molecular Forge and the Metal Refinery add their own recipes in ConfigureBuildingTemplate, so each recipe
            // goes in a postfix there. Only these configs are patched, so the static constructors of the other configs don't run at load.
            var metalRefinery = AccessTools.Method(typeof(MetalRefineryConfig), nameof(MetalRefineryConfig.ConfigureBuildingTemplate));
            harmony.Patch(metalRefinery, null, new HarmonyMethod(AccessTools.Method(typeof(AbyssaliteToTungsten), nameof(AddMetalRefineryRecipe))));

            var molecularForge = AccessTools.Method(typeof(SupermaterialRefineryConfig), nameof(SupermaterialRefineryConfig.ConfigureBuildingTemplate));
            harmony.Patch(molecularForge, null, new HarmonyMethod(AccessTools.Method(typeof(AbyssaliteToTungsten), nameof(AddMolecularForgeRecipe))));

            // The Metal Refinery's coolant heat is worked out inside SpawnOrderProduct. The prefix adds the extra heat first.
            var spawnOrderProduct = AccessTools.Method(typeof(LiquidCooledRefinery), "SpawnOrderProduct");
            harmony.Patch(spawnOrderProduct, new HarmonyMethod(AccessTools.Method(typeof(AbyssaliteToTungsten), nameof(TripleCoolantHeat))));

            // BuildingDef.Desc reads the strings on each call, so the heat line is added in a postfix on that getter.
            var description = AccessTools.PropertyGetter(typeof(BuildingDef), nameof(BuildingDef.Desc));
            harmony.Patch(description, null, new HarmonyMethod(AccessTools.Method(typeof(AbyssaliteToTungsten), nameof(AppendHeatLine))));

            AbyssaliteToTungsten.Log("Complete");
        }

        // Fast, but the Metal Refinery cools its molten output through the coolant module, so this recipe needs coolant.
        // The coolant module already charges the cooling from the element's high temperature (3695 K for Tungsten), so
        // the recipe needs no extra heat code.
        public static void AddMetalRefineryRecipe()
        {
            metalRefineryRecipeId = AddRecipe("MetalRefinery", 40f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature);
        }

        // Slower, but the Molecular Forge has no coolant step and doesn't need the molten heat.
        public static void AddMolecularForgeRecipe()
        {
            // The Molecular Forge heats its outputs, the same as its own recipes.
            AddRecipe("SupermaterialRefinery", 80f, ComplexRecipe.RecipeElement.TemperatureOperation.Heated);
        }

        private static string AddRecipe(string fabricatorId, float time, ComplexRecipe.RecipeElement.TemperatureOperation outputTemperature)
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
            string id = ComplexRecipeManager.MakeRecipeID(fabricatorId, inputs, outputs);
            new ComplexRecipe(id, inputs, outputs)
            {
                time = time,
                nameDisplay = ComplexRecipe.RecipeNameDisplay.IngredientToResult,
                description = "Refines Abyssalite into Tungsten.",
                fabricators = new List<Tag> { TagManager.Create(fabricatorId) }
            };
            return id;
        }

        // Runs before the vanilla SpawnOrderProduct, while the coolant is still in buildStorage. The vanilla code then adds its
        // own heat on top, so the coolant ends up with three times the vanilla heat.
        public static void TripleCoolantHeat(LiquidCooledRefinery __instance, ComplexRecipe recipe)
        {
            if (recipe.id != metalRefineryRecipeId || !SingletonOptions<Config>.Instance.BalanceHeatProduction) return;

            // The heat the vanilla code sends to the coolant: thermalFudge x (high temperature - output temperature) x mass x heat capacity.
            var tungsten = ElementLoader.FindElementByHash(SimHashes.Tungsten);
            float vanillaKilowatts = __instance.thermalFudge * (tungsten.highTemp - __instance.outputTemperature) * recipe.results[0].amount * tungsten.specificHeatCapacity;

            var coolant = new List<GameObject>();
            __instance.buildStorage.Find(__instance.coolantTag, coolant);
            float heatCapacity = 0f;
            foreach (GameObject item in coolant)
            {
                PrimaryElement element = item.GetComponent<PrimaryElement>();
                if (element.Mass != 0f) heatCapacity += element.Mass * element.Element.specificHeatCapacity;
            }
            if (heatCapacity <= 0f) return;

            // The vanilla code splits its heat across the coolant by heat capacity, so every element rises by the same amount.
            float extraTemperature = (TRIPLE_COOLANT_HEAT - 1f) * vanillaKilowatts / heatCapacity;
            foreach (GameObject item in coolant)
            {
                PrimaryElement element = item.GetComponent<PrimaryElement>();
                if (element.Mass != 0f) element.Temperature += extraTemperature;
            }
        }

        // Adds the heat line to the Metal Refinery's description, only while the toggle is on.
        public static void AppendHeatLine(BuildingDef __instance, ref string __result)
        {
            if (__instance.PrefabID != MetalRefineryConfig.ID || !SingletonOptions<Config>.Instance.BalanceHeatProduction) return;
            __result += "\n\n" + HEAT_LINE;
        }
    }
}
