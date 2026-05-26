using Entities;
using Repositories.PostgreSQL;

namespace SEP4.Tests;

public class DeviceRepositoryIntegrationTests
{
    private readonly string _connectionString =
        "Host=localhost;Port=1324;Database=user_data;Username=postgres;Password=postgres";

    [Fact]
    public async Task CreateAsync_ShouldCreateDevice()
    {
        // Arrange
        var userRepo = new UserRepository(_connectionString);
        var roomRepo = new RoomRepository(_connectionString);
        var deviceRepo = new DeviceRepository(_connectionString);

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

        var device = new Device
        {
            Id = Guid.NewGuid().ToString("N")[..10],
            RoomId = room.Id,
            Type = DeviceType.Heater,
            State = DeviceState.On
        };

        // Act
        var createdDevice = await deviceRepo.CreateAsync(device);

        // Assert
        Assert.NotNull(createdDevice);
        Assert.Equal(device.Id, createdDevice.Id);
        Assert.Equal(DeviceType.Heater, createdDevice.Type);
        Assert.Equal(DeviceState.On, createdDevice.State);
    }

    [Fact]
    public async Task GetDevice_ShouldReturnDevice()
    {
        // Arrange
        var userRepo = new UserRepository(_connectionString);
        var roomRepo = new RoomRepository(_connectionString);
        var deviceRepo = new DeviceRepository(_connectionString);

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

        var createdDevice = await deviceRepo.CreateAsync(new Device
        {
            Id = Guid.NewGuid().ToString("N")[..10],
            RoomId = room.Id,
            Type = DeviceType.Window,
            State = DeviceState.Open
        });

        // Act
        var fetchedDevice = await deviceRepo.GetDevice(createdDevice.Id);

        // Assert
        Assert.Equal(createdDevice.Id, fetchedDevice.Id);
        Assert.Equal(DeviceType.Window, fetchedDevice.Type);
        Assert.Equal(DeviceState.Open, fetchedDevice.State);
    }

    [Fact]
    public async Task GetDevice_WithInvalidId_ShouldThrowException()
    {
        // Arrange
        var deviceRepo = new DeviceRepository(_connectionString);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            deviceRepo.GetDevice("invalid_device"));
    }

    [Fact]
    public async Task GetDeviceState_ShouldReturnState()
    {
        // Arrange
        var userRepo = new UserRepository(_connectionString);
        var roomRepo = new RoomRepository(_connectionString);
        var deviceRepo = new DeviceRepository(_connectionString);

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

        await deviceRepo.CreateAsync(new Device
        {
            Id = Guid.NewGuid().ToString("N")[..10],
            RoomId = room.Id,
            Type = DeviceType.Heater,
            State = DeviceState.On
        });

        // Act
        var state = await deviceRepo.GetDeviceState(
            room.Id,
            DeviceType.Heater
        );

        // Assert
        Assert.Equal(DeviceState.On, state);
    }

    [Fact]
    public async Task GetDeviceState_WithInvalidRoom_ShouldThrowException()
    {
        // Arrange
        var deviceRepo = new DeviceRepository(_connectionString);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            deviceRepo.GetDeviceState(
                "invalid_room",
                DeviceType.Heater
            ));
    }

    [Fact]
    public async Task SetState_ShouldUpdateDeviceState()
    {
        // Arrange
        var userRepo = new UserRepository(_connectionString);
        var roomRepo = new RoomRepository(_connectionString);
        var deviceRepo = new DeviceRepository(_connectionString);

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

        await deviceRepo.CreateAsync(new Device
        {
            Id = Guid.NewGuid().ToString("N")[..10],
            RoomId = room.Id,
            Type = DeviceType.Heater,
            State = DeviceState.Off
        });

        // Act
        await deviceRepo.SetState(
            room.Id,
            DeviceType.Heater,
            DeviceState.On
        );

        var updatedState = await deviceRepo.GetDeviceState(
            room.Id,
            DeviceType.Heater
        );

        // Assert
        Assert.Equal(DeviceState.On, updatedState);
    }

    [Fact]
    public async Task SetState_WithInvalidRoom_ShouldThrowException()
    {
        // Arrange
        var deviceRepo = new DeviceRepository(_connectionString);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            deviceRepo.SetState(
                "invalid_room",
                DeviceType.Heater,
                DeviceState.On
            ));
    }

    [Fact]
    public async Task GetAllDevices_ShouldReturnDevices()
    {
        // Arrange
        var userRepo = new UserRepository(_connectionString);
        var roomRepo = new RoomRepository(_connectionString);
        var deviceRepo = new DeviceRepository(_connectionString);

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

        await deviceRepo.CreateAsync(new Device
        {
            Id = Guid.NewGuid().ToString("N")[..10],
            RoomId = room.Id,
            Type = DeviceType.Heater,
            State = DeviceState.On
        });

        // Act
        var devices = await deviceRepo.GetAllDevices(room.Id);

        // Assert
        Assert.NotEmpty(devices);
        Assert.True(devices.ContainsKey(DeviceType.Heater));
    }
}