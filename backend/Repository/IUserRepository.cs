public interface IUserRepository {
    Task<User> GetUserById(string id);
    Task<User> GetUserByEmail(string email);
    Task<User> GetUserByUserName(string userName);
    Task<User> CreateUser(User user);
    Task<User> UpdateUser(User user);
    Task<User> DeleteUser(string id);
    Task<IEnumerable<User>> GetUsers();
}