
using MixMedia.MCP.LexOffice.Models;
using System;
using System.Collections.Generic;
using System.IO; // For Stream
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json; // For ReadFromJsonAsync, PostAsJsonAsync
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web; // For HttpUtility

namespace MixMedia.MCP.LexOffice.Services;

/// <summary>
/// Service to interact with the Lexoffice API.
/// Handles HTTP requests, authentication, and error handling.
/// </summary>
public class LexOfficeService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey; // Should be securely managed, e.g., via IConfiguration
    private const string BaseApiUrl = "https://api.lexoffice.io/v1";

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true, // For deserialization flexibility
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull // To omit null values in request payloads
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="LexOfficeService"/> class.
    /// </summary>
    /// <param name="httpClient">The HttpClient instance to use for requests. It's recommended to manage HttpClient lifetime via IHttpClientFactory.</param>
    /// <param name="apiKey">The Lexoffice API key.</param>
    public LexOfficeService(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));

        // Configure HttpClient instance (base address, default headers)
        // BaseAddress can be set here or by IHttpClientFactory
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// Creates a new contact in Lexoffice.
    /// The payload must adhere to Lexoffice API specifications:
    /// - Version must be 0.
    /// - At least one role (customer or vendor) must be defined.
    /// - Either company or person details must be provided, but not both.
    /// - If company is provided, company name is mandatory.
    /// - If person is provided, person last name is mandatory.
    /// - If addresses are provided, countryCode is mandatory for each address.
    /// - Max one of each: billing address, shipping address.
    /// - Max one of each email type (business, office, private, other).
    /// - Max one of each phone type (business, office, mobile, private, fax, other).
    /// - Max one contact person within a company for creation.
    /// </summary>
    /// <param name="contactPayload">The contact data to create. This object should be pre-validated or constructed carefully.</param>
    /// <returns>The response from Lexoffice after creating the contact, including the new contact's ID and URI.</returns>
    /// <exception cref="HttpRequestException">If the API request fails or returns an unsuccessful status code. The exception may contain details from the API response.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="contactPayload"/> is null.</exception>
    /// <exception cref="ArgumentException">If <paramref name="contactPayload"/> fails basic validation (e.g., version not 0, missing mandatory fields).</exception>
    public async Task<ContactCreationResponse?> CreateContactAsync(Contact contactPayload)
    {
        if (contactPayload == null) throw new ArgumentNullException(nameof(contactPayload));

        // Basic payload validation (more specific validation can be done by the caller or a dedicated validator)
        if (contactPayload.Version != 0) 
            throw new ArgumentException("Contact version must be 0 for creation.", nameof(contactPayload.Version));
        if (contactPayload.Roles.Customer == null && contactPayload.Roles.Vendor == null) 
            throw new ArgumentException("At least one role (customer or vendor) must be specified.", nameof(contactPayload.Roles));
        if (contactPayload.Company == null && contactPayload.Person == null) 
            throw new ArgumentException("Either company details or person details must be provided for the contact type.", "contactPayload.Company/Person");
        if (contactPayload.Company != null && contactPayload.Person != null) 
            throw new ArgumentException("Provide either company details or person details for the contact type, not both.", "contactPayload.Company/Person");
        if (contactPayload.Company != null && string.IsNullOrWhiteSpace(contactPayload.Company.Name)) 
            throw new ArgumentException("Company name is mandatory if company details are provided.", nameof(contactPayload.Company.Name));
        if (contactPayload.Person != null && string.IsNullOrWhiteSpace(contactPayload.Person.LastName)) 
            throw new ArgumentException("Person's last name is mandatory if person details are provided.", nameof(contactPayload.Person.LastName));
        if (contactPayload.Addresses?.Billing?.Any(a => string.IsNullOrWhiteSpace(a.CountryCode)) == true) 
            throw new ArgumentException("Country code is mandatory for all billing addresses.", "contactPayload.Addresses.Billing.CountryCode");
        if (contactPayload.Addresses?.Shipping?.Any(a => string.IsNullOrWhiteSpace(a.CountryCode)) == true) 
            throw new ArgumentException("Country code is mandatory for all shipping addresses.", "contactPayload.Addresses.Shipping.CountryCode");

        var response = await _httpClient.PostAsJsonAsync($"{BaseApiUrl}/contacts", contactPayload, _jsonSerializerOptions);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            // TODO: Implement more structured error parsing if Lexoffice provides detailed error objects
            throw new HttpRequestException($"Error creating contact. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}, Details: {errorContent}", null, response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<ContactCreationResponse>(_jsonSerializerOptions);
    }

    /// <summary>
    /// Retrieves a list of contacts from Lexoffice, with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="name">Filters by the name of the contact (person or company). Case-insensitive containment search.</param>
    /// <param name="customerNumber">Filters by the exact customer number.</param>
    /// <param name="vendorNumber">Filters by the exact vendor number.</param>
    /// <param name="email">Filters by any email address of the contact. Case-insensitive containment search.</param>
    /// <param name="page">The page number to retrieve (0-based). Defaults to 0 if not specified.</param>
    /// <param name="size">The number of contacts per page (min 1, max 1000). Defaults to 100 if not specified.</param>
    /// <param name="direction">The sort direction. Defaults to ASC if not specified.</param>
    /// <param name="sort">The property to sort by. Defaults to 'name' if not specified.</param>
    /// <param name="archived">Filters by archived status. If null, archived status is not filtered.</param>
    /// <returns>A <see cref="PaginatedContactsResponse"/> containing the list of contacts and pagination details.</returns>
    /// <exception cref="HttpRequestException">If the API request fails or returns an unsuccessful status code.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If page or size parameters are out of valid range.</exception>
    public async Task<PaginatedContactsResponse?> GetContactsAsync(
        string? name = null,
        string? customerNumber = null,
        string? vendorNumber = null,
        string? email = null,
        int? page = null,
        int? size = null,
        ContactSortDirection? direction = null,
        ContactSortField? sort = null,
        bool? archived = null)
    {
        var queryParameters = new Dictionary<string, string?>();

        if (!string.IsNullOrEmpty(name)) queryParameters["name"] = name;
        if (!string.IsNullOrEmpty(customerNumber)) queryParameters["customerNumber"] = customerNumber;
        if (!string.IsNullOrEmpty(vendorNumber)) queryParameters["vendorNumber"] = vendorNumber;
        if (!string.IsNullOrEmpty(email)) queryParameters["email"] = email;
        
        if (page.HasValue)
        {
            if (page.Value < 0) throw new ArgumentOutOfRangeException(nameof(page), "Page cannot be negative.");
            queryParameters["page"] = page.Value.ToString();
        }
        if (size.HasValue)
        {
            if (size.Value < 1 || size.Value > 1000) throw new ArgumentOutOfRangeException(nameof(size), "Size must be between 1 and 1000.");
            queryParameters["size"] = size.Value.ToString();
        }
        if (direction.HasValue) queryParameters["direction"] = direction.Value.ToString().ToUpperInvariant(); // API expects uppercase ASC/DESC
        if (sort.HasValue) queryParameters["sort"] = sort.Value.ToString(); // Enum to string directly, API expects camelCase
        if (archived.HasValue) queryParameters["archived"] = archived.Value.ToString().ToLowerInvariant(); // API expects "true" or "false"

        var queryStringParts = queryParameters
            .Where(kvp => kvp.Value != null)
            .Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"); // Ensure values are URL encoded
        
        var queryString = string.Join("&", queryStringParts);
        var requestUrl = string.IsNullOrEmpty(queryString) ? $"{BaseApiUrl}/contacts" : $"{BaseApiUrl}/contacts?{queryString}";

        var response = await _httpClient.GetAsync(requestUrl);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error fetching contacts. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}, Details: {errorContent}", null, response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<PaginatedContactsResponse>(_jsonSerializerOptions);
    }

    /// <summary>
    /// Retrieves a specific contact by its unique Lexoffice ID.
    /// </summary>
    /// <param name="contactId">The GUID of the contact to retrieve.</param>
    /// <returns>The <see cref="Contact"/> object if found; otherwise, null if the contact does not exist (HTTP 404).</returns>
    /// <exception cref="HttpRequestException">If the API request fails with a status code other than 404.</exception>
    /// <exception cref="ArgumentException">If <paramref name="contactId"/> is an empty GUID.</exception>
    public async Task<Contact?> GetContactByIdAsync(Guid contactId)
    {
        if (contactId == Guid.Empty) throw new ArgumentException("Contact ID cannot be an empty GUID.", nameof(contactId));

        var response = await _httpClient.GetAsync($"{BaseApiUrl}/contacts/{contactId}");

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null; // Contact not found, return null as per common practice
            }
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error fetching contact with ID {contactId}. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}, Details: {errorContent}", null, response.StatusCode);
        }
        return await response.Content.ReadFromJsonAsync<Contact>(_jsonSerializerOptions);
    }

    /// <summary>
    /// Updates an existing contact in Lexoffice.
    /// The payload must include the current 'version' of the contact being updated.
    /// It's recommended to fetch the contact first to get its current version before attempting an update.
    /// </summary>
    /// <param name="contactId">The GUID of the contact to update.</param>
    /// <param name="contactPayload">The contact data containing the updates. The 'Version' property must match the current version in Lexoffice.</param>
    /// <returns>The response from Lexoffice after updating the contact.</returns>
    /// <exception cref="HttpRequestException">If the API request fails or returns an unsuccessful status code.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="contactPayload"/> is null.</exception>
    /// <exception cref="ArgumentException">If <paramref name="contactId"/> is an empty GUID or if payload validation fails (e.g., missing version).</exception>
    public async Task<ContactUpdateResponse?> UpdateContactAsync(Guid contactId, Contact contactPayload)
    {
        if (contactId == Guid.Empty) throw new ArgumentException("Contact ID cannot be an empty GUID.", nameof(contactId));
        if (contactPayload == null) throw new ArgumentNullException(nameof(contactPayload));
        // The API requires the version field to be present and match the current version of the contact.
        // Consider adding a check for contactPayload.Version > 0 or similar if appropriate.

        var response = await _httpClient.PutAsJsonAsync($"{BaseApiUrl}/contacts/{contactId}", contactPayload, _jsonSerializerOptions);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error updating contact with ID {contactId}. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}, Details: {errorContent}", null, response.StatusCode);
        }

        return await response.Content.ReadFromJsonAsync<ContactUpdateResponse>(_jsonSerializerOptions);
    }


    // --- Invoices --- 

    /// <summary>
    /// Retrieves a list of invoices and other voucher types from Lexoffice using the /voucherlist endpoint.
    /// </summary>
    /// <param name="voucherStatus">Comma-separated list of voucher statuses (e.g., "open,paid"). Defaults to "open,paid,paidoff,voided".</param>
    /// <param name="voucherType">Comma-separated list of voucher types (e.g., "invoice,salesinvoice"). Defaults to "invoice,salesinvoice,purchaseinvoice".</param>
    /// <param name="contactId">Filter by contact ID.</param>
    /// <param name="voucherDateFrom">Filter by voucher date (from).</param>
    /// <param name="voucherDateTo">Filter by voucher date (to).</param>
    /// <param name="voucherNumber">Filter by specific voucher number.</param>
    /// <param name="page">Page number (0-indexed).</param>
    /// <param name="size">Number of items per page.</param>
    /// <param name="sortProperty">Property to sort by (e.g., "voucherNumber", "voucherDate").</param>
    /// <param name="sortDirection">Sort direction ("ASC" or "DESC").</param>
    /// <returns>A paginated response of invoice summaries.</returns>
    /// <exception cref="HttpRequestException">If the API request fails.</exception>
    public async Task<PageableResponse<InvoiceSummary>?> ListInvoicesAsync(
        string voucherStatus = "open,paid,paidoff,voided", 
        string voucherType = "invoice,salesinvoice,purchaseinvoice", 
        int page = 0, 
        int size = 100, 
        Guid? contactId = null,
        DateTime? voucherDateFrom = null,
        DateTime? voucherDateTo = null,
        string? voucherNumber = null,
        string? sortProperty = null, 
        string? sortDirection = null)
    {
        var queryParameters = new Dictionary<string, string?>();
        queryParameters["page"] = page.ToString();
        queryParameters["size"] = size.ToString();
        if (!string.IsNullOrEmpty(voucherStatus)) queryParameters["voucherStatus"] = voucherStatus;
        // voucherType is mandatory for /voucherlist according to docs, ensure it's always passed or has a sensible default.
        if (!string.IsNullOrEmpty(voucherType)) queryParameters["voucherType"] = voucherType; 
        else throw new ArgumentException("voucherType cannot be empty for ListInvoicesAsync (voucherlist).", nameof(voucherType));


        if (contactId.HasValue) queryParameters["contactId"] = contactId.Value.ToString();
        if (voucherDateFrom.HasValue) queryParameters["voucherDateFrom"] = voucherDateFrom.Value.ToString("yyyy-MM-dd");
        if (voucherDateTo.HasValue) queryParameters["voucherDateTo"] = voucherDateTo.Value.ToString("yyyy-MM-dd");
        if (!string.IsNullOrEmpty(voucherNumber)) queryParameters["voucherNumber"] = voucherNumber;
        
        if (!string.IsNullOrEmpty(sortProperty) && !string.IsNullOrEmpty(sortDirection))
        {
            queryParameters["sort"] = $"{sortProperty},{sortDirection.ToUpperInvariant()}";
        }
        else if (!string.IsNullOrEmpty(sortProperty))
        {
            queryParameters["sort"] = sortProperty; // Default direction (usually ASC)
        }

        var queryStringParts = queryParameters
            .Where(kvp => kvp.Value != null)
            .Select(kvp => $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}");
        
        var queryString = string.Join("&", queryStringParts);
        var requestUrl = $"{BaseApiUrl}/voucherlist?{queryString}";

        var response = await _httpClient.GetAsync(requestUrl);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error fetching voucher list. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}, Details: {errorContent}", null, response.StatusCode);
        }
        return await response.Content.ReadFromJsonAsync<PageableResponse<InvoiceSummary>>(_jsonSerializerOptions);
    }

    /// <summary>
    /// Retrieves a specific invoice by its unique Lexoffice ID.
    /// </summary>
    /// <param name="invoiceId">The GUID of the invoice to retrieve.</param>
    /// <returns>The <see cref="InvoiceDetail"/> object if found; otherwise, null if the invoice does not exist (HTTP 404).</returns>
    /// <exception cref="HttpRequestException">If the API request fails with a status code other than 404.</exception>
    /// <exception cref="ArgumentException">If <paramref name="invoiceId"/> is an empty GUID.</exception>
    public async Task<InvoiceDetail?> GetInvoiceAsync(Guid invoiceId)
    {
        if (invoiceId == Guid.Empty) throw new ArgumentException("Invoice ID cannot be an empty GUID.", nameof(invoiceId));

        var response = await _httpClient.GetAsync($"{BaseApiUrl}/invoices/{invoiceId}");

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null; 
            }
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error fetching invoice with ID {invoiceId}. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}, Details: {errorContent}", null, response.StatusCode);
        }
        return await response.Content.ReadFromJsonAsync<InvoiceDetail>(_jsonSerializerOptions);
    }

    /// <summary>
    /// Creates a payment for a specific invoice.
    /// </summary>
    /// <param name="invoiceId">The ID of the invoice to mark as paid.</param>
    /// <param name="paymentRequest">The payment details.</param>
    /// <returns>True if the payment was successfully created (API returns 200 OK).</returns>
    /// <exception cref="HttpRequestException">If the API request fails.</exception>
    /// <exception cref="ArgumentException">If invoiceId is empty.</exception>
    /// <exception cref="ArgumentNullException">If paymentRequest is null.</exception>
    public async Task<bool> CreateInvoicePaymentAsync(Guid invoiceId, InvoicePaymentRequest paymentRequest)
    {
        if (invoiceId == Guid.Empty) throw new ArgumentException("Invoice ID cannot be empty.", nameof(invoiceId));
        if (paymentRequest == null) throw new ArgumentNullException(nameof(paymentRequest));

        var response = await _httpClient.PostAsJsonAsync($"{BaseApiUrl}/invoices/{invoiceId}/payment", paymentRequest, _jsonSerializerOptions);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error creating payment for invoice {invoiceId}. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}, Details: {errorContent}", null, response.StatusCode);
        }
        // API documentation states "A successful call returns a 200 OK HTTP status code."
        return response.StatusCode == System.Net.HttpStatusCode.OK; 
    }


    // --- Quotations --- 
    
    /// <summary>
    /// Retrieves a specific quotation by its unique Lexoffice ID.
    /// </summary>
    /// <param name="quotationId">The GUID of the quotation to retrieve.</param>
    /// <returns>The <see cref="QuotationDetail"/> object if found; otherwise, null.</returns>
    public async Task<QuotationDetail?> GetQuotationAsync(Guid quotationId)
    {
        if (quotationId == Guid.Empty) throw new ArgumentException("Quotation ID cannot be an empty GUID.", nameof(quotationId));

        var response = await _httpClient.GetAsync($"{BaseApiUrl}/quotations/{quotationId}");

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error fetching quotation {quotationId}. Status: {response.StatusCode}, Details: {errorContent}", null, response.StatusCode);
        }
        return await response.Content.ReadFromJsonAsync<QuotationDetail>(_jsonSerializerOptions);
    }

    /// <summary>
    /// Creates a new quotation.
    /// </summary>
    /// <param name="quotationPayload">The quotation data. Version should be 0 for creation.</param>
    /// <param name="finalize">If true, the quotation is finalized directly. Otherwise, it's saved as a draft.</param>
    /// <returns>The response from Lexoffice after creating the quotation.</returns>
    public async Task<QuotationCreationResponse?> CreateQuotationAsync(QuotationDetail quotationPayload, bool finalize = false)
    {
        if (quotationPayload == null) throw new ArgumentNullException(nameof(quotationPayload));
        if (quotationPayload.Version != 0) throw new ArgumentException("Quotation version must be 0 for creation.", nameof(quotationPayload.Version));

        string url = finalize ? $"{BaseApiUrl}/quotations?finalize=true" : $"{BaseApiUrl}/quotations";
        var response = await _httpClient.PostAsJsonAsync(url, quotationPayload, _jsonSerializerOptions);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error creating quotation. Status: {response.StatusCode}, Details: {errorContent}", null, response.StatusCode);
        }
        return await response.Content.ReadFromJsonAsync<QuotationCreationResponse>(_jsonSerializerOptions);
    }

    /// <summary>
    /// Updates an existing quotation.
    /// </summary>
    /// <param name="quotationId">The ID of the quotation to update.</param>
    /// <param name="quotationPayload">The quotation data with updates. Version must match the current version.</param>
    /// <param name="finalize">If true, the quotation is finalized upon update.</param>
    /// <returns>The response from Lexoffice after updating the quotation.</returns>
    public async Task<QuotationCreationResponse?> UpdateQuotationAsync(Guid quotationId, QuotationDetail quotationPayload, bool finalize = false)
    {
        if (quotationId == Guid.Empty) throw new ArgumentException("Quotation ID cannot be empty.", nameof(quotationId));
        if (quotationPayload == null) throw new ArgumentNullException(nameof(quotationPayload));
        // Version check is important for updates, usually > 0
        if (quotationPayload.Version <= 0) throw new ArgumentException("Quotation version must be greater than 0 for updates.", nameof(quotationPayload.Version));


        string url = finalize ? $"{BaseApiUrl}/quotations/{quotationId}?finalize=true" : $"{BaseApiUrl}/quotations/{quotationId}";
        var response = await _httpClient.PutAsJsonAsync(url, quotationPayload, _jsonSerializerOptions);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error updating quotation {quotationId}. Status: {response.StatusCode}, Details: {errorContent}", null, response.StatusCode);
        }
        return await response.Content.ReadFromJsonAsync<QuotationCreationResponse>(_jsonSerializerOptions);
    }

    /// <summary>
    /// Retrieves all available payment conditions from Lexoffice.
    /// </summary>
    /// <returns>A list of payment condition items.</returns>
    /// <exception cref="HttpRequestException">If the API request fails.</exception>
    public async Task<List<PaymentConditionItem>?> GetPaymentConditionsAsync()
    {
        var response = await _httpClient.GetAsync($"{BaseApiUrl}/payment-conditions");

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error fetching payment conditions. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}, Details: {errorContent}", null, response.StatusCode);
        }
        
        // The API returns a root object with a "content" array.
        var paginatedResponse = await response.Content.ReadFromJsonAsync<PageableResponse<PaymentConditionItem>>(_jsonSerializerOptions);
        return paginatedResponse?.Content;
    }

    /// <summary>
    /// Retrieves all available print layouts from Lexoffice.
    /// </summary>
    /// <returns>A list of print layouts.</returns>
    /// <exception cref="HttpRequestException">If the API request fails.</exception>
    public async Task<List<PrintLayout>?> GetPrintLayoutsAsync()
    {
        var response = await _httpClient.GetAsync($"{BaseApiUrl}/print-layouts");

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error fetching print layouts. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}, Details: {errorContent}", null, response.StatusCode);
        }
        // The API returns a root object with a "content" array.
        var paginatedResponse = await response.Content.ReadFromJsonAsync<PageableResponse<PrintLayout>>(_jsonSerializerOptions);
        return paginatedResponse?.Content;
    }

    /// <summary>
    /// Downloads a file (e.g., an invoice PDF) from Lexoffice.
    /// The ID of the file can typically be retrieved from the respective resource (e.g., an invoice has a 'documentFileId' property).
    /// </summary>
    /// <param name="fileId">The ID of the file to download.</param>
    /// <returns>A tuple containing the file stream and its content type. Returns null if the file is not found (HTTP 404).</returns>
    /// <exception cref="HttpRequestException">If the API request fails with a status code other than 404.</exception>
    /// <exception cref="ArgumentException">If <paramref name="fileId"/> is null, empty, or whitespace.</exception>
    public async Task<(Stream fileStream, string contentType)?> DownloadFileAsync(string fileId)
    {
        if (string.IsNullOrWhiteSpace(fileId)) throw new ArgumentException("File ID cannot be null, empty, or whitespace.", nameof(fileId));

        var response = await _httpClient.GetAsync($"{BaseApiUrl}/files/{fileId}");

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null; // File not found
            }
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error downloading file with ID {fileId}. Status: {response.StatusCode}, Reason: {response.ReasonPhrase}, Details: {errorContent}", null, response.StatusCode);
        }

        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream"; // Default content type if not specified
        var fileStream = await response.Content.ReadAsStreamAsync();
        return (fileStream, contentType);
    }

    /// <summary>
    /// Disposes the underlying HttpClient instance if it was created and managed by this service.
    /// </summary>
    public void Dispose()
    {
        // HttpClient is typically managed by IHttpClientFactory, so direct disposal might not be needed
        // or could be handled by the factory. If this service *owns* the HttpClient, then dispose it.
        // For this example, assuming it might be owned, but best practice is factory management.
        _httpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
// Ensure the following using directive is present at the top of the file:
// using System.Text.Json.Serialization;
}

