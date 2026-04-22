using Entities;

namespace RepositoryContracts;

public interface IUserRepository
{
    Task<User> CreateAsync(User user);
    Task<User> GetSingle(int id);
    Task<User> UpdateContentAsync(User user);
    Task<User> DeleteAsync(int id);
}