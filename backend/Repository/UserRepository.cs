using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository
{
    public class UserRepository : IUserRepository {
        private readonly KeepContext _context;
        public UserRepository(KeepContext context) {
            _context = context;
        }

        public async Task<User> GetUserById(string id) {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> GetUserByEmail(string email) {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetUserByUserName(string userName)
        {
            return await _context.Users
                .Include(u => u.Notes)
                .FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<User> CreateUser(User user) {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateUser(User user) {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> DeleteUser(string id) {
            var user = await _context.Users.FindAsync(id);
            if (user == null) {
                return null;
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<IEnumerable<User>> GetUsers(){
            var users = await _context.Users.ToListAsync();
            return users;
        }
    }
}