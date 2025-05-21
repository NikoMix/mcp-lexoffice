using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models
{
    // Depends on InvoiceAddress, LineItem, TotalPrice, TaxConditions, PaymentConditions, 
    // ShippingConditions, InvoiceContactLink, InvoiceContactPersonLink, InvoiceFiles being accessible 
    // (e.g., defined in InvoiceDetail.cs or separate files in MixMedia.MCP.LexOffice.Models)

    public class QuotationDetail
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("organizationId")]
        public Guid OrganizationId { get; set; } // Usually read-only

        [JsonPropertyName("createdDate")]
        public DateTime CreatedDate { get; set; } // Read-only

        [JsonPropertyName("updatedDate")]
        public DateTime UpdatedDate { get; set; } // Read-only

        [JsonPropertyName("version")]
        public int Version { get; set; } // Required for updates

        [JsonPropertyName("language")]
        public string Language { get; set; } = "de";

        [JsonPropertyName("archived")]
        public bool Archived { get; set; } // Read-only

        [JsonPropertyName("voucherStatus")]
        public string VoucherStatus { get; set; } = string.Empty; // e.g. "draft", "open", "accepted", "rejected", "invoiced"

        [JsonPropertyName("voucherNumber")]
        public string? VoucherNumber { get; set; } // Usually set by lexoffice

        [JsonPropertyName("voucherDate")]
        public DateTime VoucherDate { get; set; }

        [JsonPropertyName("expirationDate")] // Specific to quotations
        public DateTime? ExpirationDate { get; set; }

        [JsonPropertyName("address")] // Main recipient address
        public InvoiceAddress Address { get; set; } = new();

        [JsonPropertyName("lineItems")]
        public List<LineItem> LineItems { get; set; } = new();

        [JsonPropertyName("totalPrice")]
        public TotalPrice TotalPrice { get; set; } = new();

        [JsonPropertyName("taxConditions")]
        public TaxConditions TaxConditions { get; set; } = new();

        [JsonPropertyName("paymentConditions")]
        public PaymentConditions? PaymentConditions { get; set; }

        [JsonPropertyName("shippingConditions")]
        public ShippingConditions? ShippingConditions { get; set; }
        
        [JsonPropertyName("contact")] // Link to the contact
        public InvoiceContactLink Contact { get; set; } = new();

        [JsonPropertyName("contactPerson")]
        public InvoiceContactPersonLink? ContactPerson { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("introduction")]
        public string? Introduction { get; set; }

        [JsonPropertyName("remark")]
        public string? Remark { get; set; }

        [JsonPropertyName("files")]
        public InvoiceFiles? Files { get; set; } // Read-only, contains documentFileId
    }
}
