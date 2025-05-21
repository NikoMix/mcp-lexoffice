using System;
using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models
{
    public class InvoicePaymentRequest
    {
        [JsonPropertyName("paymentAmount")]
        public decimal PaymentAmount { get; set; }

        [JsonPropertyName("paymentDate")] // Format "YYYY-MM-DD"
        public string PaymentDate { get; set; } = string.Empty;

        [JsonPropertyName("paymentMethod")]
        public string? PaymentMethod { get; set; } // e.g. "cash", "bankTransfer"
    }
}
