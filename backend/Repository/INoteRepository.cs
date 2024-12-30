using backend.Models;

namespace backend.Repository
{
    public interface INoteRepository {
        Task<Note> Create(Note note);
        Task<Note> Update(Note note);
        Task<Note> Delete(string id);
        Task<Note> Get(string id);
        Task<IEnumerable<Note>> GetAll();
    }
}