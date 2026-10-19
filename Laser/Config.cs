using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace Laser
{
    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        [Option("Default Brightness", "Brightness of a newly built LASER, in lux. Each LASER has a slider to change it.", Format = "F0")]
        [Limit(1000.0, 10000000.0)]
        [JsonProperty]
        public int DefaultLux { get; set; } = 100000;

        [Option("Maximum Brightness", "The top of each LASER's brightness slider, in lux.", Format = "F0")]
        [Limit(1000.0, 10000000.0)]
        [JsonProperty]
        public int MaxLux { get; set; } = 1000000;

        [Option("Power per 100,000 Lux", "Power drawn while lit for each 100,000 lux of brightness, in watts. 0 means the LASER needs no power. All drawn power becomes heat.", Format = "F1")]
        [Limit(0.0, 1000.0)]
        [JsonProperty]
        public float WattsPer100kLux { get; set; } = 5.0f;

        [Option("Beam Length", "Cells lit by the beam, counting the LASER's own cell. Solid tiles stop it, windows don't.", Format = "F0")]
        [Limit(1.0, 64.0)]
        [JsonProperty]
        public int BeamLength { get; set; } = 16;
    }
}
