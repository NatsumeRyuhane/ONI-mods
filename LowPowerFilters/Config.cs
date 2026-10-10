using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace LowPowerFilters
{
    // Options shown in the game's mod menu and saved to config.json in the mod folder. Each filter uses 120 W in vanilla.
    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        [Option("Gas Filter Power Consumption (W)")]
        [Limit(0.0, 1000.0)]
        [JsonProperty]
        public int FilterPowerRequirementGas { get; set; } = 10;

        [Option("Liquid Filter Power Consumption (W)")]
        [Limit(0.0, 1000.0)]
        [JsonProperty]
        public int FilterPowerRequirementLiquid { get; set; } = 10;

        [Option("Solid Filter Power Consumption (W)")]
        [Limit(0.0, 1000.0)]
        [JsonProperty]
        public int FilterPowerRequirementSolid { get; set; } = 10;
    }
}
