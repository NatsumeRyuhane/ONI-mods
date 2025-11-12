using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace BuffedWheezewort
{
    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        [Option("Radiation Emission", "rads", Format = "F0")]
        [Limit(0.0, 10000.0)]
        [JsonProperty]
        public float Radiation { get; set; } = 0.0f;

        [Option("Phosphorus Consumption Rate", "kg/cycle")]
        [Limit(0.0, 100.0)]
        [JsonProperty]
        public float PhosphorusConsumptionRate { get; set; } = 0.01f;
        
        [Option("Emission Temperature Delta", null, Format = "F0")]
        [Limit(-100.0, -5.0)]
        [JsonProperty]
        public float EmissionTemperatureDelta { get; set; } = -5.0f;
        
    }
}