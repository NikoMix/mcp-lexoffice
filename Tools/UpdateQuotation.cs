using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using Polly;
using Polly.Retry;

namespace MixMedia.MCP.LexOffice.Tools;

public class UpdateQuotationRequest
{
    public Guid Id { get; set; }
    public int Version { get; set; }
    // Add other updatable fields as needed
}

[McpServerToolType]
public static class UpdateQuotationTool
{
    [McpServerTool, Description("Updates a Lexoffice quotation with optimistic locking. Retries on version conflict.")]
    public static async Task<string> UpdateQuotationAsync(IServiceProvider serviceProvider, UpdateQuotationRequest request, CancellationToken cancellationToken = default)
    {
        var httpFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var client = httpFactory.CreateClient("LexOfficeClient");
        var retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => r.StatusCode == System.Net.HttpStatusCode.Conflict)
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                async (result, timespan, retryCount, context) =>
                {
                    // On 409, fetch latest version and update request.Version
                    var getResp = await client.GetAsync($"quotations/{request.Id}", cancellationToken);
                    getResp.EnsureSuccessStatusCode();
                    var json = await getResp.Content.ReadAsStringAsync(cancellationToken);
                    var latest = JsonSerializer.Deserialize<Models.GetQuotationResponse>(json);
                    if (latest != null)
                        request.Version = latest.Version;
                });

        var putJson = JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var content = new StringContent(putJson, Encoding.UTF8, "application/json");
        var response = await retryPolicy.ExecuteAsync(() => client.PutAsync($"quotations/{request.Id}", content, cancellationToken));

        switch ((int)response.StatusCode)
        {
            case 200:
            case 201:
            case 202:
                return await response.Content.ReadAsStringAsync(cancellationToken);
            case 204:
                return "No Content: The resource was successfully updated or no content to return.";
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
                throw new InvalidOperationException("Conflict: Request not allowed due to the current state of the resource after retries.");
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
