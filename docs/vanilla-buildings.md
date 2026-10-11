# Vanilla building IDs

Display names of the game's buildings mapped to their prefab IDs (the `ID` constant on each
`IBuildingConfig`, which is what `BuildingDef.PrefabID`, `ModUtil.AddBuildingToPlanScreen`, tech
unlocks and `STRINGS.BUILDINGS.PREFABS.<ID upper-cased>` use).

Generated from game build 744825. Names are the English strings with link markup removed.
Buildings are grouped by build menu category, in the order they appear in game. The DLC column lists
the content a building needs, or the content it is hidden in. An empty cell means the base game.

482 building configs: 397 in the build menu, 32 rocket modules, 53 others.

## Contents

- [Base](#base)
- [Oxygen](#oxygen)
- [Power](#power)
- [Food](#food)
- [Plumbing](#plumbing)
- [Ventilation](#ventilation)
- [Refinement](#refinement)
- [Medicine](#medicine)
- [Furniture](#furniture)
- [Stations](#stations)
- [Utilities](#utilities)
- [Automation](#automation)
- [Shipping](#shipping)
- [Rocketry](#rocketry)
- [Radiation](#radiation)
- [Rocket modules](#rocket-modules)
- [Not in the build menu](#not-in-the-build-menu)

## Base

| Name | ID | Subcategory | DLC |
|---|---|---|---|
| Ladder | `Ladder` | Ladders |  |
| Fire Pole | `FirePole` | Ladders |  |
| Plastic Ladder | `LadderFast` | Ladders |  |
| Tile | `Tile` | Tiles and Drywall |  |
| Snow Tile | `SnowTile` | Tiles and Drywall | The Frosty Planet Pack |
| Wood Tile | `WoodTile` | Tiles and Drywall | The Frosty Planet Pack |
| Airflow Tile | `GasPermeableMembrane` | Tiles and Drywall |  |
| Mesh Tile | `MeshTile` | Tiles and Drywall |  |
| Rubber Tile | `RubberTile` | Tiles and Drywall | The Aquatic Planet Pack |
| Insulated Tile | `InsulationTile` | Tiles and Drywall |  |
| Plastic Tile | `PlasticTile` | Tiles and Drywall |  |
| Metal Tile | `MetalTile` | Tiles and Drywall |  |
| Window Tile | `GlassTile` | Tiles and Drywall |  |
| Storage Tile | `StorageTile` | Tiles and Drywall |  |
| Bunker Tile | `BunkerTile` | Tiles and Drywall |  |
| Carpeted Tile | `CarpetTile` | Tiles and Drywall |  |
| Drywall | `ExteriorWall` | Tiles and Drywall |  |
| Glass Backwall | `GlassExteriorWall` | Tiles and Drywall | The Aquatic Planet Pack |
| Mini-Pod | `ExobaseHeadquarters` | Printing Pods | Spaced Out! |
| Pneumatic Door | `Door` | Doors |  |
| Wicker Door | `WoodenDoor` | Doors | The Frosty Planet Pack |
| Manual Airlock | `ManualPressureDoor` | Doors |  |
| Insulated Door | `InsulatedDoor` | Doors |  |
| Mechanized Airlock | `PressureDoor` | Doors |  |
| Bunker Door | `BunkerDoor` | Doors |  |
| Storage Bin | `StorageLocker` | Storage |  |
| Smart Storage Bin | `StorageLockerSmart` | Storage |  |
| Liquid Reservoir | `LiquidReservoir` | Storage |  |
| Gas Reservoir | `GasReservoir` | Storage |  |
| Automatic Dispenser | `ObjectDispenser` | Storage |  |
| Transit Tube | `TravelTube` | Transit Tubes |  |
| Transit Tube Access | `TravelTubeEntrance` | Transit Tubes |  |
| Transit Tube Crossing | `TravelTubeWallBridge` | Transit Tubes |  |

## Oxygen

| Name | ID | Subcategory | DLC |
|---|---|---|---|
| Oxygen Diffuser | `MineralDeoxidizer` | Production |  |
| Sublimation Station | `SublimationStation` | Production | Spaced Out! |
| Oxylite Sconce | `Oxysconce` | Production | The Frosty Planet Pack |
| Algae Terrarium | `AlgaeHabitat` | Purification |  |
| Deodorizer | `AirFilter` | Purification |  |
| Carbon Skimmer | `CO2Scrubber` | Purification |  |
| Electrolyzer | `Electrolyzer` | Production |  |
| Rust Deoxidizer | `RustDeoxidizer` | Production |  |
| Breathing Station | `UnderwaterBreathingStation` | Distribution | The Aquatic Planet Pack |

## Power

| Name | ID | Subcategory | DLC |
|---|---|---|---|
| Dev Generator | `DevGenerator` | Generators |  |
| Manual Generator | `ManualGenerator` | Generators |  |
| Coal Generator | `Generator` | Generators |  |
| Wood Burner | `WoodGasGenerator` | Generators |  |
| Peat Burner | `PeatGenerator` | Generators | The Prehistoric Planet Pack |
| Tidal Turbine | `ReefGenerator` | Generators | The Aquatic Planet Pack |
| Hydrogen Generator | `HydrogenGenerator` | Generators |  |
| Natural Gas Generator | `MethaneGenerator` | Generators |  |
| Petroleum Generator | `PetroleumGenerator` | Generators |  |
| [DEPRECATED] Steam Turbine | `SteamTurbine` | Generators |  |
| Steam Turbine | `SteamTurbine2` | Generators |  |
| Solar Panel | `SolarPanel` | Generators |  |
| Wire | `Wire` | Wires |  |
| Wire Bridge | `WireBridge` | Wires |  |
| Heavi-Watt Wire | `HighWattageWire` | Wires |  |
| Heavi-Watt Joint Plate | `WireBridgeHighWattage` | Wires |  |
| Conductive Wire | `WireRefined` | Wires |  |
| Conductive Wire Bridge | `WireRefinedBridge` | Wires |  |
| Heavi-Watt Conductive Wire | `WireRefinedHighWattage` | Wires |  |
| Heavi-Watt Conductive Joint Plate | `WireRefinedBridgeHighWattage` | Wires |  |
| Insulated Conductive Wire | `WireRubber` | Wires |  |
| Insulated Conductive Wire Bridge | `WireRubberBridge` | Wires |  |
| Battery | `Battery` | Batteries |  |
| Jumbo Battery | `BatteryMedium` | Batteries |  |
| Smart Battery | `BatterySmart` | Batteries |  |
| Power Bank Charger | `ElectrobankCharger` | Converters | The Bionic Booster Pack |
| Compact Discharger | `SmallElectrobankDischarger` | Converters | The Bionic Booster Pack |
| Large Discharger | `LargeElectrobankDischarger` | Converters | The Bionic Booster Pack |
| Power Transformer | `PowerTransformerSmall` | Power Regulation |  |
| Large Power Transformer | `PowerTransformer` | Power Regulation |  |
| Switch | `Switch` | Switches |  |
| Power Shutoff | `LogicPowerRelay` | Switches |  |
| Thermo Switch | `TemperatureControlledSwitch` | Switches |  |
| Hydro Switch | `PressureSwitchLiquid` | Switches |  |
| Atmo Switch | `PressureSwitchGas` | Switches |  |

## Food

| Name | ID | Subcategory | DLC |
|---|---|---|---|
| Aquatic Critter Fountain | `UnderwaterMilkFeeder` | Ranching |  |
| Microbe Musher | `MicrobeMusher` | Cooking |  |
| Electric Grill | `CookingStation` | Cooking |  |
| Deep Fryer | `Deepfryer` | Cooking | The Frosty Planet Pack |
| Gas Range | `GourmetCookingStation` | Cooking |  |
| Spice Grinder | `SpiceGrinder` | Cooking |  |
| Dehydrator | `FoodDehydrator` | Cooking |  |
| Rehydrator | `FoodRehydrator` | Cooking |  |
| Smoker | `Smoker` | Cooking | The Prehistoric Planet Pack |
| Sushi Bar | `SushiBar` | Cooking | The Aquatic Planet Pack |
| Planter Box | `PlanterBox` | Farming |  |
| Farm Tile | `FarmTile` | Farming |  |
| Hydroponic Farm | `HydroponicFarm` | Farming |  |
| Wide Hydroponic Farm | `WideFarmTile` | Farming | The Aquatic Planet Pack |
| Wall Planter | `LargeBackwallFarm` | Farming | The Aquatic Planet Pack |
| Ration Box | `RationBox` | Storage |  |
| Refrigerator | `Refrigerator` | Storage |  |
| Mini Fridge | `MiniFridge` | Storage | The Aquatic Planet Pack |
| Critter Drop-Off | `CreatureDeliveryPoint` | Ranching |  |
| Critter Pick-Up | `CritterPickUp` | Ranching |  |
| Critter Drop-Off | `CritterDropOff` | Ranching |  |
| Fish Pick-Up | `FishPickUp` | Ranching |  |
| Fish Release | `FishDeliveryPoint` | Ranching |  |
| Critter Feeder | `CreatureFeeder` | Ranching |  |
| Fish Feeder | `FishFeeder` | Ranching |  |
| Critter Fountain | `MilkFeeder` | Ranching |  |
| Incubator | `EggIncubator` | Ranching |  |
| Egg Cracker | `EggCracker` | Ranching |  |
| Critter Trap | `CreatureGroundTrap` | Ranching |  |
| Fish Trap | `WaterTrap` | Ranching |  |
| Airborne Critter Trap | `CreatureAirTrap` | Ranching |  |
| Critter Condo | `CritterCondo` | Ranching |  |
| Aquatic Critter Condo | `UnderwaterCritterCondo` | Ranching |  |
| Airborne Critter Condo | `AirBorneCritterCondo` | Ranching |  |

## Plumbing

| Name | ID | Subcategory | DLC |
|---|---|---|---|
| Dev Pump Liquid | `DevPumpLiquid` | Pumps |  |
| Outhouse | `Outhouse` | Washroom |  |
| Lavatory | `FlushToilet` | Washroom |  |
| Wall Toilet | `WallToilet` | Washroom | Spaced Out! |
| Shower | `Shower` | Washroom |  |
| Gunk Extractor | `GunkEmptier` | Washroom | The Bionic Booster Pack |
| Pitcher Pump | `LiquidPumpingStation` | Pumps |  |
| Bottle Emptier | `BottleEmptier` | Valves |  |
| Bottle Drainer | `BottleEmptierConduitLiquid` | Valves |  |
| Bottle Filler | `LiquidBottler` | Valves |  |
| Liquid Pipe | `LiquidConduit` | Pipes |  |
| Insulated Liquid Pipe | `InsulatedLiquidConduit` | Pipes |  |
| Radiant Liquid Pipe | `LiquidConduitRadiant` | Pipes |  |
| Liquid Bridge | `LiquidConduitBridge` | Pipes |  |
| Priority Liquid Flow | `LiquidConduitPreferentialFlow` | Valves |  |
| Liquid Overflow Valve | `LiquidConduitOverflow` | Valves |  |
| Liquid Pump | `LiquidPump` | Pumps |  |
| Mini Liquid Pump | `LiquidMiniPump` | Pumps |  |
| Liquid Vent | `LiquidVent` | Pipes |  |
| Liquid Filter | `LiquidFilter` | Valves |  |
| Liquid Valve | `LiquidValve` | Valves |  |
| Liquid Shutoff | `LiquidLogicValve` | Valves |  |
| Liquid Meter Valve | `LiquidLimitValve` | Valves |  |
| Liquid Pipe Element Sensor | `LiquidConduitElementSensor` | Sensors |  |
| Liquid Pipe Germ Sensor | `LiquidConduitDiseaseSensor` | Sensors |  |
| Liquid Pipe Thermo Sensor | `LiquidConduitTemperatureSensor` | Sensors |  |
| Liquid Rocket Port Loader | `ModularLaunchpadPortLiquid` | Ports | Spaced Out! |
| Liquid Rocket Port Unloader | `ModularLaunchpadPortLiquidUnloader` | Ports | Spaced Out! |
| Conduction Panel | `ContactConductivePipeBridge` | Pipes |  |

## Ventilation

| Name | ID | Subcategory | DLC |
|---|---|---|---|
| Dev Pump Gas | `DevPumpGas` | Pumps |  |
| Gas Pipe | `GasConduit` | Pipes |  |
| Insulated Gas Pipe | `InsulatedGasConduit` | Pipes |  |
| Radiant Gas Pipe | `GasConduitRadiant` | Pipes |  |
| Gas Bridge | `GasConduitBridge` | Pipes |  |
| Priority Gas Flow | `GasConduitPreferentialFlow` | Valves |  |
| Gas Overflow Valve | `GasConduitOverflow` | Valves |  |
| Gas Pump | `GasPump` | Pumps |  |
| Mini Gas Pump | `GasMiniPump` | Pumps |  |
| Gas Vent | `GasVent` | Pipes |  |
| High Pressure Gas Vent | `GasVentHighPressure` | Pipes |  |
| Gas Filter | `GasFilter` | Valves |  |
| Gas Valve | `GasValve` | Valves |  |
| Gas Shutoff | `GasLogicValve` | Valves |  |
| Gas Meter Valve | `GasLimitValve` | Valves |  |
| Canister Filler | `GasBottler` | Valves |  |
| Canister Emptier | `BottleEmptierGas` | Valves |  |
| Canister Drainer | `BottleEmptierConduitGas` | Valves |  |
| Gas Rocket Port Loader | `ModularLaunchpadPortGas` | Ports | Spaced Out! |
| Gas Rocket Port Unloader | `ModularLaunchpadPortGasUnloader` | Ports | Spaced Out! |
| Gas Pipe Element Sensor | `GasConduitElementSensor` | Sensors |  |
| Gas Pipe Germ Sensor | `GasConduitDiseaseSensor` | Sensors |  |
| Gas Pipe Thermo Sensor | `GasConduitTemperatureSensor` | Sensors |  |

## Refinement

| Name | ID | Subcategory | DLC |
|---|---|---|---|
| Plywood Press | `FabricatedWoodMaker` | Materials |  |
| Compost | `Compost` | Organic |  |
| Water Sieve | `WaterPurifier` | Materials |  |
| Desalinator | `Desalinator` | Materials |  |
| Fertilizer Synthesizer | `FertilizerMaker` | Organic |  |
| Algae Distiller | `AlgaeDistillery` | Organic |  |
| Ethanol Distiller | `EthanolDistillery` | Organic |  |
| Rock Crusher | `RockCrusher` | Materials |  |
| Kiln | `Kiln` | Materials |  |
| Sludge Press | `SludgePress` | Organic | The Aquatic Planet Pack |
| Metal Refinery | `MetalRefinery` | Materials |  |
| Glass Forge | `GlassForge` | Materials |  |
| Oil Refinery | `OilRefinery` | Oil |  |
| Polymer Press | `Polymerizer` | Oil |  |
| Vulcanizer | `RubberMaker` | Materials | The Aquatic Planet Pack |
| Oxylite Refinery | `OxyliteRefinery` | Advanced |  |
| Bleach Stone Hopper | `Chlorinator` | Advanced |  |
| Emulsifier | `ChemicalRefinery` | Advanced |  |
| Molecular Forge | `SupermaterialRefinery` | Advanced |  |
| Diamond Press | `DiamondPress` | Advanced | Spaced Out! |
| Gleaner | `MilkFatSeparator` | Organic |  |
| Plant Pulverizer | `MilkPress` | Organic |  |

## Medicine

| Name | ID | Subcategory | DLC |
|---|---|---|---|
| Dev Life Support | `DevLifeSupport` | Medical |  |
| Wash Basin | `WashBasin` | Hygiene |  |
| Sink | `WashSink` | Hygiene |  |
| Hand Sanitizer | `HandSanitizer` | Hygiene |  |
| Decontamination Shower | `DecontaminationShower` | Hygiene | Spaced Out! |
| Lubrication Station | `OilChanger` | Wellness | The Bionic Booster Pack |
| Apothecary | `Apothecary` | Medical |  |
| Sick Bay | `DoctorStation` | Medical |  |
| Disease Clinic | `AdvancedDoctorStation` | Medical |  |
| Triage Cot | `MedicalCot` | Medical |  |
| Massage Table | `MassageTable` | Wellness |  |
| Tasteful Memorial | `Grave` | Wellness |  |

## Furniture

| Name | ID | Subcategory | DLC |
|---|---|---|---|
| Display Shelf | `Shelf` | Decor |  |
| Cot | `Bed` | Beds |  |
| Comfy Bed | `LuxuryBed` | Beds |  |
| Ladder Bed | `LadderBed` | Beds | Spaced Out! |
| Lamp | `FloorLamp` | Lights |  |
| Ceiling Light | `CeilingLight` | Lights |  |
| Glass Jelly Ceiling Light | `GlassCeilingLight` | Lights | The Aquatic Planet Pack |
| Sun Lamp | `SunLamp` | Lights |  |
| Dev Light Source | `DevLightGenerator` | Lights |  |
| Mercury Ceiling Light | `MercuryCeilingLight` | Lights | The Frosty Planet Pack |
| Mess Table | `DiningTable` | Dining |  |
| Communal Table | `MultiMinionDiningTable` | Dining |  |
| Water Cooler | `WaterCooler` | Recreation |  |
| Jukebot | `Phonobox` | Recreation |  |
| Arcade Cabinet | `ArcadeMachine` | Recreation |  |
| Espresso Machine | `EspressoMachine` | Recreation |  |
| Hot Tub | `HotTub` | Recreation |  |
| Mechanical Surfboard | `MechanicalSurfboard` | Recreation |  |
| Sauna | `Sauna` | Recreation |  |
| Juicer | `Juicer` | Recreation |  |
| Soda Fountain | `SodaFountain` | Recreation |  |
| Beach Chair | `BeachChair` | Recreation |  |
| Vertical Wind Tunnel | `VerticalWindTunnel` | Recreation |  |
| Pixel Pack | `PixelPack` | Decor |  |
| Party Line Phone | `Telephone` | Recreation | Spaced Out! |
| Flower Pot | `FlowerVase` | Decor |  |
| Wall Pot | `FlowerVaseWall` | Decor |  |
| Hanging Pot | `FlowerVaseHanging` | Decor |  |
| Aero Pot | `FlowerVaseHangingFancy` | Decor |  |
| Sculpting Block | `SmallSculpture` | Decor |  |
| Large Sculpting Block | `Sculpture` | Decor |  |
| Ice Block | `IceSculpture` | Decor |  |
| Wood Block | `WoodSculpture` | Decor | The Frosty Planet Pack |
| Marble Block | `MarbleSculpture` | Decor |  |
| Metal Block | `MetalSculpture` | Decor |  |
| Fossil Block | `FossilSculpture` | Decor | The Prehistoric Planet Pack |
| Hanging Fossil Block | `CeilingFossilSculpture` | Decor | The Prehistoric Planet Pack |
| Ceiling Trim | `CrownMoulding` | Decor |  |
| Corner Trim | `CornerMoulding` | Decor |  |
| Blank Canvas | `Canvas` | Decor |  |
| Landscape Canvas | `CanvasWide` | Decor |  |
| Portrait Canvas | `CanvasTall` | Decor |  |
| Pedestal | `ItemPedestal` | Decor |  |
| Monument Base | `MonumentBottom` | Decor |  |
| Monument Midsection | `MonumentMiddle` | Decor |  |
| Monument Top | `MonumentTop` | Decor |  |
| Park Sign | `ParkSign` | Decor |  |

## Stations

| Name | ID | Subcategory | DLC |
|---|---|---|---|
| Research Station | `ResearchCenter` | Research |  |
| Super Computer | `AdvancedResearchCenter` | Research |  |
| Materials Study Terminal | `NuclearResearchCenter` | Research | Spaced Out! |
| Orbital Data Collection Lab | `OrbitalResearchCenter` | Research | Spaced Out! |
| Virtual Planetarium | `CosmicResearchCenter` | Research | not in Spaced Out! |
| Virtual Planetarium | `DLC1CosmicResearchCenter` | Research | Spaced Out! |
| Telescope | `Telescope` | Telescopes | not in Spaced Out! |
| Geotuner | `GeoTuner` | Research |  |
| Data Miner | `DataMiner` | Research | The Bionic Booster Pack |
| Power Control Station | `PowerControlStation` | Industrial |  |
| Farm Station | `FarmStation` | Farming |  |
| Botanical Analyzer | `GeneticAnalysisStation` | Farming | Spaced Out! |
| Grooming Station | `RanchStation` | Ranching |  |
| Shearing Station | `ShearingStation` | Ranching |  |
| Milking Station | `MilkingStation` | Ranching |  |
| Aquatic Grooming Station | `UnderwaterRanchStation` | Ranching | The Aquatic Planet Pack |
| Aquatic Shearing Station | `UnderwaterShearingStation` | Ranching | The Aquatic Planet Pack |
| Aquatic Milking Station | `UnderwaterMilkingStation` | Ranching | The Aquatic Planet Pack |
| Skills Board | `RoleStation` | Workstations |  |
| Skill Scrubber | `ResetSkillsStation` | Industrial |  |
| Artifact Analysis Station | `ArtifactAnalysisStation` | Archaeology | Spaced Out! |
| Remote Worker Dock | `RemoteWorkerDock` | Operations | The Bionic Booster Pack |
| Remote Controller | `RemoteWorkTerminal` | Operations | The Bionic Booster Pack |
| Blastshot Maker | `MissileFabricator` | Meteor Defense |  |
| Crafting Station | `CraftingTable` | Manufacturing |  |
| Soldering Station | `AdvancedCraftingTable` | Manufacturing | The Bionic Booster Pack |
| Textile Loom | `ClothingFabricator` | Manufacturing |  |
| Clothing Refashionator | `ClothingAlterationStation` | Manufacturing |  |
| Exosuit Forge | `SuitFabricator` | Manufacturing |  |
| Oxygen Mask Checkpoint | `OxygenMaskMarker` | Equipment |  |
| Oxygen Mask Dock | `OxygenMaskLocker` | Equipment |  |
| Atmo Suit Checkpoint | `SuitMarker` | Equipment |  |
| Atmo Suit Dock | `SuitLocker` | Equipment |  |
| Jet Suit Checkpoint | `JetSuitMarker` | Equipment |  |
| Jet Suit Dock | `JetSuitLocker` | Equipment |  |
| Lead Suit Checkpoint | `LeadSuitMarker` | Equipment | Spaced Out! |
| Lead Suit Dock | `LeadSuitLocker` | Equipment | Spaced Out! |
| Space Cadet Centrifuge | `AstronautTrainingCenter` | Exploration |  |

## Utilities

| Name | ID | Subcategory | DLC |
|---|---|---|---|
| Marine Drill | `UnderwaterVentDrill` | Structural | The Aquatic Planet Pack |
| Wood Heater | `Campfire` | Temperature | The Frosty Planet Pack |
| Dev Heater | `DevHeater` | Temperature |  |
| Ice Liquefier | `IceKettle` | Materials | The Frosty Planet Pack |
| Space Heater | `SpaceHeater` | Temperature |  |
| Liquid Tepidizer | `LiquidHeater` | Temperature |  |
| Hydrofan | `LiquidCooledFan` | Temperature |  |
| Ice-E Fan | `IceCooledFan` | Temperature |  |
| Ice Maker | `IceMachine` | Temperature |  |
| Thermo Regulator | `AirConditioner` | Temperature |  |
| Thermo Aquatuner | `LiquidConditioner` | Temperature |  |
| Ore Scrubber | `OreScrubber` | Sanitation |  |
| Oil Well | `OilWellCap` | Oil |  |
| Tempshift Plate | `ThermalBlock` | Temperature |  |
| Sweepy's Dock | `SweepBotStation` | Sanitation |  |

## Automation

| Name | ID | Subcategory | DLC |
|---|---|---|---|
| Automation Wire | `LogicWire` | Wires |  |
| Automation Wire Bridge | `LogicWireBridge` | Wires |  |
| Automation Ribbon | `LogicRibbon` | Wires |  |
| Automation Ribbon Bridge | `LogicRibbonBridge` | Wires |  |
| Signal Switch | `LogicSwitch` | Switches |  |
| Duplicant Motion Sensor | `LogicDuplicantSensor` | Sensors |  |
| Atmo Sensor | `LogicPressureSensorGas` | Sensors |  |
| Hydro Sensor | `LogicPressureSensorLiquid` | Sensors |  |
| Thermo Sensor | `LogicTemperatureSensor` | Sensors |  |
| Light Sensor | `LogicLightSensor` | Sensors |  |
| Wattage Sensor | `LogicWattageSensor` | Sensors |  |
| Cycle Sensor | `LogicTimeOfDaySensor` | Sensors |  |
| Timer Sensor | `LogicTimerSensor` | Sensors |  |
| Germ Sensor | `LogicDiseaseSensor` | Sensors |  |
| Gas Element Sensor | `LogicElementSensorGas` | Sensors |  |
| Liquid Element Sensor | `LogicElementSensorLiquid` | Sensors |  |
| Critter Sensor | `LogicCritterCountSensor` | Sensors |  |
| Radiation Sensor | `LogicRadiationSensor` | Sensors | Spaced Out! |
| Radbolt Sensor | `LogicHEPSensor` | Sensors | Spaced Out! |
| Signal Counter | `LogicCounter` | Monitoring |  |
| Automated Notifier | `LogicAlarm` | Monitoring |  |
| Hammer | `LogicHammer` | Ambience |  |
| Automation Broadcaster | `LogicInterasteroidSender` | Transmissions | Spaced Out! |
| Automation Receiver | `LogicInterasteroidReceiver` | Transmissions | Spaced Out! |
| Ribbon Reader | `LogicRibbonReader` | Wires |  |
| Ribbon Writer | `LogicRibbonWriter` | Wires |  |
| Weight Plate | `FloorSwitch` | Switches |  |
| Duplicant Checkpoint | `Checkpoint` | Monitoring |  |
| Space Scanner | `CometDetector` | Sensors |  |
| NOT Gate | `LogicGateNOT` | Gates |  |
| AND Gate | `LogicGateAND` | Gates |  |
| OR Gate | `LogicGateOR` | Gates |  |
| BUFFER Gate | `LogicGateBUFFER` | Gates |  |
| FILTER Gate | `LogicGateFILTER` | Gates |  |
| XOR Gate | `LogicGateXOR` | Gates |  |
| Memory Toggle | `LogicMemory` | Gates |  |
| Signal Selector | `LogicGateMultiplexer` | Gates |  |
| Signal Distributor | `LogicGateDemultiplexer` | Gates |  |

## Shipping

| Name | ID | Subcategory | DLC |
|---|---|---|---|
| Dev Pump Solid | `DevPumpSolid` | Pumps |  |
| Auto-Sweeper | `SolidTransferArm` | Automated |  |
| Conveyor Rail | `SolidConduit` | Structural |  |
| Conveyor Bridge | `SolidConduitBridge` | Structural |  |
| Conveyor Loader | `SolidConduitInbox` | Structural |  |
| Conveyor Receptacle | `SolidConduitOutbox` | Structural |  |
| Solid Filter | `SolidFilter` | Structural |  |
| Conveyor Chute | `SolidVent` | Structural |  |
| Conveyor Shutoff | `SolidLogicValve` | Valves |  |
| Conveyor Meter | `SolidLimitValve` | Valves |  |
| Conveyor Rail Germ Sensor | `SolidConduitDiseaseSensor` | Sensors |  |
| Conveyor Rail Element Sensor | `SolidConduitElementSensor` | Sensors |  |
| Conveyor Rail Thermo Sensor | `SolidConduitTemperatureSensor` | Sensors |  |
| Robo-Miner | `AutoMiner` | Automated |  |
| Solid Rocket Port Loader | `ModularLaunchpadPortSolid` | Ports | Spaced Out! |
| Solid Rocket Port Unloader | `ModularLaunchpadPortSolidUnloader` | Ports | Spaced Out! |

## Rocketry

| Name | ID | Subcategory | DLC |
|---|---|---|---|
| Telescope | `ClusterTelescope` | Telescopes | Spaced Out! |
| Enclosed Telescope | `ClusterTelescopeEnclosed` | Telescopes | Spaced Out! |
| Mission Control Station | `MissionControl` | Navigation | not in Spaced Out! |
| Mission Control Station | `MissionControlCluster` | Navigation | Spaced Out! |
| Rocket Platform | `LaunchPad` | Structural | Spaced Out! |
| Gantry | `Gantry` | Structural |  |
| Steam Engine | `SteamEngine` | Engines | not in Spaced Out! |
| Petroleum Engine | `KeroseneEngine` | Engines | not in Spaced Out! |
| Biodiesel Engine | `BiodieselEngine` | Engines | not in Spaced Out! |
| Solid Fuel Thruster | `SolidBooster` | Engines | not in Spaced Out! |
| Liquid Fuel Tank | `LiquidFuelTank` | Tanks | not in Spaced Out! |
| Solid Oxidizer Tank | `OxidizerTank` | Tanks | not in Spaced Out! |
| Liquid Oxidizer Tank | `OxidizerTankLiquid` | Tanks | not in Spaced Out! |
| Cargo Bay | `CargoBay` | Cargo | not in Spaced Out! |
| Gas Cargo Canister | `GasCargoBay` | Cargo | not in Spaced Out! |
| Liquid Cargo Tank | `LiquidCargoBay` | Cargo | not in Spaced Out! |
| Command Capsule | `CommandModule` | Navigation | not in Spaced Out! |
| Robo-Pilot Capsule | `RoboPilotCommandModule` | Navigation | The Bionic Booster Pack; not in Spaced Out! |
| Sight-Seeing Module | `TouristModule` | Modules | not in Spaced Out! |
| Research Module | `ResearchModule` | Modules | not in Spaced Out! |
| Biological Cargo Bay | `SpecialCargoBay` | Cargo | not in Spaced Out! |
| Hydrogen Engine | `HydrogenEngine` | Engines | not in Spaced Out! |
| Rocket Control Station | `RocketControlStation` | Navigation | Spaced Out! |
| Power Outlet Fitting | `RocketInteriorPowerPlug` | Fittings | Spaced Out! |
| Liquid Intake Fitting | `RocketInteriorLiquidInput` | Fittings | Spaced Out! |
| Liquid Output Fitting | `RocketInteriorLiquidOutput` | Fittings | Spaced Out! |
| Gas Intake Fitting | `RocketInteriorGasInput` | Fittings | Spaced Out! |
| Gas Output Fitting | `RocketInteriorGasOutput` | Fittings | Spaced Out! |
| Conveyor Receptacle Fitting | `RocketInteriorSolidInput` | Fittings | Spaced Out! |
| Conveyor Loader Fitting | `RocketInteriorSolidOutput` | Fittings | Spaced Out! |
| Starmap Location Sensor | `LogicClusterLocationSensor` | Navigation | Spaced Out! |
| Interplanetary Launcher | `RailGun` | Fittings | Spaced Out! |
| Payload Opener | `RailGunPayloadOpener` | Fittings | Spaced Out! |
| Targeting Beacon | `LandingBeacon` | Navigation | Spaced Out! |
| Meteor Blaster | `MissileLauncher` | Meteor Defense |  |
| Rocket Port Extension | `ModularLaunchpadPortBridge` | Structural | Spaced Out! |

## Radiation

| Name | ID | Subcategory | DLC |
|---|---|---|---|
| Radiation Lamp | `RadiationLight` | Production | Spaced Out! |
| Manual Radbolt Generator | `ManualHighEnergyParticleSpawner` | Production | Spaced Out! |
| Research Reactor | `NuclearReactor` | Production | Spaced Out! |
| Uranium Centrifuge | `UraniumCentrifuge` | Production | Spaced Out! |
| Radbolt Generator | `HighEnergyParticleSpawner` | Production | Spaced Out! |
| Dev Radbolt Generator | `DevHEPSpawner` | Production | Spaced Out! |
| Radbolt Reflector | `HighEnergyParticleRedirector` | Transmissions | Spaced Out! |
| Radbolt Chamber | `HEPBattery` | Batteries | Spaced Out! |
| Radbolt Joint Plate | `HEPBridgeTile` | Transmissions | Spaced Out! |
| Dev Radiation Emitter | `DevRadiationGenerator` | Production |  |

## Rocket modules

Built from a rocket's module screen, not the build menu.

| Name | ID | DLC |
|---|---|---|
| Artifact Transport Module | `ArtifactCargoBay` | Spaced Out! |
| Basic Nosecone | `NoseconeBasic` | Spaced Out! |
| Battery Module | `BatteryModule` | Spaced Out! |
| Biodiesel Engine | `BiodieselEngineCluster` | Spaced Out! |
| Carbon Dioxide Engine | `CO2Engine` | Spaced Out! |
| Cargo Bay | `SolidCargoBaySmall` | Spaced Out! |
| Cartographic Module | `ScannerModule` | Spaced Out! |
| Critter Cargo Bay | `SpecialCargoBayCluster` | Spaced Out! |
| Drillcone | `NoseconeHarvest` | Spaced Out! |
| Gas Cargo Canister | `GasCargoBaySmall` | Spaced Out! |
| Hydrogen Engine | `HydrogenEngineCluster` | Spaced Out! |
| Large Cargo Bay | `CargoBayCluster` | Spaced Out! |
| Large Gas Cargo Canister | `GasCargoBayCluster` | Spaced Out! |
| Large Liquid Cargo Tank | `LiquidCargoBayCluster` | Spaced Out! |
| Large Liquid Fuel Tank | `LiquidFuelTankCluster` | Spaced Out! |
| Large Solid Oxidizer Tank | `OxidizerTankCluster` | Spaced Out! |
| Liquid Cargo Tank | `LiquidCargoBaySmall` | Spaced Out! |
| Liquid Oxidizer Tank | `OxidizerTankLiquidCluster` | Spaced Out! |
| Orbital Cargo Module | `OrbitalCargoModule` | Spaced Out! |
| Petroleum Engine | `KeroseneEngineCluster` | Spaced Out! |
| Radbolt Engine | `HEPEngine` | Spaced Out! |
| Research Module | `ResearchClusterModule` | Spaced Out! |
| Robo-Pilot Module | `RoboPilotModule` | Spaced Out!, The Bionic Booster Pack |
| Rover's Module | `ScoutModule` | Spaced Out! |
| Small Petroleum Engine | `KeroseneEngineClusterSmall` | Spaced Out! |
| Small Solid Oxidizer Tank | `SmallOxidizerTank` | Spaced Out! |
| Solar Panel Module | `SolarPanelModule` | Spaced Out! |
| Solo Spacefarer Nosecone | `HabitatModuleSmall` | Spaced Out! |
| Spacefarer Module | `HabitatModuleMedium` | Spaced Out! |
| Steam Engine | `SteamEngineCluster` | Spaced Out! |
| Sugar Engine | `SugarEngine` | Spaced Out! |
| Trailblazer Module | `PioneerModule` | Spaced Out! |

## Not in the build menu

Placed by world generation (Gravitas facility props, the Printing Pod, story buildings), made by critters, or no longer buildable. Some IDs have no display name.

| Name | ID | DLC |
|---|---|---|
| Airborne Critter Bait | `FlyingCreatureBait` |  |
| Airborne Critter Lure | `AirborneCreatureLure` |  |
| Ancient Specimen | `FossilDig` |  |
| Anti Entropy Thermo-Nullifier | `MassiveHeatSink` |  |
| Biobot Builder | `MorbRoverMaker` |  |
| Critter Flux-O-Matic | `GravitasCreatureManipulator` |  |
| Critter Trap | `CreatureTrap` |  |
| Fish Trap | `FishTrap` |  |
| Gas Spacefarer Input Port | `RocketInteriorGasInputPort` | Spaced Out! |
| Gas Spacefarer Output Port | `RocketInteriorGasOutputPort` | Spaced Out! |
| Gravitas Door | `GravitasDoor` |  |
| Gravitas Pedestal | `GravitasPedestal` | Spaced Out! |
| Gravitas Shipping Container | `LonelyMinionHouse` |  |
| Lab Wall | `PropGravitasLabWall` |  |
| Lab Window | `PropGravitasLabWindow` |  |
| Lab Window | `PropGravitasLabWindowHorizontal` | Spaced Out! |
| LED Light | `GravitasLabLight` |  |
| Liquid Heatquilizer | `HeatCompressor` |  |
| Liquid Spacefarer Input Port | `RocketInteriorLiquidInputPort` | Spaced Out! |
| Liquid Spacefarer Output Port | `RocketInteriorLiquidOutputPort` | Spaced Out! |
| Lobby Doors | `POIFacilityDoor` |  |
| Mailbox | `LonelyMailBox` |  |
| Mechanics Station | `MachineShop` |  |
| Nuclear Apothecary | `AdvancedApothecary` | Spaced Out! |
| Omniprinter | `GenericFabricator` |  |
| Oxygen Mask Station | `OxygenMaskStation` |  |
| Pajama Cubby | `GravitasContainer` |  |
| Plug Slug | `StaterpillarGenerator` | Spaced Out! |
| Printerceptor | `HijackedHeadquarters` |  |
| Printing Pod | `Headquarters` |  |
| Rocket Wall | `RocketWallTile` | Spaced Out! |
| Rocket Window | `RocketEnvelopeWindowTile` |  |
| Security Door | `POIBunkerExteriorDoor` |  |
| Security Door | `POIDoorInternal` |  |
| Showroom Doors | `POIDlc2ShowroomDoor` |  |
| Somnium Synthesizer | `MegaBrainTank` |  |
| Supply Teleporter Input | `WarpConduitSender` | Spaced Out! |
| Supply Teleporter Output | `WarpConduitReceiver` | Spaced Out! |
| Tattered Beach Chair | `PropBeachChair` | The Aquatic Planet Pack |
| Teleporter Pad | `TeleportalPad` |  |
| Temporal Tear Opener | `TemporalTearOpener` | Spaced Out! |
| Tile | `TilePOI` |  |
| Toilet | `GravitasBathroomStall` |  |
| Trimming Tile | `MouldingTile` |  |
| Wall | `PropGravitasWall` |  |
| Wall | `PropGravitasWallPurple` |  |
| Wall | `PropGravitasWallPurpleWhiteDiagonal` |  |
| Window | `FacilityBackWallWindow` |  |
| Wood Pile | `WoodStorage` | The Frosty Planet Pack |
| — | `AtomicGarden` |  |
| — | `CrewCapsule` |  |
| — | `StaterpillarGasConnector` | Spaced Out! |
| — | `StaterpillarLiquidConnector` | Spaced Out! |
