using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models;

public class InvoiceTotalPrice
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "EUR";
    [JsonPropertyName("totalNetAmount")]
    public decimal? TotalNetAmount { get; set; }
    [JsonPropertyName("totalGrossAmount")]
    public decimal? TotalGrossAmount { get; set; }
    [JsonPropertyName("totalTaxAmount")]
    public decimal? TotalTaxAmount { get; set; }
}
