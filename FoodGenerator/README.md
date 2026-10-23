# Food Generator

Adds the Food Generator, a new building that burns food to order for power. It uses the Coal Generator's art and size, has its power and heat, and is unlocked by the same research.

Each food is a recipe in the usual fabricator queue, and one order burns exactly one unit of it, so you choose what gets burned and how much. Calories become energy at the real rate, 4,184 J per kcal, and that energy pays for both the power and the heat the generator gives off. The exhaust depends on what each food is made of.

## The building

| | Coal Generator (vanilla) | Food Generator (defaults) |
|---|---|---|
| Size | 3x3 | 3x3 |
| Material | 800 kg of any metal | 800 kg of any metal |
| Fuel | Coal, 1 kg/s | Food to order, about 2.29 kcal/s (1,377 kcal a cycle) |
| Power | 600 W | Option, default 600 W |
| Heat | 8 kW exhaust, 1 kW self | Options, default 8 kW exhaust and 1 kW self |
| Exhaust | Carbon Dioxide, 20 g/s at 383.15 K or hotter | Carbon Dioxide and Steam, from the food (see below), at 383.15 K or hotter |
| Fuel delivery | Power Supply errand, refills below 100 kg | Power Supply errand, fetches the food for the next queued orders |
| Fuel Request Threshold | Slider, default 50%: no coal delivered while every battery is at or above it | Slider, default 50%: no food fetched while every battery is at or above it |
| Running | Burns its coal whatever the batteries hold | Burns the order in progress whatever the batteries hold |

