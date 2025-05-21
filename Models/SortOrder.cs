using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models
{
    public class SortOrder
    {
        [JsonPropertyName("property")]
        public string? Property { get; set; }

        [JsonPropertyName("direction")]
        public string? Direction { get; set; } // "ASC" or "DESC"

        [JsonPropertyName("ignoreCase")]
        public bool IgnoreCase { get; set; }

        [JsonPropertyName("nullHandling")]
        public string? NullHandling { get; set; } // e.g., "NATIVE"

        [JsonPropertyName("ascending")]
        public bool Ascending { get; set; }

        [JsonPropertyName("descending")]
        public bool Descending { get; set; }
    }
}
