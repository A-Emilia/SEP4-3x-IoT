using Entities;
using Npgsql;
using RepositoryContracts;

namespace Repositories.PostgreSQL;

public class DeviceRepository : IDeviceRepository
{
    private readonly string _connectionString;

    public DeviceRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Device> CreateAsync(Device device)
    {
        const string sql = @"
            INSERT INTO actuator (id, room_id, state, type)
            VALUES (
                @id,
                @roomId,
                CAST(@state AS actuator_state),
                CAST(@type AS actuator_type)
            )
            RETURNING id, room_id, state::text, type::text;
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", device.Id);
        command.Parameters.AddWithValue("roomId", device.RoomId);
        command.Parameters.AddWithValue("state", ToDatabaseState(device.Type, device.State));
        command.Parameters.AddWithValue("type", ToDatabaseType(device.Type));

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return ReadDevice(reader);
        }

        throw new Exception("Failed to create device.");
    }

    public async Task<Device> GetDevice(string id)
    {
        const string sql = @"
            SELECT id, room_id, state::text, type::text
            FROM actuator
            WHERE id = @id;
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return ReadDevice(reader);
        }

        throw new KeyNotFoundException($"Device with id '{id}' was not found.");
    }

    public async Task<DeviceState> GetDeviceState(string roomId, DeviceType device)
    {
        const string sql = @"
            SELECT state::text
            FROM actuator
            WHERE room_id = @roomId
              AND type = CAST(@type AS actuator_type)
            LIMIT 1;
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("roomId", roomId);
        command.Parameters.AddWithValue("type", ToDatabaseType(device));

        var result = await command.ExecuteScalarAsync();

        if (result == null)
        {
            throw new KeyNotFoundException(
                $"Device '{device}' was not found in room '{roomId}'."
            );
        }

        return FromDatabaseState(device, result.ToString()!);
    }

    public async Task SetState(string roomId, DeviceType device, DeviceState state)
    {
        const string sql = @"
            UPDATE actuator
            SET state = CAST(@state AS actuator_state)
            WHERE room_id = @roomId
              AND type = CAST(@type AS actuator_type);
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("roomId", roomId);
        command.Parameters.AddWithValue("type", ToDatabaseType(device));
        command.Parameters.AddWithValue("state", ToDatabaseState(device, state));

        var affectedRows = await command.ExecuteNonQueryAsync();

        if (affectedRows == 0)
        {
            throw new KeyNotFoundException(
                $"Device '{device}' was not found in room '{roomId}'."
            );
        }
    }

    public async Task<Dictionary<DeviceType, DeviceState>> GetAllDevices(string roomId)
    {
        const string sql = @"
            SELECT type::text, state::text
            FROM actuator
            WHERE room_id = @roomId;
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("roomId", roomId);

        await using var reader = await command.ExecuteReaderAsync();

        var devices = new Dictionary<DeviceType, DeviceState>();

        while (await reader.ReadAsync())
        {
            var databaseType = reader.GetString(0);
            var databaseState = reader.GetString(1);

            var deviceType = FromDatabaseType(databaseType);
            var deviceState = FromDatabaseState(deviceType, databaseState);

            devices[deviceType] = deviceState;
        }

        return devices;
    }

    private async Task<NpgsqlConnection> CreateConnectionAsync()
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static Device ReadDevice(NpgsqlDataReader reader)
    {
        var databaseState = reader.GetString(2);
        var databaseType = reader.GetString(3);

        var deviceType = FromDatabaseType(databaseType);

        return new Device
        {
            Id = reader.GetString(0),
            RoomId = reader.GetString(1),
            State = FromDatabaseState(deviceType, databaseState),
            Type = deviceType
        };
    }

    private static string ToDatabaseType(DeviceType type)
    {
        return type switch
        {
            DeviceType.Heater => "Heater",
            DeviceType.Window => "Window Servo",
            DeviceType.Curtain => "Curtain Servo",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static DeviceType FromDatabaseType(string type)
    {
        return type switch
        {
            "Heater" => DeviceType.Heater,
            "Window Servo" => DeviceType.Window,
            "Curtain Servo" => DeviceType.Curtain,
            _ => throw new InvalidOperationException($"Unknown device type: {type}")
        };
    }

    private static string ToDatabaseState(DeviceType type, DeviceState state)
    {
        return type switch
        {
            DeviceType.Heater => state switch
            {
                DeviceState.On => "On/Open",
                DeviceState.Off => "Off/Closed",
                _ => throw new InvalidOperationException("Heater can only be On or Off.")
            },

            DeviceType.Window => state switch
            {
                DeviceState.Open => "On/Open",
                DeviceState.Closed => "Off/Closed",
                _ => throw new InvalidOperationException("Window can only be Open or Closed.")
            },

            DeviceType.Curtain => state switch
            {
                DeviceState.Open => "On/Open",
                DeviceState.Closed => "Off/Closed",
                _ => throw new InvalidOperationException("Curtain can only be Open or Closed.")
            },

            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static DeviceState FromDatabaseState(DeviceType type, string state)
    {
        return type switch
        {
            DeviceType.Heater => state switch
            {
                "On/Open" => DeviceState.On,
                "Off/Closed" => DeviceState.Off,
                _ => throw new InvalidOperationException($"Unknown heater state: {state}")
            },

            DeviceType.Window => state switch
            {
                "On/Open" => DeviceState.Open,
                "Off/Closed" => DeviceState.Closed,
                _ => throw new InvalidOperationException($"Unknown window state: {state}")
            },

            DeviceType.Curtain => state switch
            {
                "On/Open" => DeviceState.Open,
                "Off/Closed" => DeviceState.Closed,
                _ => throw new InvalidOperationException($"Unknown curtain state: {state}")
            },

            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}