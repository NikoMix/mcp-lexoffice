using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models;

public class InvoicePaymentConditions
{
    [JsonPropertyName("paymentTermLabel")]
    public string? PaymentTermLabel { get; set; }
    [JsonPropertyName("paymentTermDuration")]
    public int? PaymentTermDuration { get; set; }
    [JsonPropertyName("paymentDiscountConditions")]
    public InvoicePaymentDiscountConditions? PaymentDiscountConditions { get; set; }
}
