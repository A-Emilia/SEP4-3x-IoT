using Entities;
using Repositories.PostgreSQL;

namespace SEP4.Tests;

public class DeviceActionLogRepositoryIntegrationTests
{
    private readonly string _connectionString =
        "Host=localhost;Port=1324;Database=user_data;Username=postgres;Password=postgres";

    [Fact]
    public async Task CreateAsync_ShouldCreateLog()
    {
        // Arrange
        var userRepo = new UserRepository(_connectionString);
        var roomRepo = new RoomRepository(_connectionString);
        var logRepo = new DeviceActionLogRepository(_connectionString);

        var user = await userRepo.CreateAsync(new User
        {
            Name = Guid.NewGuid().ToString("N")[..10],
            Email = $"{Guid.NewGuid().ToString("N")[..8]}@t.com",
            PasswordHash = "hashedPassword"
        });

        var room = await roomRepo.CreateAsync(new Room
        {
            UserId = user.Id,
            Name = Guid.NewGuid().ToString("N")[..8]
        });

        var log = new DeviceActionLog
        {
            RoomId = room.Id,
            DeviceType = DeviceType.Heater,
            PreviousState = DeviceState.Off,
            NewState = DeviceState.On
        };

        // Act
        var createdLog = await logRepo.CreateAsync(log);

        // Assert
        Assert.NotNull(createdLog);
        Assert.Equal(DeviceType.Heater, createdLog.DeviceType);
        Assert.Equal(DeviceState.On, createdLog.NewState);
    }

    [Fact]
    public async Task GetByRoomIdAsync_ShouldReturnLogs()
    {
        // Arrange
        var userRepo = new UserRepository(_connectionString);
        var roomRepo = new RoomRepository(_connectionString);
        var logRepo = new DeviceActionLogRepository(_connectionString);

        var user = await userRepo.CreateAsync(new User
        {
            Name = Guid.NewGuid().ToString("N")[..10],
            Email = $"{Guid.NewGuid().ToString("N")[..8]}@t.com",
            PasswordHash = "hashedPassword"
        });

        var room = await roomRepo.CreateAsync(new Room
        {
            UserId = user.Id,
            Name = Guid.NewGuid().ToString("N")[..8]
        });

        await logRepo.CreateAsync(new DeviceActionLog
        {
            RoomId = room.Id,
            DeviceType = DeviceType.Window,
            PreviousState = DeviceState.Closed,
            NewState = DeviceState.Open
        });

        // Act
        var logs = await logRepo.GetByRoomIdAsync(room.Id);

        // Assert
        Assert.NotEmpty(logs);
    }

    [Fact]
    public async Task CreateAsync_WithNullPreviousState_ShouldCreateLog()
    {
        // Arrange
        var userRepo = new UserRepository(_connectionString);
        var roomRepo = new RoomRepository(_connectionString);
        var logRepo = new DeviceActionLogRepository(_connectionString);

        var user = await userRepo.CreateAsync(new User
        {
            Name = Guid.NewGuid().ToString("N")[..10],
            Email = $"{Guid.NewGuid().ToString("N")[..8]}@t.com",
            PasswordHash = "hashedPassword"
        });

        var room = await roomRepo.CreateAsync(new Room
        {
            UserId = user.Id,
            Name = Guid.NewGuid().ToString("N")[..8]
        });

        var log = new DeviceActionLog
        {
            RoomId = room.Id,
            DeviceType = DeviceType.Heater,
            PreviousState = null,
            NewState = DeviceState.On
        };

        // Act
        var createdLog = await logRepo.CreateAsync(log);

        // Assert
        Assert.Null(createdLog.PreviousState);
        Assert.Equal(DeviceState.On, createdLog.NewState);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnLogs()
    {
        // Arrange
        var userRepo = new UserRepository(_connectionString);
        var roomRepo = new RoomRepository(_connectionString);
        var logRepo = new DeviceActionLogRepository(_connectionString);

        var user = await userRepo.CreateAsync(new User
        {
            Name = Guid.NewGuid().ToString("N")[..10],
            Email = $"{Guid.NewGuid().ToString("N")[..8]}@t.com",
            PasswordHash = "hashedPassword"
        });

        var room = await roomRepo.CreateAsync(new Room
        {
            UserId = user.Id,
            Name = Guid.NewGuid().ToString("N")[..8]
        });

        await logRepo.CreateAsync(new DeviceActionLog
        {
            RoomId = room.Id,
            DeviceType = DeviceType.Heater,
            PreviousState = DeviceState.Off,
            NewState = DeviceState.On
        });

        // Act
        var logs = await logRepo.GetAllAsync();

        // Assert
        Assert.NotEmpty(logs);
    }

    [Fact]
    public async Task GetByRoomIdAsync_WithInvalidRoom_ShouldReturnEmptyList()
    {
        // Arrange
        var logRepo = new DeviceActionLogRepository(_connectionString);

        // Act
        var logs = await logRepo.GetByRoomIdAsync("invalid_room");

        // Assert
        Assert.Empty(logs);
    }
}