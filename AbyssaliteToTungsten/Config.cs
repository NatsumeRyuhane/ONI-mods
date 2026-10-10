using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace AbyssaliteToTungsten
{
    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        [Option("Triple Coolant Heat", "Abyssalite refined in the Metal Refinery sends three times the normal heat into its coolant. The Metal Refinery description also gets a line about it.")]
        [JsonProperty]
        public bool TripleCoolantHeat { get; set; } = false;
    }
}
