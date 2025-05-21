using System;
using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models
{
    // Represents a summary of a voucher, typically from a list endpoint like /voucherlist
    public class InvoiceSummary 
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("voucherType")] // e.g., "invoice", "salesinvoice", "purchaseinvoice"
        public string VoucherType { get; set; } = string.Empty;

        [JsonPropertyName("voucherStatus")] // e.g., "draft", "open", "paid", "paidoff", "voided"
        public string VoucherStatus { get; set; } = string.Empty;

        [JsonPropertyName("voucherNumber")]
        public string VoucherNumber { get; set; } = string.Empty;

        [JsonPropertyName("voucherDate")]
        public DateTime VoucherDate { get; set; }

        [JsonPropertyName("updatedDate")]
        public DateTime UpdatedDate { get; set; }

        [JsonPropertyName("dueDate")]
        public DateTime? DueDate { get; set; }

        [JsonPropertyName("contactId")]
        public Guid? ContactId { get; set; }

        [JsonPropertyName("contactName")]
        public string? ContactName { get; set; }

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("openAmount")]
        public decimal? OpenAmount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "EUR";

        [JsonPropertyName("archived")]
        public bool Archived { get; set; }
    }
}
