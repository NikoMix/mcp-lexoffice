using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models
{
    public class PaginatedContactsResponse
    {
        [JsonPropertyName("content")]
        public List<Contact> Content { get; set; } = new List<Contact>();

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
        public int Number { get; set; }

        [JsonPropertyName("sort")]
        public List<SortOrder>? Sort { get; set; }
    }
}
