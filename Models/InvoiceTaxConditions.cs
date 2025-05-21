using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models;

public class InvoiceTaxConditions
{
    [JsonPropertyName("taxType")]
    public string TaxType { get; set; } = string.Empty;
}
