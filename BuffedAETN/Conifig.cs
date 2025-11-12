using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace BuffedAETN
{
    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        [Option("Cooling Power (kW)")]
        [Limit(0.0, 10000.0)]
        [JsonProperty]
        public int CoolingPower { get; set; } = 80;
        
        [Option("Minimum Operating Temperature", "K, setting this too low with high cooling power may crash the game")]
        [Limit(0.0, 300.0)]
        [JsonProperty]
        public float MinimumOperatingTemperature { get; set; } = 100.0f;
        
        [Option("Hydrogen Consumption Rate (g/s)")]
        [Limit(0.0, 1000.0)]
        [JsonProperty]
        public int HydrogenConsumptionRate { get; set; } = 10;
        
        [Option("Hydrogen Storage Capacity (g)", "will be at least 5 second worth of consumption")]
        [Limit(10.0, 1000.0)]
        [JsonProperty]
        public float HydrogenStorageCapacity { get; set; } = 10;
    }
}