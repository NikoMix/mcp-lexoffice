using System;
using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models;

public class InvoiceLineItem
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("quantity")]
    public decimal? Quantity { get; set; }
    [JsonPropertyName("unitName")]
    public string? UnitName { get; set; }
    [JsonPropertyName("unitPrice")]
    public InvoiceUnitPrice? UnitPrice { get; set; }
    [JsonPropertyName("discountPercentage")]
    public decimal? DiscountPercentage { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("id")]
    public Guid? Id { get; set; }
}
