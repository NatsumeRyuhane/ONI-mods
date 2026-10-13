using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace SuperMop
{
    // Options shown in the game's mod menu and saved to config.json in the mod folder. Setting speed and reach to 1,
    // with Instant Mop off, gives vanilla speed and reach, with only the liquid mass limit removed.
    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        [Option("Mop Speed Multiplier", "Vanilla mops 100 kg per second from each cell in reach.")]
        [Limit(1.0, 100.0)]
        [JsonProperty]
        public float MopSpeedMultiplier { get; set; } = 10.0f;

        [Option("Mop Reach (cells to each side)", "Vanilla mops 1 cell to each side. Walls stop the reach.")]
        [Limit(1, 10)]
        [JsonProperty]
        public int MopReach { get; set; } = 3;

        [Option("Instant Mop", "Mops all the liquid in reach at once, ignoring the speed multiplier.")]
        [JsonProperty]
        public bool InstantMop { get; set; }
    }
}
