using Entities;
using Npgsql;
using RepositoryContracts;

namespace Repositories.PostgreSQL;

public class UserRepository : IUserRepository {

    private readonly string _connectionString;

    public UserRepository(string connectionstring) {
        _connectionString = connectionstring;
    }
    public async Task<User> CreateAsync(User user) {
        const string sql = @"
            INSERT INTO app_user (id, name)
            VALUES (@id, @name)
            RETURNING id, name;
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", user.Id);
        command.Parameters.AddWithValue("name", user.Name);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync()) {
            return new User {
                Id = reader.GetString(0),
                Name = reader.GetString(1)
            };
        }

        throw new Exception("Failed to create user.");
    }

    public Task<User> DeleteAsync(int id) {
        throw new NotImplementedException();
    }

    public Task<User> GetSingle(int id) {
        throw new NotImplementedException();
    }

    public Task<User> UpdateContentAsync(User user) {
        throw new NotImplementedException();
    }

    private async Task<NpgsqlConnection> CreateConnectionAsync() {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}