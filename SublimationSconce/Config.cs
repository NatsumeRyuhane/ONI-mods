using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace SublimationSconce
{
    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        [Option("Storage Capacity", "Most a sconce holds, in kg. The vanilla Oxylite Sconce holds 240 kg. Each sconce also has a slider to hold less.", Format = "F0")]
        [Limit(10.0, 2000.0)]
        [JsonProperty]
        public float CapacityKg { get; set; } = 240.0f;

        [Option("Oxylite Sublimation Rate", "Items in a sconce sublimate as fast as Oxylite would. When off, they keep their vanilla rate, so the sconce only stores them.")]
        [JsonProperty]
        public bool MatchOxyliteRate { get; set; } = true;
    }
}
