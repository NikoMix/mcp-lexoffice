using System;
using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models;

/// <summary>
/// Represents the Lexoffice profile properties.
/// </summary>
public class Profile
{
    /// <summary>Unique id of the organization.</summary>
    [JsonPropertyName("organizationId")]
    public Guid OrganizationId { get; set; }

    /// <summary>Name of the organization.</summary>
    [JsonPropertyName("organizationName")]
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>Country code of the organization (ISO 3166-1 alpha-2).</summary>
    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; } = string.Empty;

    /// <summary>Language code of the organization (ISO 639-1).</summary>
    [JsonPropertyName("languageCode")]
    public string LanguageCode { get; set; } = string.Empty;

    /// <summary>Currency code of the organization (ISO 4217).</summary>
    [JsonPropertyName("currencyCode")]
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>VAT ID of the organization.</summary>
    [JsonPropertyName("vatId")]
    public string? VatId { get; set; }

    /// <summary>Tax number of the organization.</summary>
    [JsonPropertyName("taxNumber")]
    public string? TaxNumber { get; set; }

    /// <summary>Tax office of the organization.</summary>
    [JsonPropertyName("taxOffice")]
    public string? TaxOffice { get; set; }

    /// <summary>Street address of the organization.</summary>
    [JsonPropertyName("street")]
    public string? Street { get; set; }

    /// <summary>Zip code of the organization.</summary>
    [JsonPropertyName("zip")]
    public string? Zip { get; set; }

    /// <summary>City of the organization.</summary>
    [JsonPropertyName("city")]
    public string? City { get; set; }

    /// <summary>Phone number of the organization.</summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    /// <summary>Email address of the organization.</summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>Website of the organization.</summary>
    [JsonPropertyName("website")]
    public string? Website { get; set; }
}
