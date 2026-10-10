using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace BetterEmptier
{
    // Options shown in the game's mod menu and saved to config.json in the mod folder. The defaults match vanilla.
    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        [Option("Emptier Capacity (kg)")]
        [Limit(10, 100000)]
        [JsonProperty]
        public int Capacity { get; set; } = 200;

        [Option("Release Speed Multiplier")]
        [Limit(0.1, 1000.0)]
        [JsonProperty]
        public float EmissionRateMultiplier { get; set; } = 1.0f;
    }
}
