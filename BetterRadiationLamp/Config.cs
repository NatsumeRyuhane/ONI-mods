using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace BetterRadiationLamp
{
    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        [Option("Radiation per Cell", "Radiation the lamp keeps in each of the 9 cells in front of it, in rads.", Format = "F0")]
        [Limit(1.0, 10000.0)]
        [JsonProperty]
        public float RadsPerCell { get; set; } = 500.0f;

        [Option("Power Use", "Power drawn while emitting, in watts. 0 means the lamp needs no power. All drawn power becomes heat.", Format = "F0")]
        [Limit(0.0, 1000.0)]
        [JsonProperty]
        public float PowerWatts { get; set; } = 10.0f;

        [Option("Uranium Ore Use", "Uranium Ore consumed while emitting, in kg per cycle. 0 means the lamp needs no fuel. 90% of the consumed mass comes out as Depleted Uranium.", Format = "F2")]
        [Limit(0.0, 100.0)]
        [JsonProperty]
        public float UraniumOrePerCycle { get; set; } = 0.6f;
    }
}
