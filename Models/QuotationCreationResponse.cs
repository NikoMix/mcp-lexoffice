using System;
using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models
{
    public class QuotationCreationResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("resourceUri")]
        public string ResourceUri { get; set; } = string.Empty;

        [JsonPropertyName("createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonPropertyName("updatedDate")]
        public DateTime UpdatedDate { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }
    }
}
