using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MixMedia.MCP.LexOffice.Models;
using System.Web;
using System.Collections.Generic;
using System;

namespace MixMedia.MCP.LexOffice.Services
{
    public class LexOfficeApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BaseUrl = "https://api.lexoffice.io/v1";

        public LexOfficeApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["LexOfficeApiKey"] ?? throw new InvalidOperationException("LexOffice API key not found in configuration.");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<PageableResponse<InvoiceSummary>?> ListInvoicesAsync(
            string voucherStatus = "open,paid,paidoff,voided", 
            string voucherType = "invoice,salesinvoice,purchaseinvoice", 
            int page = 0, 
            int size = 100, 
            string? contactId = null,
            DateTime? voucherDateFrom = null,
            DateTime? voucherDateTo = null,
            string? voucherNumber = null,
            string? sortProperty = null, 
            string? sortDirection = null
            )
        {
            var queryBuilder = HttpUtility.ParseQueryString(string.Empty);
            queryBuilder["voucherStatus"] = voucherStatus;
            queryBuilder["voucherType"] = voucherType;
            queryBuilder["page"] = page.ToString();
            queryBuilder["size"] = size.ToString();

            if (!string.IsNullOrEmpty(contactId))
            {
                queryBuilder["contactId"] = contactId;
            }
            if (voucherDateFrom.HasValue)
            {
                queryBuilder["voucherDateFrom"] = voucherDateFrom.Value.ToString("yyyy-MM-dd");
            }
            if (voucherDateTo.HasValue)
            {
                queryBuilder["voucherDateTo"] = voucherDateTo.Value.ToString("yyyy-MM-dd");
            }
            if (!string.IsNullOrEmpty(voucherNumber))
            {
                queryBuilder["voucherNumber"] = voucherNumber;
            }
            if (!string.IsNullOrEmpty(sortProperty) && !string.IsNullOrEmpty(sortDirection))
            {
                queryBuilder["sort"] = $"{sortProperty},{sortDirection}";
            }

            string requestUri = $"{BaseUrl}/voucherlist?{queryBuilder.ToString()}";

            var response = await _httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is an error code.

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PageableResponse<InvoiceSummary>>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<InvoiceDetail?> GetInvoiceAsync(Guid invoiceId)
        {
            string requestUri = $"{BaseUrl}/invoices/{invoiceId}";

            var response = await _httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<InvoiceDetail>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
