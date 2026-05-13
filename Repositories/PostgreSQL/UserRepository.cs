using Entities;
using Npgsql;
using RepositoryContracts;

namespace Repositories.PostgreSQL;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<User> CreateAsync(User user)
    {
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

        if (await reader.ReadAsync())
        {
            return ReadUser(reader);
        }

        throw new Exception("Failed to create user.");
    }

    public async Task<User> GetSingle(string id)
    {
        const string sql = @"
            SELECT id, name
            FROM app_user
            WHERE id = @id;
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return ReadUser(reader);
        }

        throw new KeyNotFoundException($"User with id '{id}' was not found.");
    }

    public async Task<User> UpdateContentAsync(User user)
    {
        const string sql = @"
            UPDATE app_user
            SET name = @name
            WHERE id = @id
            RETURNING id, name;
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", user.Id);
        command.Parameters.AddWithValue("name", user.Name);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return ReadUser(reader);
        }

        throw new KeyNotFoundException($"User with id '{user.Id}' was not found.");
    }

    public async Task<User> DeleteAsync(string id)
    {
        const string sql = @"
            DELETE FROM app_user
            WHERE id = @id
            RETURNING id, name;
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return ReadUser(reader);
        }

        throw new KeyNotFoundException($"User with id '{id}' was not found.");
    }

    private async Task<NpgsqlConnection> CreateConnectionAsync()
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static User ReadUser(NpgsqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetString(0),
            Name = reader.GetString(1)
        };
    }
}