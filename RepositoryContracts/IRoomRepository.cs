using Entities;

namespace RepositoryContracts;

public interface IRoomRepository
{
    Task<Room> CreateAsync(Room room);
    Task<Room> GetSingle(int id);
    Task<Room> UpdateContentAsync(Room room);
    Task<Room> DeleteAsync(int id);
    Task<List<Room>> GetManyAsync();
}