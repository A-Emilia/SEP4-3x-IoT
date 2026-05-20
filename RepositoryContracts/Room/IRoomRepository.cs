using Entities;

namespace RepositoryContracts;

public interface IRoomRepository
{
    Task<Room> CreateAsync(Room room);
    Task<Room> GetSingle(string id);
    Task<Room> UpdateContentAsync(Room room);
    Task<Room> DeleteAsync(string id);
    Task<List<Room>> GetManyAsync();
}