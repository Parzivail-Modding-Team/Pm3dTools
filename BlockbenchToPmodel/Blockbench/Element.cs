using Newtonsoft.Json;

namespace BlockbenchToPmodel.Blockbench
{
    internal class Element
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("from")] public float[] From { get; set; }
        [JsonProperty("to")] public float[] To { get; set; }
        [JsonProperty("faces")] public Faces Faces { get; set; }
        [JsonProperty("rotation")] public Rotation Rotation { get; set; }
        [JsonProperty("shade")] public bool Shade { get; set; }
        [JsonProperty("quadData")] public QuadData QuadData { get; set; }
    }
}