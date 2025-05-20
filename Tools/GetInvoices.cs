using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using System.Text.Json;

namespace MixMedia.MCP.LexOffice.Tools;

[McpServerToolType]
public static class GetInvoicesTool
{
    [McpServerTool, Description("Gets a list of Lexoffice invoices. Optional: pass a status filter (e.g. 'draft', 'open', 'paid', 'canceled').")]
    public static async Task<string> GetInvoicesAsync(IServiceProvider serviceProvider, [Description("Invoice status filter")] string? status = null)
    {
        var httpFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var client = httpFactory.CreateClient("LexOfficeClient");
        var url = "invoices";
        if (!string.IsNullOrWhiteSpace(status))
            url += $"?status={Uri.EscapeDataString(status)}";
        var response = await client.GetAsync(url);
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
