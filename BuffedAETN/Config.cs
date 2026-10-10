using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace BuffedAETN
{
    // Options shown in the game's mod menu and saved to config.json in the mod folder. The cooling power, minimum
    // temperature and hydrogen use defaults match vanilla.
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

        [Option("Hydrogen Storage Capacity (g)", "Never less than 3 seconds of consumption")]
        [Limit(10.0, 1000.0)]
        [JsonProperty]
        public float HydrogenStorageCapacity { get; set; } = 10;
    }
}
