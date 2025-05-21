using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models
{
    public class PageableResponse<T>
    {
        [JsonPropertyName("content")]
        public List<T> Content { get; set; } = new List<T>();

        [JsonPropertyName("first")]
        public bool First { get; set; }

        [JsonPropertyName("last")]
        public bool Last { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("totalElements")]
        public int TotalElements { get; set; }

        [JsonPropertyName("numberOfElements")]
        public int NumberOfElements { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }

        [JsonPropertyName("number")]
        public int Number { get; set; } // Page number (0-indexed)

        [JsonPropertyName("sort")]
        public List<SortOrder>? Sort { get; set; }
    }
}
