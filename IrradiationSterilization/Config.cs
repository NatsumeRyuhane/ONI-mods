using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace IrradiationSterilization
{
    // Options shown in the game's mod menu and saved to config.json in the mod folder.
    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        [Option("Radiation Sensitivity Multiplier", "Multiplies how many germs each rad kills per second. 1 is vanilla.", Format = "F1")]
        [Limit(0.0, 1000.0)]
        [JsonProperty]
        public float KillRateMultiplier { get; set; } = 10.0f;
    }
}
