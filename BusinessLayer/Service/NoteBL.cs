using BusinessLayer.Interface;
using ModelLayer.Model.Notes.RequestModel;
using ModelLayer.Model.Notes.ResponseModel;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;

namespace BusinessLayer.Service
{
    public class NoteBL : INoteBL
    {
        private readonly INoteRL _noteRL;
        public NoteBL(INoteRL noteRL) => _noteRL = noteRL;

        private static NoteResponseModel Map(NoteEntity n) => new()
        {
            NoteId = n.NoteId,
            Title = n.Title,
            Description = n.Description,
            Colour = n.Colour,
            IsArchived = n.IsArchived,
            IsDeleted = n.IsDeleted
        };

        public async Task<NoteResponseModel> CreateNote(int userId, NoteCreationModel model)
        {
            var note = new NoteEntity
            {
                Title = model.Title.Trim(),
                Description = model.Description,
                Colour = model.Colour,
                UserId = userId
            };
            return Map(await _noteRL.CreateNote(note));
        }

        // CHANGED: now takes a NoteViewType (Active / Archive / Trash)
        public async Task<List<NoteResponseModel>> ViewNotes(int userId, NoteViewType type)
        {
            var notes = type switch
            {
                NoteViewType.Archive => await _noteRL.GetNotesByStatus(userId, true, false),
                NoteViewType.Trash => await _noteRL.GetNotesByStatus(userId, null, true),
                _ => await _noteRL.GetNotesByStatus(userId, false, false)  // Active
            };

            return notes.Select(Map).ToList();
        }

        public async Task<NoteResponseModel?> ViewNotesById(int userId, NotesIdModel model)
        {
            var note = await _noteRL.GetNoteById(userId, model.NoteId);
            return note == null ? null : Map(note);
        }

        public async Task<NoteResponseModel?> EditNotes(int userId, EditNotesModel model)
        {
            var note = await _noteRL.GetNoteById(userId, model.NoteId);
            if (note == null) return null;

            note.Title = model.Title.Trim();
            note.Description = model.Description;
            note.Colour = model.Colour;
            await _noteRL.UpdateNote(note);
            return Map(note);
        }

        public async Task<bool> DeleteNotes(int userId, NotesIdModel model)
        {
            var note = await _noteRL.GetNoteById(userId, model.NoteId);
            if (note == null) return false;

            await _noteRL.RemoveNote(note);   // permanent delete
            return true;
        }

        public async Task<NoteResponseModel?> ArchiveUnarchive(int userId, NotesIdModel model)
        {
            var note = await _noteRL.GetNoteById(userId, model.NoteId);
            if (note == null) return null;

            note.IsArchived = !note.IsArchived;
            await _noteRL.UpdateNote(note);
            return Map(note);
        }

        public async Task<NoteResponseModel?> TrashUntrash(int userId, NotesIdModel model)
        {
            var note = await _noteRL.GetNoteById(userId, model.NoteId);
            if (note == null) return null;

            note.IsDeleted = !note.IsDeleted;
            await _noteRL.UpdateNote(note);
            return Map(note);
        }
    }
}