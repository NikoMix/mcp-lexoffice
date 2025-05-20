using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models;

public class GetQuotationResponse
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset UpdatedDate { get; set; }
    public int Version { get; set; }
    public string Language { get; set; } = string.Empty;
    public bool Archived { get; set; }
    public string VoucherStatus { get; set; } = string.Empty;
    public string VoucherNumber { get; set; } = string.Empty;
    public DateTimeOffset VoucherDate { get; set; }
    public DateTimeOffset ExpirationDate { get; set; }
    public QuotationAddress Address { get; set; } = new();
    public List<QuotationLineItem> LineItems { get; set; } = new();
    public QuotationTotalPrice TotalPrice { get; set; } = new();
    public List<QuotationTaxAmount> TaxAmounts { get; set; } = new();
    public QuotationTaxConditions TaxConditions { get; set; } = new();
    public QuotationPaymentConditions PaymentConditions { get; set; } = new();
    public string Introduction { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public QuotationFiles Files { get; set; } = new();
    public string Title { get; set; } = string.Empty;
}

public class QuotationAddress
{
    public Guid ContactId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
}

public class QuotationLineItem
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public QuotationUnitPrice UnitPrice { get; set; } = new();
    public decimal DiscountPercentage { get; set; }
    public decimal LineItemAmount { get; set; }
    public List<QuotationLineItem>? SubItems { get; set; }
    public bool Alternative { get; set; }
    public bool Optional { get; set; }
}

public class QuotationUnitPrice
{
    public string Currency { get; set; } = string.Empty;
    public decimal NetAmount { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal TaxRatePercentage { get; set; }
}

public class QuotationTotalPrice
{
    public string Currency { get; set; } = string.Empty;
    public decimal TotalNetAmount { get; set; }
    public decimal TotalGrossAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
}

public class QuotationTaxAmount
{
    public decimal TaxRatePercentage { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetAmount { get; set; }
}

public class QuotationTaxConditions
{
    public string TaxType { get; set; } = string.Empty;
}

public class QuotationPaymentConditions
{
    public string PaymentTermLabel { get; set; } = string.Empty;
    public string PaymentTermLabelTemplate { get; set; } = string.Empty;
    public int PaymentTermDuration { get; set; }
    public QuotationPaymentDiscountConditions PaymentDiscountConditions { get; set; } = new();
}

public class QuotationPaymentDiscountConditions
{
    public decimal DiscountPercentage { get; set; }
    public int DiscountRange { get; set; }
}

public class QuotationFiles
{
    public Guid DocumentFileId { get; set; }
}
