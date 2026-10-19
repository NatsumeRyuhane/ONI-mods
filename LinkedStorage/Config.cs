using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace LinkedStorage
{
    [JsonObject]
    [ConfigFile("config.json", true, false)]
    [RestartRequired]
    public class Config
    {
        [Option("Separate Contents per Planetoid", "Linked buildings on different planetoids, and in different rocket interiors, keep separate contents. When off, every linked building of a kind shares one store across the whole save. Only matters with Spaced Out.")]
        [JsonProperty]
        public bool PerPlanetoid { get; set; } = true;

        [Option("Storage Bin Stock", "How much of each picked material a Linked Storage Bin keeps out of the shared store, in kg, for Duplicants to take.", Format = "F0")]
        [Limit(10.0, 5000.0)]
        [JsonProperty]
        public float SolidStockKg { get; set; } = 500.0f;

        [Option("Refrigerator Stock", "How much of each picked food a Linked Refrigerator keeps out of the shared store, in kg, for Duplicants to take.", Format = "F0")]
        [Limit(1.0, 1000.0)]
        [JsonProperty]
        public float FoodStockKg { get; set; } = 100.0f;

        [Option("Liquid Reservoir Stock", "How much of its output liquid a Linked Liquid Reservoir keeps out of the shared store, in kg, for its pipe and for Duplicants to take.", Format = "F0")]
        [Limit(20.0, 5000.0)]
        [JsonProperty]
        public float LiquidStockKg { get; set; } = 100.0f;

        [Option("Gas Reservoir Stock", "How much of its output gas a Linked Gas Reservoir keeps out of the shared store, in kg, for its pipe and for Duplicants to take.", Format = "F0")]
        [Limit(2.0, 1000.0)]
        [JsonProperty]
        public float GasStockKg { get; set; } = 10.0f;
    }
}
