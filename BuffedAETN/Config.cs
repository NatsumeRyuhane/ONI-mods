using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace BuffedAETN
{
    // Options shown in the game's mod menu and saved to config.json in the mod folder. Each AETN sets its own cooling
    // power and minimum temperature with sliders; these options set the slider limit and the Hydrogen it costs. The
    // Hydrogen default matches vanilla (80 kW for 10 g/s, so 8 kDTU per gram).
    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        [Option("Maximum Cooling Power", "The top of each AETN's cooling power slider, in kW. Higher cooling power also raises the lowest minimum temperature it can be set to.", Format = "F0")]
        [Limit(80.0, 10000.0)]
        [JsonProperty]
        public float MaxCoolingPower { get; set; } = 2000.0f;

        [Option("Cooling per Gram of Hydrogen", "Heat removed for each gram of Hydrogen burned, in kDTU per gram. Higher uses less Hydrogen. The default matches vanilla (80 kW for 10 g/s). 0 means the AETN needs no Hydrogen.", Format = "F1")]
        [Limit(0.0, 10000.0)]
        [JsonProperty]
        public float CoolingKdtuPerGram { get; set; } = AdjustableAetn.VanillaCoolingKdtuPerGram;
    }
}
