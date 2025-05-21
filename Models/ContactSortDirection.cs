using System.Text.Json.Serialization;

namespace MixMedia.MCP.LexOffice.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ContactSortDirection
    {
        ASC,
        DESC
    }
}
