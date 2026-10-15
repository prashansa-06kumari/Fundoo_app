using ModelLayer.Model;

namespace BusinessLayer.Interface
{
    public interface INoteBL
    {
        Task<NoteResponseModel> CreateNote(int userId, NoteCreationModel model);

        // CHANGED: now takes NoteViewType (Active / Archive / Trash)
        Task<List<NoteResponseModel>> ViewNotes(int userId, NoteViewType type);

        Task<NoteResponseModel?> ViewNotesById(int userId, NotesIdModel model);
        Task<NoteResponseModel?> EditNotes(int userId, EditNotesModel model);
        Task<bool> DeleteNotes(int userId, NotesIdModel model);
        Task<NoteResponseModel?> ArchiveUnarchive(int userId, NotesIdModel model);
        Task<NoteResponseModel?> TrashUntrash(int userId, NotesIdModel model);
    }
}