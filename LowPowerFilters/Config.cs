using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace LowPowerFilters
{
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