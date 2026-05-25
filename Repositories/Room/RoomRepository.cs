using Entities;
using Npgsql;
using RepositoryContracts;

namespace Repositories.PostgreSQL;

public class RoomRepository : IRoomRepository
{
    private readonly string _connectionString;

    public RoomRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Room> CreateAsync(Room room)
    {
        if (string.IsNullOrWhiteSpace(room.Id))
        {
            room.Id = GenerateRoomId();
        }

        const string sql = @"
            INSERT INTO room (id, user_id, name)
            VALUES (@id, @userId, @name)
            RETURNING id, user_id, name;
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", room.Id);
        command.Parameters.AddWithValue("userId", room.UserId);
        command.Parameters.AddWithValue("name", room.Name);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return ReadRoom(reader);
        }

        throw new Exception("Failed to create room.");
    }

    public async Task<Room> GetSingle(string id)
    {
        const string sql = @"
            SELECT id, user_id, name
            FROM room
            WHERE id = @id;
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return ReadRoom(reader);
        }

        throw new KeyNotFoundException($"Room with id '{id}' was not found.");
    }

    public async Task<List<Room>> GetManyAsync()
    {
        const string sql = @"
            SELECT id, user_id, name
            FROM room
            ORDER BY name;
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var rooms = new List<Room>();

        while (await reader.ReadAsync())
        {
            rooms.Add(ReadRoom(reader));
        }

        return rooms;
    }

    public async Task<Room> UpdateContentAsync(Room room)
    {
        const string sql = @"
        UPDATE room
        SET name = @name
        WHERE id = @id
          AND user_id = @userId
        RETURNING id, user_id, name;
    ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", room.Id);
        command.Parameters.AddWithValue("userId", room.UserId);
        command.Parameters.AddWithValue("name", room.Name);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return ReadRoom(reader);
        }

        throw new KeyNotFoundException($"Room with id '{room.Id}' was not found for this user.");
    }
    
    public async Task<Room> DeleteAsync(string id, string userId)
    {
        const string sql = @"
        DELETE FROM room
        WHERE id = @id
          AND user_id = @userId
        RETURNING id, user_id, name;
    ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("userId", userId);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return ReadRoom(reader);
        }

        throw new KeyNotFoundException($"Room with id '{id}' was not found for this user.");
    }
    
    public async Task<List<Room>> GetManyByUserIdAsync(string userId)
    {
        const string sql = @"
        SELECT id, user_id, name
        FROM room
        WHERE user_id = @userId
        ORDER BY name;
    ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("userId", userId);

        await using var reader = await command.ExecuteReaderAsync();

        var rooms = new List<Room>();

        while (await reader.ReadAsync())
        {
            rooms.Add(ReadRoom(reader));
        }

        return rooms;
    }

    public async Task<Room> GetSingleForUserAsync(string id, string userId)
    {
        const string sql = @"
        SELECT id, user_id, name
        FROM room
        WHERE id = @id
          AND user_id = @userId;
    ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("userId", userId);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return ReadRoom(reader);
        }

        throw new KeyNotFoundException($"Room with id '{id}' was not found for this user.");
    }
    
    private async Task<NpgsqlConnection> CreateConnectionAsync()
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static Room ReadRoom(NpgsqlDataReader reader)
    {
        return new Room
        {
            Id = reader.GetString(0),
            UserId = reader.GetString(1),
            Name = reader.GetString(2)
        };
    }

    private static string GenerateRoomId()
    {
        return Guid.NewGuid().ToString("N")[..16];
    }
}