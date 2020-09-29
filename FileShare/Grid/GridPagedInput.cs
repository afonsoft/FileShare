using Newtonsoft.Json;
using System.Collections.Generic;

namespace FileShare.Grid
{
    public class GridPagedInput
    {
        [JsonProperty("current")]
        public int Current { get; set; } = 1;

        [JsonProperty("rowCount")]
        public int RowCount { get; set; } = 10;

        [JsonProperty("id")]
        public string Id { get; set; } = "";

        [JsonProperty("searchPhrase")]
        public string SearchPhrase { get; set; } = "";

        [JsonProperty("sort")]
        public Dictionary<string, string> Sort { get; set; }
    }
}
