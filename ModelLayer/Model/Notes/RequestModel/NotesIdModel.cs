// NotesIdModel.cs
using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Model.Notes.RequestModel
{
    public class NotesIdModel
    {
        [Required]
        public int NoteId { get; set; }
    }
}