using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace BuffedWheezewort
{
    // Options shown in the game's mod menu and saved to config.json in the mod folder.
    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        [Option("Radiation Emission", "Radiation the plant emits, in rads. Vanilla is 480. Has no effect without Spaced Out.", Format = "F0")]
        [Limit(0.0, 10000.0)]
        [JsonProperty]
        public float Radiation { get; set; } = 480.0f;

        [Option("Phosphorite Use", "Phosphorite fertilizer a planted Wheezewort uses, in g per cycle. Vanilla is 4000. At 0 it needs no fertilizer.", Format = "F0")]
        [Limit(0.0, 10000.0)]
        [JsonProperty]
        public float PhosphoriteGramsPerCycle { get; set; } = 4000.0f;

        [Option("Emission Temperature Delta", "How much the plant cools the gas it breathes, in degrees. Vanilla is -5. The gas is never cooled below 5 degrees above its condensation point.", Format = "F0")]
        [Limit(-500.0, 0.0)]
        [JsonProperty]
        public float EmissionTemperatureDelta { get; set; } = -5.0f;

        [Option("Cool Below Condensation", "Lets the plant cool gas below its condensation point, so the gas comes out as liquid. Vanilla stops 5 degrees above the condensation point. The liquid can fall back onto the plant and drown it.")]
        [JsonProperty]
        public bool CoolBelowCondensation { get; set; } = false;

        [Option("Gas Throughput", "Gas a planted Wheezewort breathes, in kg/s. Vanilla is 1.")]
        [Limit(0.1, 100.0)]
        [JsonProperty]
        public float GasThroughput { get; set; } = 1.0f;

        [Option("Wild Throughput Multiplier", "Share of the gas throughput a wild Wheezewort breathes. Vanilla is 0.25.")]
        [Limit(0.0, 1.0)]
        [JsonProperty]
        public float WildThroughputMultiplier { get; set; } = 0.25f;

        [Option("Minimum Growth Temperature", "In K. Vanilla is 213.15 K (-60 C). The plant dies 30 K below this.", Format = "F2")]
        [Limit(35.0, 373.15)]
        [JsonProperty]
        public float MinGrowthTemperature { get; set; } = 213.15f;

        [Option("Maximum Growth Temperature", "In K. Vanilla is 368.15 K (95 C). The plant dies 95 K above this.", Format = "F2")]
        [Limit(73.15, 1273.15)]
        [JsonProperty]
        public float MaxGrowthTemperature { get; set; } = 368.15f;
    }
}
