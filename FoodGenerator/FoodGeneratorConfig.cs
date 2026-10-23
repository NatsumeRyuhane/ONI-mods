using System.Collections.Generic;
using PeterHan.PLib.Options;
using TUNING;
using UnityEngine;

namespace FoodGenerator
{
    // A generator with the Coal Generator's art, size and stats that burns food to order. Each food is a recipe in the
    // usual fabricator queue (FoodBurner), and each order burns one unit of it. FoodGenerator burns the order in
    // progress. A food's calories convert exactly, at 4,184 J per kcal, into the power it makes plus the heat it
    // gives off.
    public class FoodGeneratorConfig : IBuildingConfig
    {
        public const string ID = "FoodGenerator";

        public const float JoulesPerKcal = 4184f;

        // Where the Coal Generator lets out its Carbon Dioxide.
        public static readonly CellOffset ExhaustOffset = new CellOffset(1, 2);

        private static bool recipesCreated;

        public static Config Options
        {
            get { return SingletonOptions<Config>.Instance; }
        }

        public static float HeatWatts
        {
            get { return (Options.ExhaustHeatKw + Options.SelfHeatKw) * 1000f; }
        }

        // Food energy spent per second at the base power, without a tune-up.
        public static float BurnWatts
        {
            get { return Options.PowerWatts + HeatWatts; }
        }

        // The energy in an order: its unit of food's calories, at 4,184 J per kcal.
        public static float OrderJoules(ComplexRecipe order)
        {
            ComplexRecipe.RecipeElement food = order.ingredients[0];
            EdiblesManager.FoodInfo info = EdiblesManager.GetFoodInfo(food.material.Name);
            // CaloriesPerUnit is in calories, 1,000 to the kcal.
            return info != null ? info.CaloriesPerUnit / 1000f * food.amount * JoulesPerKcal : 0f;
        }

        public override BuildingDef CreateBuildingDef()
        {
            BuildingDef def = BuildingTemplates.CreateBuildingDef(ID, 3, 3, "generatorphos_kanim", 100, 120f,
                BUILDINGS.CONSTRUCTION_MASS_KG.TIER5, MATERIALS.ALL_METALS, 2400f, BuildLocationRule.OnFloor,
                noise: NOISE_POLLUTION.NOISY.TIER5, decor: BUILDINGS.DECOR.PENALTY.TIER2);
            def.GeneratorWattageRating = Options.PowerWatts;
            def.GeneratorBaseCapacity = def.GeneratorWattageRating;
            def.ExhaustKilowattsWhenActive = Options.ExhaustHeatKw;
            def.SelfHeatKilowattsWhenActive = Options.SelfHeatKw;
            def.RequiresPowerOutput = true;
            def.PowerOutputOffset = new CellOffset(0, 0);
            def.ViewMode = OverlayModes.Power.ID;
            def.AudioCategory = "HollowMetal";
            def.AudioSize = "large";
            def.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(0, 0));
            def.AddSearchTerms(STRINGS.SEARCH_TERMS.POWER);
            def.AddSearchTerms(STRINGS.SEARCH_TERMS.GENERATOR);
            def.AddSearchTerms(STRINGS.SEARCH_TERMS.FOOD);
            return def;
        }

