// NoteCreationModel.cs
using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Model.Notes.RequestModel
{
    public class NoteCreationModel
    {
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? Colour { get; set; }
    }
}