using Newtonsoft.Json;
using System.Collections.Generic;

namespace FileShare.Grid
{
    public class GridPagedOutput<T> where T : class, new()
    {
        public GridPagedOutput(IEnumerable<T> value)
        {
            if (value == null)
                value = new List<T>();
            this.Rows = value;
        }

        [JsonProperty("current")]
        public int Current { get; set; }

        [JsonProperty("rowCount")]
        public int RowCount { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("rows")]
        public IEnumerable<T> Rows { get; set; }
    }
}
