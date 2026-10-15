using RepositoryLayer.Entity;

namespace RepositoryLayer.Interface
{
    public interface INoteRL
    {
        Task<NoteEntity> CreateNote(NoteEntity note);
        Task<List<NoteEntity>> GetNotes(int userId);
        Task<NoteEntity?> GetNoteById(int userId, int noteId);
        Task UpdateNote(NoteEntity note);
        Task RemoveNote(NoteEntity note);
        Task<List<NoteEntity>> GetNotesByStatus(int userId, bool? isArchived, bool isDeleted);
    }
}