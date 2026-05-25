using Entities;

namespace RepositoryContracts;

public interface IRoomRepository
{
    Task<Room> CreateAsync(Room room);
    Task<Room> GetSingle(string id);
    Task<Room> GetSingleForUserAsync(string id, string userId);
    Task<Room> UpdateContentAsync(Room room);
    Task<Room> DeleteAsync(string id, string userId);
    Task<List<Room>> GetManyByUserIdAsync(string userId);
}