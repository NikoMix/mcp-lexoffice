using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace MixMedia.MCP.LexOffice.Tools;

public class CreateQuotationRequest
{
    // Add only a few representative properties for brevity. Expand as needed.
    public string ContactId { get; set; } = string.Empty;
    public QuotationLineItem[] LineItems { get; set; } = Array.Empty<QuotationLineItem>();
    public string? Introduction { get; set; }
    public string? Remark { get; set; }
    public int Version { get; set; } = 0; // Always 0 for POST per Lexoffice optimistic locking
    // Add more properties as needed from the Lexoffice API docs
}

public class QuotationLineItem
{
    public string Type { get; set; } = "custom";
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? UnitName { get; set; }
    // Add more fields as needed
}

[McpServerToolType]
public static class CreateQuotationTool
{
    [McpServerTool, Description("Creates a new Lexoffice quotation. Pass a CreateQuotationRequest object as input.")]
    public static async Task<string> CreateQuotationAsync(IServiceProvider serviceProvider, CreateQuotationRequest request)
    {
        var httpFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var client = httpFactory.CreateClient("LexOfficeClient");
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("quotations", content);
        switch ((int)response.StatusCode)
        {
            case 200:
            case 201:
            case 202:
                return await response.Content.ReadAsStringAsync();
            case 204:
                return "No Content: The resource was successfully deleted or no content to return.";
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
