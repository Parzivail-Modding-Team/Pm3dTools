using Newtonsoft.Json;

namespace BlockbenchToPmodel.Blockbench
{
    internal class Faces
    {
        [JsonProperty("north")] public Face North { get; set; }
        [JsonProperty("east")] public Face East { get; set; }
        [JsonProperty("south")] public Face South { get; set; }
        [JsonProperty("west")] public Face West { get; set; }
        [JsonProperty("down")] public Face Down { get; set; }
        [JsonProperty("up")] public Face Up { get; set; }
    }
}