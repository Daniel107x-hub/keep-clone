using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository
{
    public class NoteRepository: INoteRepository {
        private readonly KeepContext _context;
        public NoteRepository(KeepContext context) {
            _context = context;
        }

        public async Task<Note> Create(Note note) {
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<Note> Update(Note note) {
            _context.Notes.Update(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<Note> Delete(string id) {
            var note = await _context.Notes.FindAsync(id);
            if (note == null) {
                return null;
            }
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<Note> Get(string id) {
            return await _context.Notes.FindAsync(id);
        }

        public async Task<IEnumerable<Note>> GetAll() {
            return await _context.Notes.ToListAsync();
        }
    }   
}