using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models
{
    public class PaymentConditionItem
    {
        [JsonPropertyName("paymentTermLabel")]
        public string? PaymentTermLabel { get; set; }

        [JsonPropertyName("paymentTermLabelTemplate")]
        public string? PaymentTermLabelTemplate { get; set; } // Read-only

        [JsonPropertyName("paymentTermDuration")]
        public int? PaymentTermDuration { get; set; }

        [JsonPropertyName("paymentDiscountLabel")]
        public string? PaymentDiscountLabel { get; set; }

        [JsonPropertyName("paymentDiscountLabelTemplate")]
        public string? PaymentDiscountLabelTemplate { get; set; } // Read-only

        [JsonPropertyName("paymentDiscountDuration")]
        public int? PaymentDiscountDuration { get; set; }

        [JsonPropertyName("paymentDiscountPercentage")]
        public decimal? PaymentDiscountPercentage { get; set; }
    }
}