// --- Helper DTOs for API responses (These should ideally be in the Models folder or a dedicated DTOs folder within Models) ---
// For brevity, they are included here. In a larger project, separate them.

/// <summary>
/// Represents the response from Lexoffice when a contact is created.
/// Contains the ID, resource URI, creation/update timestamps, and version of the newly created contact.
/// </summary>
public class ContactCreationResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("resourceUri")]
    public string ResourceUri { get; set; } = string.Empty;

    [JsonPropertyName("createdDate")]
    public DateTimeOffset CreatedDate { get; set; }

    [JsonPropertyName("updatedDate")]
    public DateTimeOffset UpdatedDate { get; set; }

    [JsonPropertyName("version")]
    public int Version { get; set; }
}

/// <summary>
/// Represents the response from Lexoffice when a contact is updated.
/// Typically, this response is similar to the creation response, confirming the update and providing new timestamps/version.
/// </summary>
public class ContactUpdateResponse : ContactCreationResponse 
{
    // Inherits all properties from ContactCreationResponse
    // Add any update-specific fields if the API provides them differently
}

/// <summary>
/// Represents a paginated response for a list of contacts from Lexoffice.
/// Includes the list of contacts for the current page and metadata about pagination (total pages, total elements, etc.).
/// </summary>
public class PaginatedContactsResponse
{
    [JsonPropertyName("content")]
    public List<Contact> Content { get; set; } = new List<Contact>();

    [JsonPropertyName("first")]
    public bool First { get; set; }

    [JsonPropertyName("last")]
    public bool Last { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("totalElements")]
    public int TotalElements { get; set; }

    [JsonPropertyName("numberOfElements")]
    public int NumberOfElements { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("number")]
    public int Number { get; set; } // Current page number (0-based)

    [JsonPropertyName("sort")]
    public List<SortOrder>? Sort { get; set; } // Details about how the results are sorted
}

/// <summary>
/// Describes the sort order applied to a property in a paginated list response.
/// </summary>
public class SortOrder
{
    [JsonPropertyName("property")]
    public string Property { get; set; } = string.Empty;

    [JsonPropertyName("direction")]
    public string Direction { get; set; } = string.Empty;

    [JsonPropertyName("ignoreCase")]
    public bool IgnoreCase { get; set; }

    [JsonPropertyName("nullHandling")]
    public string NullHandling { get; set; } = string.Empty; // e.g., "NATIVE", "NULLS_FIRST", "NULLS_LAST"

    [JsonPropertyName("ascending")]
    public bool Ascending { get; set; }
}
