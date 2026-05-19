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
        if (string.IsNullOrWhiteSpace(user.Id))
        {
            user.Id = GenerateUserId();
        }

        const string sql = @"
            INSERT INTO app_user (id, name, email, password_hash)
            VALUES (@id, @name, @email, @passwordHash)
            RETURNING id, name, email, password_hash;
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", user.Id);
        command.Parameters.AddWithValue("name", user.Name);
        command.Parameters.AddWithValue("email", user.Email);
        command.Parameters.AddWithValue("passwordHash", user.PasswordHash);

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
            SELECT id, name, email, password_hash
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

    public async Task<User?> GetByNameAsync(string name)
    {
        const string sql = @"
            SELECT id, name, email, password_hash
            FROM app_user
            WHERE name = @name;
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("name", name);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return ReadUser(reader);
        }

        return null;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = @"
            SELECT id, name, email, password_hash
            FROM app_user
            WHERE email = @email
            LIMIT 1;
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("email", email);

        await using var reader = await command.ExecuteReaderAsync();

         if (await reader.ReadAsync())
        {
            return ReadUser(reader);
        }

        return null;
    }

    public async Task<User> UpdateContentAsync(User user)
    {
        const string sql = @"
            UPDATE app_user
            SET name = @name, email = @email
            WHERE id = @id
            RETURNING id, name, email, password_hash;
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", user.Id);
        command.Parameters.AddWithValue("name", user.Name);
        command.Parameters.AddWithValue("email", user.Email);

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
            RETURNING id, name, email, password_hash;
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
            Name = reader.GetString(1),
            Email = reader.GetString(2),
            PasswordHash = reader.GetString(3)
        };
    }

    private static string GenerateUserId()
    {
        return Guid.NewGuid().ToString("N")[..16];
    }
}