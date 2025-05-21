using System;
using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models;

public class InvoiceShippingConditions
{
    [JsonPropertyName("shippingDate")]
    public DateTimeOffset? ShippingDate { get; set; }
    [JsonPropertyName("shippingType")]
    public string? ShippingType { get; set; }
    [JsonPropertyName("shippingEndDate")]
    public DateTimeOffset? ShippingEndDate { get; set; }
}
