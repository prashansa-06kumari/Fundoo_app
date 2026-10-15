using System.ComponentModel.DataAnnotations;

namespace RepositoryLayer.Entity
{
    public class NoteEntity
    {
        [Key]
        public int NoteId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? Colour { get; set; }
        public bool IsArchived { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        [Required]
        public int UserId { get; set; }
        public UserEntity User { get; set; } = null!;
    }
}