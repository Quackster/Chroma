using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extractor.Json
{
    public class JsonFurniData
    {
        public string type { get; set; }
        public Visualizations visualization { get; set; }
        public Logic logic { get; set; }
        public Dictionary<string, Asset> assets { get; set; }
    }

    public class Logic
    {
        public string type { get; set; }
        public Dictionary<string, string> dimensions { get; set; }
        public List<string> directions { get; set; }
    }

    public class Asset
    {
        public string name { get; set; }
        public string x { get; set; }
        public string y { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, Required = Required.AllowNull)]
        public string source { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, Required = Required.AllowNull)]
        public string flipH { get; set; }
    }
}
