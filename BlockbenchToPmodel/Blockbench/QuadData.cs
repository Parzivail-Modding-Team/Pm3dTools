using Newtonsoft.Json;

namespace BlockbenchToPmodel.Blockbench
{
    internal class QuadData
    {
        [JsonProperty("lit")] public bool Lit { get; set; }
        [JsonProperty("lightColor")] public int LightColor { get; set; }
    }
}