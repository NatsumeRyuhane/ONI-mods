using System.Collections.Generic;

namespace FoodGenerator
{
    // What a food is made of, as the share of its calories from carbohydrate, protein and fat, eyeballed from each
    // vanilla food's description. The exhaust follows from burning it:
    // - Dry matter: carbohydrate and protein give 4 kcal per gram, fat 9 (the Atwater factors). The food's game
    //   calories set how many grams of each it holds.
    // - Carbon Dioxide and water: per gram burned, carbohydrate (as starch, C6H10O5) gives 1.63 g of CO2 and 0.56 g
    //   of water, protein (about 53% carbon, 7% hydrogen) 1.94 g and 0.63 g, and fat (as tripalmitin) 2.78 g and
    //   1.09 g. The nitrogen in protein is left out.
    // - Moisture: whatever a unit weighs beyond its dry matter is water. It leaves as Steam with the water from
    //   burning.
    public struct FoodComposition
    {
        private const float CarbKcalPerGram = 4f;
        private const float ProteinKcalPerGram = 4f;
        private const float FatKcalPerGram = 9f;

        private const float CarbCo2PerGram = 1.63f;
        private const float ProteinCo2PerGram = 1.94f;
        private const float FatCo2PerGram = 2.78f;

        private const float CarbWaterPerGram = 0.56f;
        private const float ProteinWaterPerGram = 0.63f;
        private const float FatWaterPerGram = 1.09f;

        // Foods without an entry, such as other mods' foods, get a mixed diet.
        public static readonly FoodComposition Default = new FoodComposition(50, 20, 30);

        // Percent of calories from carbohydrate, protein and fat.
        private static readonly Dictionary<string, FoodComposition> Vanilla = new Dictionary<string, FoodComposition>
        {
            // Raw plants and fungi
            { "BasicPlantFood", new FoodComposition(80, 12, 8) },          // Meal Lice: a flavorless grain
            { "BasicForagePlant", new FoodComposition(90, 5, 5) },         // Muckroot: a bland fruit
            { "ForestForagePlant", new FoodComposition(50, 5, 45) },       // Hexalent Fruit: rubbery, calorie dense
            { "SwampForagePlant", new FoodComposition(80, 12, 8) },        // Swamp Chard Heart: squishy, juicy center
            { "IceCavesForagePlant", new FoodComposition(90, 5, 5) },      // Sherberry: a cold fruit
            { "GardenForagePlant", new FoodComposition(90, 5, 5) },        // Snac Fruit: a fruit
            { "GardenFoodPlantFood", new FoodComposition(85, 10, 5) },     // Sweatcorn: a sugary vegetable
            { "HardSkinBerry", new FoodComposition(90, 5, 5) },            // Pikeapple: a fruit in a husk
            { "PrickleFruit", new FoodComposition(90, 5, 5) },             // Bristle Berry: a sweet fruit
            { "SwampFruit", new FoodComposition(90, 5, 5) },               // Bog Jelly: gelatinous fruit cubes
            { "VineFruit", new FoodComposition(90, 5, 5) },                // Ovagro Fig: a fruit
            { "WormBasicFruit", new FoodComposition(90, 5, 5) },           // Spindly Grubfruit: vaguely tasty
            { "WormSuperFruit", new FoodComposition(95, 3, 2) },           // Grubfruit: honey-like taste
            { "Carrot", new FoodComposition(90, 7, 3) },                   // Plume Squash: a tuber
            { "Lettuce", new FoodComposition(75, 20, 5) },                 // Lettuce: salty leaves
            { "Mushroom", new FoodComposition(55, 35, 10) },               // Dusk Cap: a fungus
            { "PlantMeat", new FoodComposition(30, 50, 20) },              // Plant Meat: planty plant meat
            { "SaltySticksFood", new FoodComposition(90, 5, 5) },          // Salty Sticks: a salty cane snack
            // Raw meat, fish and eggs
            { "Meat", new FoodComposition(0, 40, 60) },                    // Meat
            { "FishMeat", new FoodComposition(0, 70, 30) },                // Fish Fillet
            { "PrehistoricPacuFillet", new FoodComposition(0, 60, 40) },   // Jawbo Fillet: silky
            { "ShellfishMeat", new FoodComposition(5, 80, 15) },           // Raw Shellfish
            { "SquidMeat", new FoodComposition(10, 75, 15) },              // Calamari: slippery
            { "UrchinMeat", new FoodComposition(10, 30, 60) },             // Uni: a rich delicacy
            { "MusselTongue", new FoodComposition(10, 70, 20) },           // Mussel Tongue: bitter and chewy
            { "RawEgg", new FoodComposition(2, 35, 63) },                  // Raw Egg
            // Processed foods
            { "FieldRation", new FoodComposition(55, 20, 25) },            // Nutrient Bar: paste between wafers
            { "MushBar", new FoodComposition(80, 15, 5) },                 // Mush Bar: putrefied mudslop
            { "FriedMushBar", new FoodComposition(65, 10, 25) },           // Mush Fry: pan-fried mudslop
            { "GammaMush", new FoodComposition(80, 15, 5) },               // Gamma Mush: irradiated dirt and water
            { "PickledMeal", new FoodComposition(85, 10, 5) },             // Pickled Meal: Meal Lice in vinegar
            { "BasicPlantBar", new FoodComposition(80, 12, 8) },           // Liceloaf: compacted Meal Lice
            { "ColdWheatBread", new FoodComposition(75, 13, 12) },         // Frost Bun: a simple bun
            { "SpiceBread", new FoodComposition(75, 12, 13) },             // Pepper Bread: a spiced loaf
            { "Pancakes", new FoodComposition(55, 12, 33) },               // Souffle Pancakes: egg and grain
            { "ButterflyFood", new FoodComposition(50, 15, 35) },          // Toasted Mimillet: toasted seeds
            { "WormBasicFood", new FoodComposition(20, 15, 65) },          // Roast Grubfruit Nut: roasted nuts
            { "WormSuperFood", new FoodComposition(98, 1, 1) },            // Grubfruit Preserve: jam in Sucrose
            { "FruitCake", new FoodComposition(95, 2, 3) },                // Berry Sludge: aggressively sweet
            { "GrilledPrickleFruit", new FoodComposition(85, 5, 10) },     // Gristle Berry: a grilled berry
            { "CookedPikeapple", new FoodComposition(90, 5, 5) },          // Pikeapple Skewer: a grilled fruit
            { "Salsa", new FoodComposition(80, 5, 15) },                   // Stuffed Berry: a baked berry
            { "BerryPie", new FoodComposition(65, 5, 30) },                // Mixed Berry Pie: berries in a crust
            { "SwampDelights", new FoodComposition(85, 10, 5) },           // Swampy Delights: dried jelly cubes
            { "SmokedVegetables", new FoodComposition(65, 15, 20) },       // Veggie Poppers: stuffed vegetables
            { "FriedMushroom", new FoodComposition(30, 20, 50) },          // Fried Mushroom: pan-fried
            { "MushroomWrap", new FoodComposition(45, 20, 35) },           // Mushroom Wrap: mushroom in lettuce
            { "FriesCarrot", new FoodComposition(50, 5, 45) },             // Squash Fries: fried
            { "Edamame", new FoodComposition(30, 35, 35) },                // Edamame: salted beans
            { "DeepFriedNosh", new FoodComposition(40, 20, 40) },          // Nosh Noms: deep-fried beans
            { "Curry", new FoodComposition(50, 20, 30) },                  // Curried Beans: beans and ginger
            { "Tofu", new FoodComposition(10, 40, 50) },                   // Tofu: bean curd
            { "SpicyTofu", new FoodComposition(15, 35, 50) },              // Spicy Tofu: marinated tofu
            { "CookedEgg", new FoodComposition(3, 30, 67) },               // Omelette
            { "Quiche", new FoodComposition(25, 20, 55) },                 // Mushroom Quiche: omelette on a crust
            { "CookedMeat", new FoodComposition(0, 40, 60) },              // Barbeque: cooked meat
            { "DeepFriedMeat", new FoodComposition(10, 35, 55) },          // Deep Fried Steak
            { "SmokedDinosaurMeat", new FoodComposition(5, 40, 55) },      // Tender Brisket: marinated, smoked
            { "Pemmican", new FoodComposition(0, 25, 75) },                // Pemmican: meat and tallow
            { "Burger", new FoodComposition(30, 25, 45) },                 // Frost Burger: meat, lettuce, bun
            { "SurfAndTurf", new FoodComposition(0, 45, 55) },             // Surf'n'Turf: meat and seafood
            { "CookedFish", new FoodComposition(0, 65, 35) },              // Cooked Seafood
            { "SmokedFish", new FoodComposition(0, 50, 50) },              // Smoked Fish: buttery
            { "DeepFriedFish", new FoodComposition(25, 35, 40) },          // Fish Taco: fish in a fried fin
            { "DeepFriedShellfish", new FoodComposition(30, 35, 35) },     // Shellfish Tempura
            { "Maki", new FoodComposition(55, 25, 20) },                   // Sushi Roll: fish, Liceloaf, Nori
            { "Nigiri", new FoodComposition(55, 30, 15) },                 // Nigiri: Calamari and Liceloaf
        };

