using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace IrradiationSterilization
{
    // Options shown in the game's mod menu and saved to config.json in the mod folder. Each germ's D50 is the radiation
    // that kills half of it per cycle; twice that kills three quarters. Defaults keep vanilla's ratio between germs:
    // Zombie Spores' vanilla kill rate is 1 against 2.5 for Food Poisoning and Slimelung, so its D50 is 2.5 times
    // theirs.
    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        private const string HalfKillTooltip =
            "Radiation that kills half of these germs each cycle. Twice as much kills three quarters. 0 keeps vanilla behavior.";

        [Option("Food Poisoning D50", HalfKillTooltip, Format = "F0")]
        [Limit(0.0, 100000.0)]
        [JsonProperty]
        public float FoodPoisoningHalfKillRads { get; set; } = 100.0f;

        [Option("Slimelung D50", HalfKillTooltip, Format = "F0")]
        [Limit(0.0, 100000.0)]
        [JsonProperty]
        public float SlimelungHalfKillRads { get; set; } = 100.0f;

        [Option("Zombie Spores D50", HalfKillTooltip, Format = "F0")]
        [Limit(0.0, 100000.0)]
        [JsonProperty]
        public float ZombieSporesHalfKillRads { get; set; } = 250.0f;

        [Option("Floral Scent D50", HalfKillTooltip + " Floral Scent is immune to radiation in vanilla.", Format = "F0")]
        [Limit(0.0, 100000.0)]
        [JsonProperty]
        public float FloralScentHalfKillRads { get; set; } = 0.0f;

        [Option("Instant-Kill Radiation", "At or above this radiation, all germs with a D50 die at once. 0 turns it off.", Format = "F0")]
        [Limit(0.0, 100000.0)]
        [JsonProperty]
        public float InstantKillRads { get; set; } = 0.0f;
    }
}
