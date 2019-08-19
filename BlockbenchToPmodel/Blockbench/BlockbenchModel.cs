using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace BlockbenchToPmodel.Blockbench
{
    internal class BlockbenchModel
    {
        [JsonProperty("parent")] public string Parent { get; set; }
        [JsonProperty("credit")] public string Credit { get; set; }
        [JsonProperty("textures")] public Dictionary<string, string> Textures { get; set; }
        [JsonProperty("ambientocclusion")] public bool AmbientOcclusion { get; set; }
        [JsonProperty("elements")] public Element[] Elements { get; set; }

        public static BlockbenchModel FromFile(string filename)
        {
            return JsonConvert.DeserializeObject<BlockbenchModel>(File.ReadAllText(filename));
        }
    }
}