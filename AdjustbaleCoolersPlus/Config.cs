using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace AdjustableCoolersPlus
{

    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        [Option("Thermo Aquatuner Cooling Power per °C (W)")]
        [Limit(1.0, 500.0)]
        [JsonProperty]
        public float AquatunerWattsPerDegree { get; set; } = 85f;

        [Option("Air Conditioner Cooling Power per °C (W)")]
        [Limit(1.0, 200.0)]
        [JsonProperty]
        public float AirConditionerWattsPerDegree { get; set; } = 17f;

        [Option("Minimum Temperature Delta (°C)")]
        [Limit(-100.0, 0.0)]
        [JsonProperty]
        public float MinTemperatureDelta { get; set; } = -20f;

        [Option("Maximum Temperature Delta (°C)")]
        [Limit(0.0, 100.0)]
        [JsonProperty]
        public float MaxTemperatureDelta { get; set; } = 20f;
    }
}