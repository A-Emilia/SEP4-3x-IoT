using Entities;

namespace RepositoryContracts;

public interface IUserRepository
{
    Task<User> CreateAsync(User user);
    Task<User> GetSingle(string id);
    Task<User?> GetByNameAsync(string name);
    Task<User?> GetByEmailAsync(string email);
    Task<User> UpdateContentAsync(User user);
    Task<User> DeleteAsync(string id);
}