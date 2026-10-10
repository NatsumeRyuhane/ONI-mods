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
        [Option("Half-Kill Radiation", "At this radiation, half the germs die each cycle. Twice as much kills three quarters, three times as much kills seven eighths.", Format = "F0")]
        [Limit(1.0, 100000.0)]
        [JsonProperty]
        public float HalfKillRads { get; set; } = 100.0f;

        [Option("Instant-Kill Radiation", "At or above this radiation, all germs die at once.", Format = "F0")]
        [Limit(1.0, 100000.0)]
        [JsonProperty]
        public float InstantKillRads { get; set; } = 10000.0f;

        [Option("Affect Floral Scent", "Floral Scent is immune to radiation in vanilla. When on, it dies to radiation like the other germs.")]
        [JsonProperty]
        public bool AffectFloralScent { get; set; } = false;
    }
}
