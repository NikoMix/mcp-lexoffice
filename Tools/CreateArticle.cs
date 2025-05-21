using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using MixMedia.MCP.LexOffice.Models;

namespace MixMedia.MCP.LexOffice.Tools;

/// <summary>
/// Request model for creating a Lexoffice article.
/// </summary>
public class CreateArticleRequest
{
    /// <summary>Title of the article.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Type of the article. Possible values: PRODUCT, SERVICE.</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>Unit name of the article.</summary>
    public string UnitName { get; set; } = string.Empty;
    /// <summary>Article number (optional).</summary>
    public string? ArticleNumber { get; set; }
    /// <summary>Price details of the article.</summary>
    public ArticlePrice Price { get; set; } = new();
}

[McpServerToolType]
public static class CreateArticleTool
{
    [McpServerTool, Description("Creates a new Lexoffice article. Pass a CreateArticleRequest object as input.")]
    public static async Task<string> CreateArticleAsync(
        IServiceProvider serviceProvider,
        CreateArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var client = httpFactory.CreateClient("LexOfficeClient");
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("articles", content, cancellationToken);
        switch ((int)response.StatusCode)
        {
            case 200:
            case 201:
            case 202:
                return await response.Content.ReadAsStringAsync(cancellationToken);
            case 204:
                return "No Content: The resource was successfully created or no content to return.";
            case 400:
                throw new InvalidOperationException($"Bad Request: {await response.Content.ReadAsStringAsync(cancellationToken)}");
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
                throw new InvalidOperationException($"Not Acceptable: {await response.Content.ReadAsStringAsync(cancellationToken)}");
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
