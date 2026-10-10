using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace AbyssaliteToTungsten
{
    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        [Option("Balance heat production", "Abyssalite refined in the Metal Refinery sends three times the normal heat into its coolant. Abyssalite is common, so this keeps it from making Wolframite obsolete. The Metal Refinery description also gets a line about it.")]
        [JsonProperty]
        public bool BalanceHeatProduction { get; set; } = true;
    }
}
