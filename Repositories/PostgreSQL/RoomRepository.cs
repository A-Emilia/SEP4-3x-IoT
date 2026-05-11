using Entities;
using RepositoryContracts;

namespace Repositories.PostgreSQL;

public class RoomRepository : IRoomRepository {

    private readonly string _connectionString;

    public RoomRepository(string connectionstring) {
        _connectionString = connectionstring;
    }
    
    public Task<Room> CreateAsync(Room room) {
        throw new NotImplementedException();
    }

    public Task<Room> DeleteAsync(int id) {
        throw new NotImplementedException();
    }

    public Task<List<Room>> GetManyAsync() {
        throw new NotImplementedException();
    }

    public Task<Room> GetSingle(int id) {
        throw new NotImplementedException();
    }

    public Task<Room> UpdateContentAsync(Room room) {
        throw new NotImplementedException();
    }
}