The generator appears in the Power build menu after the Coal Generator and is unlocked by the same research, Combustion. It can be tuned up with a Microchip like the Coal Generator (see [Tune-up](#tune-up)).

### Energy

While running, the generator spends 600 W + 8 kW + 1 kW = 9.6 kW of food energy. At 4,184 J per kcal that's about 2.29 kcal/s, or 1,377 kcal a cycle, a bit more than a Duplicant eats. Only 6.25% of it becomes power, like a real furnace. The rest is the heat the Coal Generator gives off. Setting the heat options to 0 turns all of it into power, about 86 kcal a cycle at 600 W.

One order burns one unit of food, which holds that food's calories per unit: 250 kcal for a Grubfruit, 800 for a Mush Bar, 6,000 for a Surf'n'Turf. At the default power and heat a Mush Bar order (3.35 MJ) lasts about 349 s and a Surf'n'Turf order about 2,615 s. The last tick of an order only uses what is left of it, so the power made adds up to exactly the order's calories.

Every vanilla food weighs 1 kg a unit: every food prefab is made with a mass of 1 kg, and none changes the default `MassPerUnit` of 1. Other mods' foods may differ, and the generator reads each food's own unit mass.

### Tune-up

Like the Coal Generator, the generator takes Engie's Tune-Up: in a Power Plant, a Duplicant with the power tune-up skill installs a Microchip, which gives +50% power for 3 cycles (longer with a better Machinery skill). The food pays for the power and heat the generator actually makes, so what a tune-up costs depends on where the extra power comes from:

| | Untuned | Tuned | Tuned, with [Efficient Tune-Up](../EfficientTuneUp) |
|---|---|---|---|
| Power | 600 W | 900 W | 900 W |
| Heat | 9 kW | 9 kW | 8.7 kW |
| Food energy spent | 9.6 kW | 9.9 kW | 9.6 kW |
| Food burned | about 1,377 kcal a cycle | about 1,420 kcal a cycle | about 1,377 kcal a cycle |
| Share of the food's energy that becomes power | 6.25% | 9.09% | 9.38% |

On its own, a tune-up works as in vanilla: more power, the same heat, and the food pays for the extra. Efficient Tune-Up takes a tune-up's extra power out of the heat instead, for every generator. It tags the buildings it changes, and the Food Generator charges food for the lower heat when it finds the tag. Neither mod needs the other.

### Exhaust

Each vanilla food's make-up is eyeballed from its description, as the share of its calories from carbohydrate, protein and fat: Meal Lice is a grain, Meat is protein and fat, Grubfruit Preserve is jam. The gases follow from burning it:

- Dry matter: carbohydrate and protein give 4 kcal per gram, fat 9 (the Atwater factors). The food's game calories set how many grams of each a unit holds.
- Carbon Dioxide: per gram burned, carbohydrate (as starch) gives 1.63 g, protein (about 53% carbon) 1.94 g and fat (as tripalmitin) 2.78 g.
- Steam: the water from burning, per gram 0.56 g for carbohydrate, 0.63 g for protein and 1.09 g for fat, plus the food's moisture: whatever a unit weighs beyond its dry matter. The nitrogen in protein is left out.

The oxygen for burning isn't taken from the air, like the Coal Generator's, so the gases weigh more than the food. Foods from other mods get a mixed diet, 50% carbohydrate, 20% protein and 30% fat.

Gas is let out at the Coal Generator's exhaust cell, top right, 10 g at a time, in proportion to how much of the order has burned. Per order, at the default power and heat:

| Food | kcal per unit | Read from the description | Carbs / protein / fat, % of kcal | Carbon Dioxide | Steam | Order lasts |
|---|---|---|---|---|---|---|
| Grubfruit | 250 | honey-like taste | 95 / 3 / 2 | 102 g | 973 g | 109 s |
| Ovagro Fig | 325 | a fruit | 90 / 5 / 5 | 132 g | 966 g | 142 s |
| Lettuce | 400 | salty leaves | 75 / 20 / 5 | 167 g | 960 g | 174 s |
| Meal Lice | 600 | a flavorless grain | 80 / 12 / 8 | 245 g | 941 g | 262 s |
| Salty Sticks | 600 | a salty cane snack | 90 / 5 / 5 | 244 g | 938 g | 262 s |
| Calamari | 800 | slippery | 10 / 75 / 15 | 361 g | 937 g | 349 s |
| Muckroot | 800 | a bland fruit | 90 / 5 / 5 | 325 g | 918 g | 349 s |
| Mush Bar | 800 | putrefied mudslop | 80 / 15 / 5 | 331 g | 919 g | 349 s |
| Nutrient Bar | 800 | paste between wafers | 55 / 20 / 25 | 319 g | 939 g | 349 s |
| Pikeapple | 800 | a fruit in a husk | 90 / 5 / 5 | 325 g | 918 g | 349 s |
| Sherberry | 800 | a cold fruit | 90 / 5 / 5 | 325 g | 918 g | 349 s |
| Snac Fruit | 800 | a fruit | 90 / 5 / 5 | 325 g | 918 g | 349 s |
| Spindly Grubfruit | 800 | vaguely tasty | 90 / 5 / 5 | 325 g | 918 g | 349 s |
| Sweatcorn | 800 | a sugary vegetable | 85 / 10 / 5 | 328 g | 918 g | 349 s |
| Fish Fillet | 1,000 | - | 0 / 70 / 30 | 432 g | 938 g | 436 s |
| Jawbo Fillet | 1,000 | silky | 0 / 60 / 40 | 415 g | 948 g | 436 s |
| Raw Shellfish | 1,000 | - | 5 / 80 / 15 | 455 g | 922 g | 436 s |
| Gamma Mush | 1,050 | irradiated dirt and water | 80 / 15 / 5 | 435 g | 894 g | 458 s |
| Mush Fry | 1,050 | pan-fried mudslop | 65 / 10 / 25 | 410 g | 918 g | 458 s |
| Frost Bun | 1,200 | a simple bun | 75 / 13 / 12 | 487 g | 888 g | 523 s |
| Pikeapple Skewer | 1,200 | a grilled fruit | 90 / 5 / 5 | 488 g | 876 g | 523 s |
| Plant Meat | 1,200 | planty plant meat | 30 / 50 / 20 | 512 g | 907 g | 523 s |
| Roast Grubfruit Nut | 1,200 | roasted nuts | 20 / 15 / 65 | 426 g | 965 g | 523 s |
| Edamame | 1,450 | salted beans | 30 / 35 / 35 | 580 g | 910 g | 632 s |
| Toasted Mimillet | 1,500 | toasted seeds | 50 / 15 / 35 | 577 g | 902 g | 654 s |
| Bristle Berry | 1,600 | a sweet fruit | 90 / 5 / 5 | 650 g | 835 g | 697 s |
| Cooked Seafood | 1,600 | - | 0 / 65 / 35 | 677 g | 909 g | 697 s |
| Meat | 1,600 | - | 0 / 40 / 60 | 607 g | 950 g | 697 s |
| Raw Egg | 1,600 | - | 2 / 35 / 63 | 596 g | 955 g | 697 s |
| Liceloaf | 1,700 | compacted Meal Lice | 80 / 12 / 8 | 695 g | 833 g | 741 s |
| Pickled Meal | 1,800 | Meal Lice in vinegar | 85 / 10 / 5 | 739 g | 816 g | 784 s |
| Bog Jelly | 1,840 | gelatinous fruit cubes | 90 / 5 / 5 | 748 g | 810 g | 802 s |
| Gristle Berry | 2,000 | a grilled berry | 85 / 5 / 10 | 803 g | 806 g | 872 s |
| Swampy Delights | 2,240 | dried jelly cubes | 85 / 10 / 5 | 919 g | 771 g | 976 s |
| Dusk Cap | 2,400 | a fungus | 55 / 35 / 10 | 1.02 kg | 780 g | 1,046 s |
| Grubfruit Preserve | 2,400 | jam in Sucrose | 98 / 1 / 1 | 977 g | 739 g | 1,046 s |
| Swamp Chard Heart | 2,400 | squishy, juicy center | 80 / 12 / 8 | 981 g | 764 g | 1,046 s |
| Pemmican | 2,600 | meat and tallow | 0 / 25 / 75 | 918 g | 959 g | 1,133 s |
| Fried Mushroom | 2,800 | pan-fried | 30 / 20 / 50 | 1.05 kg | 870 g | 1,220 s |
| Mussel Tongue | 2,800 | bitter and chewy | 10 / 70 / 20 | 1.24 kg | 794 g | 1,220 s |
| Omelette | 2,800 | - | 3 / 30 / 67 | 1.02 kg | 932 g | 1,220 s |
| Smoked Fish | 2,800 | buttery | 0 / 50 / 50 | 1.11 kg | 885 g | 1,220 s |
| Veggie Poppers | 2,862.5 | stuffed vegetables | 65 / 15 / 20 | 1.14 kg | 761 g | 1,248 s |
| Nigiri | 3,600 | Calamari and Liceloaf | 55 / 30 / 15 | 1.50 kg | 688 g | 1,569 s |
| Souffle Pancakes | 3,600 | egg and grain | 55 / 12 / 33 | 1.38 kg | 754 g | 1,569 s |
| Sushi Roll | 3,600 | fish, Liceloaf, Nori | 55 / 25 / 20 | 1.47 kg | 706 g | 1,569 s |
| Tofu | 3,600 | bean curd | 10 / 40 / 50 | 1.40 kg | 845 g | 1,569 s |
| Barbeque | 4,000 | cooked meat | 0 / 40 / 60 | 1.52 kg | 876 g | 1,743 s |
| Berry Sludge | 4,000 | aggressively sweet | 95 / 2 / 3 | 1.62 kg | 576 g | 1,743 s |
| Deep Fried Steak | 4,000 | - | 10 / 35 / 55 | 1.52 kg | 848 g | 1,743 s |
| Pepper Bread | 4,000 | a spiced loaf | 75 / 12 / 13 | 1.62 kg | 631 g | 1,743 s |
| Plume Squash | 4,000 | a tuber | 90 / 7 / 3 | 1.64 kg | 579 g | 1,743 s |
| Spicy Tofu | 4,000 | marinated tofu | 15 / 35 / 50 | 1.54 kg | 825 g | 1,743 s |
| Fish Taco | 4,200 | fish in a fried fin | 25 / 35 / 40 | 1.66 kg | 765 g | 1,830 s |
| Mixed Berry Pie | 4,200 | berries in a crust | 65 / 5 / 30 | 1.60 kg | 693 g | 1,830 s |
| Shellfish Tempura | 4,200 | - | 30 / 35 / 35 | 1.68 kg | 740 g | 1,830 s |
| Stuffed Berry | 4,400 | a baked berry | 80 / 5 / 15 | 1.74 kg | 607 g | 1,918 s |
| Mushroom Wrap | 4,800 | mushroom in lettuce | 45 / 20 / 35 | 1.86 kg | 690 g | 2,092 s |
| Curried Beans | 5,000 | beans and ginger | 50 / 20 / 30 | 1.97 kg | 689 g | 2,179 s |
| Nosh Noms | 5,000 | deep-fried beans | 40 / 20 / 40 | 1.92 kg | 708 g | 2,179 s |
| Tender Brisket | 5,000 | marinated, smoked | 5 / 40 / 55 | 1.92 kg | 815 g | 2,179 s |
| Squash Fries | 5,400 | fried | 50 / 5 / 45 | 1.98 kg | 715 g | 2,354 s |
| Frost Burger | 6,000 | meat, lettuce, bun | 30 / 25 / 45 | 2.29 kg | 815 g | 2,615 s |
| Surf'n'Turf | 6,000 | meat and seafood | 0 / 45 / 55 | 2.33 kg | 825 g | 2,615 s |
| Hexalent Fruit | 6,400 | rubbery, calorie dense | 50 / 5 / 45 | 2.35 kg | 847 g | 2,789 s |
| Mushroom Quiche | 6,400 | omelette on a crust | 25 / 20 / 55 | 2.36 kg | 852 g | 2,789 s |
| Uni | 7,200 | a rich delicacy | 10 / 30 / 60 | 2.68 kg | 964 g | 3,138 s |

### Using it

- Select a built generator to queue orders. Only discovered foods are listed, the lowest quality first. Each order is one unit of food and can be queued a number of times, or forever. The recipe details show the food's calories, the energy, how long it lasts and the gases.
- Duplicants fetch the food for the next orders. An order starts once its unit of food is in, and the food stays in the generator until it has all burned.
- Like the Coal Generator, it keeps burning while the batteries are full. Its Fuel Request Threshold slider works like the Coal Generator's: while every battery on its circuit is at or above it, Duplicants fetch no food for new orders and pending fetches are cancelled. An order whose food is already in still starts. Use automation to stop it, as with the Coal Generator.
- The meter shows what is left of the order in progress.
- Copy Settings copies the queue and the Fuel Request Threshold.
- An automation signal turns it on and off, like the Coal Generator.

## Options

Set in the game's mod menu (saved to `config.json` in the mod folder). Changes need a restart.

| Option | Default | Range | Effect |
|---|---|---|---|
| Power Output | 600 W | 1 to 100,000 | Power made while burning. The Coal Generator makes 600 W |
| Exhaust Heat | 8 kW | 0 to 1,000 | Heat given off to the surroundings while burning. The Coal Generator gives off 8 kW |
| Self Heat | 1 kW | 0 to 1,000 | Heat the building itself takes on while burning. The Coal Generator takes on 1 kW |

Power and both heats are paid for by the food at 4,184 J per kcal, so raising any of them burns food faster.

## How it works

The building has two parts. `FoodBurner` is a `ComplexFabricator` that no Duplicant works, set up like the Kiln. It holds the queue, fetches the food and moves an order's food into its build storage when the order starts. `FoodGenerator` is a `Generator` like `EnergyGenerator`. Each energy tick it burns a share of the order in progress: the untuned power plus the heat over the tick, divided by the order's energy. It makes power for that share, gives off the rest as heat, adds the order's gases for that share, and finishes the order once it has all burned, which consumes the food.

The heat is the building def's exhaust and self heat, which the game gives off while the building is active. With Efficient Tune-Up installed, the building has the `EfficientTuneUp` tag, and the food is charged for the heat less the tune-up's extra power, the cut that mod makes.

There is one recipe per food, made from `EdiblesManager`'s food list just before the game processes its recipes, so foods from other mods are included. Each recipe takes one unit of the food and lists its Carbon Dioxide and Steam, from `FoodComposition`, as results, so the recipe screen shows them. They are `Melted` results that aren't stored, which the fabricator never spawns. The generator lets them out itself as the order burns.

| Patch | Target | Kind |
|---|---|---|
| `Db_Initialize_Patch` | `Db.Initialize` | Postfix: adds the strings, the build menu entry and the research unlock |
| `ComplexRecipeManager_PostProcess_Patch` | `ComplexRecipeManager.PostProcess` | Prefix: makes the recipes, once every food exists |
| `ComplexFabricator_Sim200ms_Patch` | `ComplexFabricator.Sim200ms` | Prefix: skipped for the Food Generator, which would otherwise run the order on a timer and set the building active whenever it has one |
| `ComplexFabricator_GetRecipePrefetchCount_Patch` | `ComplexFabricator.GetRecipePrefetchCount` | Postfix: 0 for a Food Generator above its Fuel Request Threshold, so the fabricator plans no fetches and cancels the ones under way |

## Notes

- The building text is English only.
- Recipe ids are built from the food and its gases, as the game builds them. Saves store the queue by those ids, and the `FoodGenerator.FoodBurner` and `FoodGenerator.FoodGenerator` components by name.
- Patch targets were checked against game build 744825.
