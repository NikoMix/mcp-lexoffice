using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models
{
    public class InvoiceAddress
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("supplement")]
        public string? Supplement { get; set; }

        [JsonPropertyName("street")]
        public string? Street { get; set; }

        [JsonPropertyName("zip")]
        public string? Zip { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("countryCode")]
        public string? CountryCode { get; set; }
    }

    public class UnitPrice
    {
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "EUR";

        [JsonPropertyName("netAmount")]
        public decimal NetAmount { get; set; }

        [JsonPropertyName("grossAmount")]
        public decimal GrossAmount { get; set; }

        [JsonPropertyName("taxRatePercentage")]
        public decimal TaxRatePercentage { get; set; }
    }

    public class LineItem
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }

        [JsonPropertyName("unitName")]
        public string? UnitName { get; set; }

        [JsonPropertyName("unitPrice")]
        public UnitPrice UnitPrice { get; set; } = new();

        [JsonPropertyName("discountPercentage")]
        public decimal? DiscountPercentage { get; set; } = 0;

        [JsonPropertyName("lineItemAmount")]
        public decimal LineItemAmount { get; set; }
    }

    public class TotalPrice
    {
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "EUR";

        [JsonPropertyName("totalNetAmount")]
        public decimal TotalNetAmount { get; set; }

        [JsonPropertyName("totalGrossAmount")]
        public decimal TotalGrossAmount { get; set; }

        [JsonPropertyName("totalTaxAmount")]
        public decimal TotalTaxAmount { get; set; }

        [JsonPropertyName("taxAmountsPerTaxRate")]
        public List<TaxAmountPerRate>? TaxAmountsPerTaxRate { get; set; }
    }

    public class TaxAmountPerRate
    {
        [JsonPropertyName("taxRatePercentage")]
        public decimal TaxRatePercentage { get; set; }

        [JsonPropertyName("taxAmount")]
        public decimal TaxAmount { get; set; }
    }

    public class TaxConditions
    {
        [JsonPropertyName("taxType")]
        public string TaxType { get; set; } = string.Empty; // "net", "gross", "vatfree"

        [JsonPropertyName("taxTypeNote")]
        public string? TaxTypeNote { get; set; }
    }

    public class PaymentConditions
    {
        [JsonPropertyName("paymentTermLabel")]
        public string? PaymentTermLabel { get; set; }

        [JsonPropertyName("paymentTermDuration")]
        public int? PaymentTermDuration { get; set; }

        [JsonPropertyName("paymentDiscountTarget")]
        public DateTime? PaymentDiscountTarget { get; set; }

        [JsonPropertyName("paymentDiscountPercentage")]
        public decimal? PaymentDiscountPercentage { get; set; }
        
        [JsonPropertyName("paymentStatus")]
        public string? PaymentStatus { get; set; } 
    }

    public class ShippingConditions
    {
        [JsonPropertyName("shippingDate")]
        public DateTime? ShippingDate { get; set; }

        [JsonPropertyName("shippingEndDate")]
        public DateTime? ShippingEndDate { get; set; }

        [JsonPropertyName("shippingType")]
        public string? ShippingType { get; set; }
    }

    public class InvoiceContactLink
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
    }
    
    public class InvoiceContactPersonLink
    {
        [JsonPropertyName("contactPersonId")]
        public Guid ContactPersonId { get; set; }
    }

    public class InvoiceFiles
    {
        [JsonPropertyName("documentFileId")]
        public Guid DocumentFileId { get; set; }
    }

    public class InvoiceDetail
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("organizationId")]
        public Guid OrganizationId { get; set; }

        [JsonPropertyName("createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonPropertyName("updatedDate")]
        public DateTime UpdatedDate { get; set; }

        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; } = "de";

        [JsonPropertyName("archived")]
        public bool Archived { get; set; }

        [JsonPropertyName("voucherStatus")]
        public string VoucherStatus { get; set; } = string.Empty;

        [JsonPropertyName("voucherNumber")]
        public string? VoucherNumber { get; set; }

        [JsonPropertyName("voucherDate")]
        public DateTime VoucherDate { get; set; }

        [JsonPropertyName("dueDate")]
        public DateTime DueDate { get; set; }

        [JsonPropertyName("address")]
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

        [JsonPropertyName("closingInvoice")]
        public bool? ClosingInvoice { get; set; }

        [JsonPropertyName("claimedGrossAmount")]
        public decimal? ClaimedGrossAmount { get; set; }

        [JsonPropertyName("contact")]
        public InvoiceContactLink Contact { get; set; } = new();
        
        [JsonPropertyName("contactPerson")]
        public InvoiceContactPersonLink? ContactPerson { get; set; }

        [JsonPropertyName("taxOption")]
        public string? TaxOption { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("introduction")]
        public string? Introduction { get; set; }

        [JsonPropertyName("remark")]
        public string? Remark { get; set; }

        [JsonPropertyName("files")]
        public InvoiceFiles? Files { get; set; }
    }
}
