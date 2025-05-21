using System;
using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models;

/// <summary>
/// Represents an article in Lexoffice.
/// </summary>
public class Article
{
    /// <summary>
    /// Unique id of the article generated on creation by lexoffice.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Unique id of the organization the article belongs to.
    /// </summary>
    [JsonPropertyName("organizationId")]
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// The instant of time when the article was created by lexoffice (RFC 3339/ISO 8601).
    /// Read-only.
    /// </summary>
    [JsonPropertyName("createdDate")]
    public DateTimeOffset CreatedDate { get; set; }

    /// <summary>
    /// The instant of time when the article was updated by lexoffice (RFC 3339/ISO 8601).
    /// Read-only.
    /// </summary>
    [JsonPropertyName("updatedDate")]
    public DateTimeOffset UpdatedDate { get; set; }

    /// <summary>
    /// Archived flag of the article. Read-only.
    /// </summary>
    [JsonPropertyName("archived")]
    public bool Archived { get; set; }

    /// <summary>
    /// Title of the article.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Description of the article.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Type of the article. Possible values are PRODUCT and SERVICE.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The article number as given by the user.
    /// </summary>
    [JsonPropertyName("articleNumber")]
    public string ArticleNumber { get; set; } = string.Empty;

    /// <summary>
    /// Global Trade Item Number (GTIN) of the article. If given, the value will be validated to match one of the GTIN-8, GTIN-12, GTIN-13, or GTIN-14 formats.
    /// </summary>
    [JsonPropertyName("gtin")]
    public string? Gtin { get; set; }

    /// <summary>
    /// Internal note for the article.
    /// </summary>
    [JsonPropertyName("note")]
    public string? Note { get; set; }

    /// <summary>
    /// Unit name of the article.
    /// </summary>
    [JsonPropertyName("unitName")]
    public string? UnitName { get; set; }

    /// <summary>
    /// Price of the article.
    /// </summary>
    [JsonPropertyName("price")]
    public ArticlePrice Price { get; set; } = new();

    /// <summary>
    /// Version (revision) number which will be increased on each change to handle optimistic locking. Read-only.
    /// </summary>
    [JsonPropertyName("version")]
    public int Version { get; set; }
}

/// <summary>
/// Represents the price details of an article.
/// </summary>
public class ArticlePrice
{
    /// <summary>
    /// Net price of the article.
    /// </summary>
    [JsonPropertyName("netPrice")]
    public decimal NetPrice { get; set; }

    /// <summary>
    /// Gross price of the article.
    /// </summary>
    [JsonPropertyName("grossPrice")]
    public decimal GrossPrice { get; set; }

    /// <summary>
    /// Leading price type. Possible values: NET, GROSS.
    /// </summary>
    [JsonPropertyName("leadingPrice")]
    public string LeadingPrice { get; set; } = string.Empty;

    /// <summary>
    /// Tax rate for the article.
    /// </summary>
    [JsonPropertyName("taxRate")]
    public decimal TaxRate { get; set; }
}
