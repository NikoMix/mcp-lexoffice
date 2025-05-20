using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace MixMedia.MCP.LexOffice.Tools;

[McpServerToolType]
public static class AttachFileToVoucherTool
{
    [McpServerTool, Description("Attaches a file to a Lexoffice voucher by its ID. Accepts a file name and byte array.")]
    public static async Task<string> AttachFileToVoucherAsync(
        IServiceProvider serviceProvider,
        [Description("The voucher ID")] Guid voucherId,
        [Description("The file name")] string fileName,
        [Description("The file content as a byte array")] byte[] fileContent)
    {
        var httpFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var client = httpFactory.CreateClient("LexOfficeClient");
        using var content = new MultipartFormDataContent();
        var fileContentContent = new ByteArrayContent(fileContent);
        fileContentContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
        content.Add(fileContentContent, "file", fileName);
        var response = await client.PostAsync($"vouchers/{voucherId}/files", content);
        switch ((int)response.StatusCode)
        {
            case 200:
            case 201:
            case 202:
                return await response.Content.ReadAsStringAsync();
            case 204:
                return "No Content: The file was successfully attached or no content to return.";
            case 400:
                throw new InvalidOperationException($"Bad Request: {await response.Content.ReadAsStringAsync()}");
            case 401:
                throw new UnauthorizedAccessException("Unauthorized: Action requires user authentication.");
            case 402:
                throw new InvalidOperationException("Payment Required: Action not accessible due to a lexoffice contract issue.");
            case 403:
                throw new UnauthorizedAccessException("Forbidden: Insufficient scope or access rights in lexoffice.");
            case 404:
                throw new InvalidOperationException("Not Found: Requested voucher does not exist (anymore).");
            case 405:
                throw new InvalidOperationException("Not Allowed: Method not allowed on resource.");
            case 406:
                throw new InvalidOperationException($"Not Acceptable: {await response.Content.ReadAsStringAsync()}");
            case 409:
                throw new InvalidOperationException("Conflict: Request not allowed due to the current state of the resource.");
            case 415:
                throw new InvalidOperationException("Unsupported Media Type: Only application/pdf is supported for file uploads.");
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
