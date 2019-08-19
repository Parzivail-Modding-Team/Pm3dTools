using Newtonsoft.Json;

namespace BlockbenchToPmodel.Blockbench
{
    internal class Rotation
    {
        [JsonProperty("angle")] public int Angle { get; set; }
        [JsonProperty("axis")] public string Axis { get; set; }
        [JsonProperty("origin")] public float[] Origin { get; set; }
    }
}