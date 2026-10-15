// NotesIdModel.cs
using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Model
{
    public class NotesIdModel
    {
        [Required]
        public int NoteId { get; set; }
    }
}