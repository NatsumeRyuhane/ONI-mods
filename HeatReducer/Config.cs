using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace HeatReducer
{
    // Options shown in the game's mod menu and saved to config.json in the mod folder.
    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        [Option("Remove Output Temperature Floor",
            "Buildings release what they make at the temperature of their inputs, or of the building, instead of at a fixed minimum such as the Electrolyzer's 70 C.")]
        [JsonProperty]
        public bool RemoveOutputTemperatureFloor { get; set; } = true;
    }
}
