using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace RadiationFieldLamp
{
    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        [Option("Default Radiation per Cell", "Radiation a newly built lamp keeps in each cell of its field, in rads. Each lamp has a slider to change it.", Format = "F0")]
        [Limit(0.0, 10000.0)]
        [JsonProperty]
        public float RadsPerCell { get; set; } = 500.0f;

        [Option("Maximum Radiation per Cell", "The top of each lamp's radiation slider, in rads.", Format = "F0")]
        [Limit(1.0, 10000.0)]
        [JsonProperty]
        public float MaxRadsPerCell { get; set; } = 1000.0f;

        [Option("Power Use", "Power drawn while emitting, in watts. 0 means the lamp needs no power. All drawn power becomes heat.", Format = "F0")]
        [Limit(0.0, 1000.0)]
        [JsonProperty]
        public float PowerWatts { get; set; } = 10.0f;

        [Option("Uranium Ore per Rad", "Uranium Ore consumed while emitting, in mg per second for each rad of the lamp's radiation per cell. 0 means the lamp needs no fuel. 90% of the consumed mass comes out as Depleted Uranium.", Format = "F2")]
        [Limit(0.0, 100.0)]
        [JsonProperty]
        public float UraniumOreMgPerRad { get; set; } = 1.0f;

        [Option("Adjustable Direction", "Lets you change a built lamp's direction, including diagonals, from the Radbolt Reflector direction panel. When off, the lamp keeps the direction it was placed in.")]
        [JsonProperty]
        public bool AdjustableDirection { get; set; } = false;
    }
}