        // Set up like the Kiln, a fabricator no Duplicant works, with the Coal Generator's tags. Its fetches use the
        // Coal Generator's errand type, Power Supply.
        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            KPrefabID prefabId = go.GetComponent<KPrefabID>();
            prefabId.AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
            prefabId.AddTag(RoomConstraints.ConstraintTags.PowerBuilding);
            prefabId.AddTag(RoomConstraints.ConstraintTags.GeneratorType);
            prefabId.AddTag(RoomConstraints.ConstraintTags.HeavyDutyGeneratorType);
            go.AddOrGet<DropAllWorkable>();
            go.AddOrGet<BuildingComplete>().isManuallyOperated = false;
            FoodBurner burner = go.AddOrGet<FoodBurner>();
            burner.duplicantOperated = false;
            burner.showProgressBar = true;
            burner.sideScreenStyle = ComplexFabricatorSideScreen.StyleSetting.ListQueueHybrid;
            burner.fetchChoreTypeIdHash = Db.Get().ChoreTypes.PowerFetch.IdHash;
            // The fabricator's state machine plays the building's animations. Idle, the generator shows "on", like
            // the Coal Generator.
            go.AddOrGet<ComplexFabricatorSM>().idleAnimationName = "on";
            go.AddOrGet<FabricatorIngredientStatusManager>();
            go.AddOrGet<CopyBuildingSettings>();
            BuildingTemplates.CreateComplexFabricatorStorage(go, burner);
            go.AddOrGet<FoodGenerator>().powerDistributionOrder = 9;
            go.AddOrGet<LoopingSounds>();
            Prioritizable.AddRef(go);
            Tinkerable.MakePowerTinkerable(go);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<LogicOperationalController>();
        }

        // One recipe per food. Food prefabs are registered after the buildings, so Patch runs this just before the
        // game processes its recipes, when every food, modded ones included, exists. Each order burns one unit of
        // the food, and its recipe lists the gases it gives off (see FoodComposition) as results, which
        // FoodGenerator lets out while it burns. The results are never spawned: a Melted result that isn't stored
        // spawns nothing.
        public static void CreateRecipes()
        {
            if (recipesCreated)
            {
                return;
            }
            recipesCreated = true;
            HashSet<string> seen = new HashSet<string>();
            foreach (EdiblesManager.FoodInfo food in EdiblesManager.GetAllLoadedFoodTypes())
            {
                if (food == null || food.CaloriesPerUnit <= 0f || !seen.Add(food.Id))
                {
                    continue;
                }
                GameObject prefab = Assets.TryGetPrefab(food.Id);
                PrimaryElement element = prefab != null ? prefab.GetComponent<PrimaryElement>() : null;
                if (element == null || prefab.GetComponent<Edible>() == null)
                {
                    continue;
                }
                // CaloriesPerUnit is in calories, 1,000 to the kcal. Every vanilla food weighs 1 kg a unit.
                float kcal = food.CaloriesPerUnit / 1000f;
                float massKg = element.MassPerUnit;
                float joules = kcal * JoulesPerKcal;
                float seconds = Mathf.Round(joules / BurnWatts);
                FoodComposition composition = FoodComposition.For(food.Id);
                List<ComplexRecipe.RecipeElement> results = new List<ComplexRecipe.RecipeElement>
                {
                    new ComplexRecipe.RecipeElement(SimHashes.CarbonDioxide.CreateTag(), composition.Co2Kg(kcal),
                        ComplexRecipe.RecipeElement.TemperatureOperation.Melted)
                };
                float steamKg = composition.SteamKg(kcal, massKg);
                if (steamKg > 0f)
                {
                    results.Add(new ComplexRecipe.RecipeElement(SimHashes.Steam.CreateTag(), steamKg,
                        ComplexRecipe.RecipeElement.TemperatureOperation.Melted));
                }
                ComplexRecipe.RecipeElement[] ingredients =
                {
                    new ComplexRecipe.RecipeElement(food.Id.ToTag(), 1f)
                };
                ComplexRecipe.RecipeElement[] resultArray = results.ToArray();
                ComplexRecipe recipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID(ID, ingredients, resultArray),
                    ingredients, resultArray)
                {
                    time = seconds,
                    description = string.Format(Strings.Get(Patch.RecipeDescKey), prefab.GetProperName(),
                        GameUtil.GetFormattedCalories(food.CaloriesPerUnit), GameUtil.GetFormattedMass(massKg),
                        GameUtil.GetFormattedJoules(joules), GameUtil.GetFormattedTime(seconds)),
                    nameDisplay = ComplexRecipe.RecipeNameDisplay.Ingredient,
                    fabricators = new List<Tag> { ID },
                    // The worst food first, since it's the one to burn.
                    sortOrder = food.Quality
                };
                recipe.SetDLCRestrictions(food.GetRequiredDlcIds(), food.GetForbiddenDlcIds());
            }
        }
    }
}
