using System.Text.Json.Serialization;

namespace ModelLayer.Model.Notes.RequestModel
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum NoteViewType
    {
        Active,
        Archive,
        Trash
    }
}