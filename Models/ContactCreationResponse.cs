using System;
using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models
{
    public class ContactCreationResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("resourceUri")]
        public string ResourceUri { get; set; } = string.Empty;

        [JsonPropertyName("createdDate")]
        public DateTimeOffset CreatedDate { get; set; }

        [JsonPropertyName("updatedDate")]
        public DateTimeOffset UpdatedDate { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }
    }
}
