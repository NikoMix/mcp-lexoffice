using System;
using System.Collections.Generic;
using System.Linq;
using System.Net; // For WebUtility
using System.Text;

namespace MixMedia.MCP.LexOffice.Tools
{
    public enum ContactSortDirection
    {
        ASC,
        DESC
    }

    public enum ContactSortField
    {
        name,
        customerNumber,
        vendorNumber,
        email,
        createdDate,
        updatedDate
    }

    public class GetContactsTool
    {
        private const string BaseUrl = "https://api.lexoffice.io/v1/contacts";

        /// <summary>
        /// Builds the request URL for fetching and filtering contacts from the Lexoffice API.
        /// </summary>
        /// <param name="name">Filters by the name of the contact (person or company).</param>
        /// <param name="customerNumber">Filters by the customer number.</param>
        /// <param name="vendorNumber">Filters by the vendor number.</param>
        /// <param name="email">Filters by any email address of the contact.</param>
        /// <param name="page">For pagination (0-based).</param>
        /// <param name="size">Number of contacts per page (default 100, max 1000).</param>
        /// <param name="direction">Sort direction (ASC or DESC; default: ASC).</param>
        /// <param name="sort">Property to sort by (default: name).</param>
        /// <param name="archived">Filters by archived status.</param>
        /// <returns>The constructed URL string for the API request.</returns>
        public string BuildContactsRequestUrl(
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
            var queryParameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(name))
                queryParameters["name"] = name;
            if (!string.IsNullOrEmpty(customerNumber))
                queryParameters["customerNumber"] = customerNumber;
            if (!string.IsNullOrEmpty(vendorNumber))
                queryParameters["vendorNumber"] = vendorNumber;
            if (!string.IsNullOrEmpty(email))
                queryParameters["email"] = email;
            if (page.HasValue)
            {
                if (page.Value < 0)
                    throw new ArgumentOutOfRangeException(nameof(page), "Page cannot be negative.");
                queryParameters["page"] = page.Value.ToString();
            }
            if (size.HasValue)
            {
                if (size.Value <= 0 || size.Value > 1000)
                    throw new ArgumentOutOfRangeException(nameof(size), "Size must be between 1 and 1000.");
                queryParameters["size"] = size.Value.ToString();
            }
            if (direction.HasValue)
                queryParameters["direction"] = direction.Value.ToString();
            if (sort.HasValue)
                queryParameters["sort"] = sort.Value.ToString();
            if (archived.HasValue)
                queryParameters["archived"] = archived.Value.ToString().ToLowerInvariant(); // "true" or "false"

            if (!queryParameters.Any())
            {
                return BaseUrl;
            }

            // Using Uri.EscapeDataString for values as it's generally safer for query components.
            // Keys are from a controlled set and don't strictly need encoding here, but values do.
            var queryString = string.Join("&", queryParameters.Select(kvp =>
                $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
            
            return $"{BaseUrl}?{queryString}";
        }
    }
}
