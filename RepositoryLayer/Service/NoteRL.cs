using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Context;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;

namespace RepositoryLayer.Service
{
    public class NoteRL : INoteRL
    {
        private readonly FundooContext _context;
        public NoteRL(FundooContext context) => _context = context;

        public async Task<NoteEntity> CreateNote(NoteEntity note)
        {
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<List<NoteEntity>> GetNotes(int userId) =>
            await _context.Notes
                .AsNoTracking()
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .ToListAsync();

        public async Task<NoteEntity?> GetNoteById(int userId, int noteId) =>
            await _context.Notes
                .FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);

        public async Task UpdateNote(NoteEntity note)
        {
            _context.Notes.Update(note);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveNote(NoteEntity note)
        {
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
        }
        public async Task<List<NoteEntity>> GetNotesByStatus(int userId, bool? isArchived, bool isDeleted)
        {
            var query = _context.Notes
                .AsNoTracking()
                .Where(n => n.UserId == userId && n.IsDeleted == isDeleted);

            if (isArchived.HasValue)
                query = query.Where(n => n.IsArchived == isArchived.Value);

            return await query.ToListAsync();
        }

    }
}