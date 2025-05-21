using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ContactSortField
    {
        name,
        customerNumber,
        vendorNumber,
        email,
        createdDate,
        updatedDate
    }
}
