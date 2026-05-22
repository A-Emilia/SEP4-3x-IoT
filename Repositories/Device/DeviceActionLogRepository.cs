using Entities;
using Npgsql;
using RepositoryContracts;

namespace Repositories.PostgreSQL;

public class DeviceActionLogRepository : IDeviceActionLogRepository
{
    private readonly string _connectionString;

    public DeviceActionLogRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<DeviceActionLog> CreateAsync(DeviceActionLog log)
    {
        const string sql = @"
            INSERT INTO device_action_log 
                (room_id, device_type, previous_state, new_state)
            VALUES 
                (@roomId, CAST(@deviceType AS actuator_type), CAST(@previousState AS actuator_state), CAST(@newState AS actuator_state))
            RETURNING id, room_id, device_type::text, previous_state::text, new_state::text, timestamp_utc;
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("roomId", log.RoomId);
        command.Parameters.AddWithValue("deviceType", ToDatabaseType(log.DeviceType));

        if (log.PreviousState == null)
            command.Parameters.AddWithValue("previousState", DBNull.Value);
        else
            command.Parameters.AddWithValue("previousState", ToDatabaseState(log.DeviceType, log.PreviousState.Value));

        command.Parameters.AddWithValue("newState", ToDatabaseState(log.DeviceType, log.NewState));

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return ReadLog(reader);
        }

        throw new Exception("Failed to create device action log.");
    }

    public async Task<List<DeviceActionLog>> GetByRoomIdAsync(string roomId)
    {
        const string sql = @"
            SELECT id, room_id, device_type::text, previous_state::text, new_state::text, timestamp_utc
            FROM device_action_log
            WHERE room_id = @roomId
            ORDER BY timestamp_utc DESC;
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("roomId", roomId);

        await using var reader = await command.ExecuteReaderAsync();

        var logs = new List<DeviceActionLog>();

        while (await reader.ReadAsync())
        {
            logs.Add(ReadLog(reader));
        }

        return logs;
    }

    public async Task<List<DeviceActionLog>> GetAllAsync()
    {
        const string sql = @"
            SELECT id, room_id, device_type::text, previous_state::text, new_state::text, timestamp_utc
            FROM device_action_log
            ORDER BY timestamp_utc DESC;
        ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var logs = new List<DeviceActionLog>();

        while (await reader.ReadAsync())
        {
            logs.Add(ReadLog(reader));
        }

        return logs;
    }
    
    public async Task<List<DeviceActionLog>> GetByTimestampAsync(DateTime from, DateTime to)
    {
        const string sql = @"
        SELECT id, room_id, device_type::text, previous_state::text, new_state::text, timestamp_utc
        FROM device_action_log
        WHERE timestamp_utc >= @from
          AND timestamp_utc <= @to
        ORDER BY timestamp_utc DESC;
    ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("from", from);
        command.Parameters.AddWithValue("to", to);

        await using var reader = await command.ExecuteReaderAsync();

        var logs = new List<DeviceActionLog>();

        while (await reader.ReadAsync())
        {
            logs.Add(ReadLog(reader));
        }

        return logs;
    }

    public async Task<List<DeviceActionLog>> GetByRoomIdAndTimestampAsync(
        string roomId,
        DateTime from,
        DateTime to)
    {
        const string sql = @"
        SELECT id, room_id, device_type::text, previous_state::text, new_state::text, timestamp_utc
        FROM device_action_log
        WHERE room_id = @roomId
          AND timestamp_utc >= @from
          AND timestamp_utc <= @to
        ORDER BY timestamp_utc DESC;
    ";

        await using var connection = await CreateConnectionAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("roomId", roomId);
        command.Parameters.AddWithValue("from", from);
        command.Parameters.AddWithValue("to", to);

        await using var reader = await command.ExecuteReaderAsync();

        var logs = new List<DeviceActionLog>();

        while (await reader.ReadAsync())
        {
            logs.Add(ReadLog(reader));
        }

        return logs;
    }

    private async Task<NpgsqlConnection> CreateConnectionAsync()
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static DeviceActionLog ReadLog(NpgsqlDataReader reader)
    {
        var databaseType = reader.GetString(2);
        var deviceType = FromDatabaseType(databaseType);

        DeviceState? previousState = null;

        if (!reader.IsDBNull(3))
        {
            previousState = FromDatabaseState(deviceType, reader.GetString(3));
        }

        return new DeviceActionLog
        {
            Id = reader.GetInt32(0),
            RoomId = reader.GetString(1),
            DeviceType = deviceType,
            PreviousState = previousState,
            NewState = FromDatabaseState(deviceType, reader.GetString(4)),
            TimestampUtc = reader.GetDateTime(5)
        };
    }

    private static string ToDatabaseType(DeviceType type)
    {
        return type switch
        {
            DeviceType.Heater => "Heater",
            DeviceType.Window => "Window Servo",
            DeviceType.Curtain => "Curtain Servo",
            DeviceType.Humidifier => "Humidifier",
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
            "Humidifier" => DeviceType.Humidifier,
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
            
            DeviceType.Humidifier => state switch
            {
                DeviceState.Open => "On/Open",
                DeviceState.Closed => "Off/Closed",
                _ => throw new InvalidOperationException("Humidifier can only be On or Off.")
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
            
            DeviceType.Humidifier => state switch
            {
                "On/Open" => DeviceState.Open,
                "Off/Closed" => DeviceState.Closed,
                _ => throw new InvalidOperationException($"Unknown curtain state: {state}.")
            },

            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}