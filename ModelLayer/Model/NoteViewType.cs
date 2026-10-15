using System.Text.Json.Serialization;

namespace ModelLayer.Model
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum NoteViewType
    {
        Active,
        Archive,
        Trash
    }
}