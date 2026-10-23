using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace FoodGenerator
{
    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        [Option("Power Output", "Power made while burning, in watts. The Coal Generator makes 600 W.", Format = "F0")]
        [Limit(1.0, 100000.0)]
        [JsonProperty]
        public float PowerWatts { get; set; } = 600.0f;

        [Option("Exhaust Heat", "Heat given off to the surroundings while burning, in kW. The Coal Generator gives off 8 kW. Food pays for this heat too, at 4,184 J per kcal.", Format = "F1")]
        [Limit(0.0, 1000.0)]
        [JsonProperty]
        public float ExhaustHeatKw { get; set; } = 8.0f;

        [Option("Self Heat", "Heat the generator itself takes on while burning, in kW. The Coal Generator takes on 1 kW. Food pays for this heat too, at 4,184 J per kcal.", Format = "F1")]
        [Limit(0.0, 1000.0)]
        [JsonProperty]
        public float SelfHeatKw { get; set; } = 1.0f;
    }
}
