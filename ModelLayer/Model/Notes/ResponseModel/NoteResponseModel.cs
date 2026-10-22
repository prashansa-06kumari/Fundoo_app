namespace ModelLayer.Model.Notes.ResponseModel
{
    public class NoteResponseModel
    {
        public int NoteId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Colour { get; set; }
        public bool IsArchived { get; set; }
        public bool IsDeleted { get; set; }
    }
}