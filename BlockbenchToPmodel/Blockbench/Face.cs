using Newtonsoft.Json;

namespace BlockbenchToPmodel.Blockbench
{
    internal class Face
    {
        [JsonProperty("uv")] public float[] Uv { get; set; }
        [JsonProperty("texture")] public string Texture { get; set; }
    }
}