        // Shares of the calories, summing to 1.
        public readonly float Carb;
        public readonly float Protein;
        public readonly float Fat;

        public FoodComposition(float carbPercent, float proteinPercent, float fatPercent)
        {
            float total = carbPercent + proteinPercent + fatPercent;
            Carb = carbPercent / total;
            Protein = proteinPercent / total;
            Fat = fatPercent / total;
        }

        public static FoodComposition For(string foodId)
        {
            FoodComposition composition;
            return Vanilla.TryGetValue(foodId, out composition) ? composition : Default;
        }

        // Grams of carbohydrate, protein and fat that give these kcal.
        private void Grams(float kcal, out float carb, out float protein, out float fat)
        {
            carb = kcal * Carb / CarbKcalPerGram;
            protein = kcal * Protein / ProteinKcalPerGram;
            fat = kcal * Fat / FatKcalPerGram;
        }

        public float DryMassKg(float kcal)
        {
            float carb, protein, fat;
            Grams(kcal, out carb, out protein, out fat);
            return (carb + protein + fat) / 1000f;
        }

        public float Co2Kg(float kcal)
        {
            float carb, protein, fat;
            Grams(kcal, out carb, out protein, out fat);
            return (carb * CarbCo2PerGram + protein * ProteinCo2PerGram + fat * FatCo2PerGram) / 1000f;
        }

        // Water from burning the dry matter, plus the moisture in a food of this mass.
        public float SteamKg(float kcal, float massKg)
        {
            float carb, protein, fat;
            Grams(kcal, out carb, out protein, out fat);
            float burnWaterKg = (carb * CarbWaterPerGram + protein * ProteinWaterPerGram + fat * FatWaterPerGram)
                / 1000f;
            return burnWaterKg + System.Math.Max(0f, massKg - DryMassKg(kcal));
        }
    }
}
