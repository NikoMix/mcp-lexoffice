using System;
using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models;

public class InvoiceAddress
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("supplement")]
    public string? Supplement { get; set; }
    [JsonPropertyName("street")]
    public string? Street { get; set; }
    [JsonPropertyName("city")]
    public string? City { get; set; }
    [JsonPropertyName("zip")]
    public string? Zip { get; set; }
    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }
    [JsonPropertyName("contactId")]
    public Guid? ContactId { get; set; }
}
