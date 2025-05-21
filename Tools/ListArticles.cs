using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using MixMedia.MCP.LexOffice.Models;

namespace MixMedia.MCP.LexOffice.Tools;

/// <summary>
/// Response model for a paged list of articles.
/// </summary>
public class ListArticlesResponse
{
    public List<Article> Content { get; set; } = new();
    public int TotalPages { get; set; }
    public int TotalElements { get; set; }
    public bool Last { get; set; }
    public List<ArticleSort>? Sort { get; set; }
    public int Size { get; set; }
    public int Number { get; set; }
    public bool First { get; set; }
    public int NumberOfElements { get; set; }
}

public class ArticleSort
{
    public string Direction { get; set; } = string.Empty;
    public string Property { get; set; } = string.Empty;
    public bool IgnoreCase { get; set; }
    public string NullHandling { get; set; } = string.Empty;
    public bool Ascending { get; set; }
}

[McpServerToolType]
public static class ListArticlesTool
{
    [McpServerTool, Description("Lists Lexoffice articles with optional filters and paging.")]
    public static async Task<ListArticlesResponse> ListArticlesAsync(
        IServiceProvider serviceProvider,
        [Description("Page number (zero-based)")] int page = 0,
        [Description("Filter by article number")] string? articleNumber = null,
        [Description("Filter by GTIN")] string? gtin = null,
        [Description("Filter by type (PRODUCT or SERVICE)")] string? type = null,
        CancellationToken cancellationToken = default)
    {
        var httpFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var client = httpFactory.CreateClient("LexOfficeClient");
        var url = $"articles?page={page}";
        if (!string.IsNullOrWhiteSpace(articleNumber))
            url += $"&articleNumber={Uri.EscapeDataString(articleNumber)}";
        if (!string.IsNullOrWhiteSpace(gtin))
            url += $"&gtin={Uri.EscapeDataString(gtin)}";
        if (!string.IsNullOrWhiteSpace(type))
            url += $"&type={Uri.EscapeDataString(type)}";
        var response = await client.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Failed to list articles: {response.StatusCode} - {error}");
        }
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<ListArticlesResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}
