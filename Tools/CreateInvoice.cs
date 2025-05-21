using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MixMedia.MCP.LexOffice.Models;
using ModelContextProtocol.Server;

namespace MixMedia.MCP.LexOffice.Tools;

public class CreateInvoiceRequest
{
    public bool Archived { get; set; } = false;
    public DateTimeOffset VoucherDate { get; set; }
    public InvoiceAddress Address { get; set; } = new();
    public List<InvoiceLineItem> LineItems { get; set; } = new();
    public InvoiceTotalPrice TotalPrice { get; set; } = new();
    public InvoiceTaxConditions TaxConditions { get; set; } = new();
    public InvoicePaymentConditions PaymentConditions { get; set; } = new();
    public InvoiceShippingConditions ShippingConditions { get; set; } = new();
    public string Title { get; set; } = string.Empty;
    public string Introduction { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    // Optional: public InvoiceXRechnung? XRechnung { get; set; }
}

public class InvoiceLineItem
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string? UnitName { get; set; }
    public InvoiceUnitPrice? UnitPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public string? Description { get; set; }
    public Guid? Id { get; set; }
}

public class InvoiceUnitPrice
{
    public string Currency { get; set; } = string.Empty;
    public decimal? NetAmount { get; set; }
    public decimal? GrossAmount { get; set; }
    public decimal TaxRatePercentage { get; set; }
}

public class InvoiceTotalPrice
{
    public string Currency { get; set; } = string.Empty;
}

public class InvoiceTaxConditions
{
    public string TaxType { get; set; } = string.Empty;
}

public class InvoicePaymentConditions
{
    public string PaymentTermLabel { get; set; } = string.Empty;
    public int PaymentTermDuration { get; set; }
    public InvoicePaymentDiscountConditions PaymentDiscountConditions { get; set; } = new();
}

public class InvoicePaymentDiscountConditions
{
    public decimal DiscountPercentage { get; set; }
    public int DiscountRange { get; set; }
}

public class InvoiceShippingConditions
{
    public DateTimeOffset ShippingDate { get; set; }
    public string ShippingType { get; set; } = string.Empty;
    public DateTimeOffset? ShippingEndDate { get; set; }
}

[McpServerToolType]
public static class CreateInvoiceTool
{
    [McpServerTool, Description("Creates a new Lexoffice invoice. Pass a CreateInvoiceRequest object as input. Optionally set finalize=true.")]
    public static async Task<string> CreateInvoiceAsync(
        IServiceProvider serviceProvider,
        CreateInvoiceRequest request,
        [Description("Set to true to finalize the invoice on creation")] bool finalize = false)
    {
        var httpFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var client = httpFactory.CreateClient("LexOfficeClient");
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var url = "invoices" + (finalize ? "?finalize=true" : "");
        var response = await client.PostAsync(url, content);
        switch ((int)response.StatusCode)
        {
            case 200:
            case 201:
            case 202:
                return await response.Content.ReadAsStringAsync();
            case 204:
                return "No Content: The resource was successfully created or no content to return.";
            case 400:
                throw new InvalidOperationException($"Bad Request: {await response.Content.ReadAsStringAsync()}");
            case 401:
                throw new UnauthorizedAccessException("Unauthorized: Action requires user authentication.");
            case 402:
                throw new InvalidOperationException("Payment Required: Action not accessible due to a lexoffice contract issue.");
            case 403:
                throw new UnauthorizedAccessException("Forbidden: Insufficient scope or access rights in lexoffice.");
            case 404:
                throw new InvalidOperationException("Not Found: Requested resource does not exist (anymore).");
            case 405:
                throw new InvalidOperationException("Not Allowed: Method not allowed on resource.");
            case 406:
                throw new InvalidOperationException($"Not Acceptable: {await response.Content.ReadAsStringAsync()}");
            case 409:
                throw new InvalidOperationException("Conflict: Request not allowed due to the current state of the resource.");
            case 415:
                throw new InvalidOperationException("Unsupported Media Type: Only application/json is supported.");
            case 429:
                throw new InvalidOperationException("Too Many Requests: Rate limit exceeded. Please retry later.");
            case 500:
                throw new InvalidOperationException("Server Error: Internal server error.");
            case 501:
                throw new InvalidOperationException("Not Implemented: Requested HTTP operation not supported.");
            case 503:
                throw new InvalidOperationException("Service Unavailable: Unable to handle the request temporarily.");
            case 504:
                throw new InvalidOperationException("Gateway Timeout: The server did not answer before the request timeout.");
            default:
                throw new InvalidOperationException($"Unexpected HTTP status code: {(int)response.StatusCode} - {response.ReasonPhrase}");
        }
    }
}
