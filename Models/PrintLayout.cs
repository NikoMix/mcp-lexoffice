using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models
{
    public class PrintLayout
    {
        [JsonPropertyName("printLayoutId")]
        public string PrintLayoutId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("default")]
        public bool IsDefault { get; set; } // Mapped from "default"
    }
}
