using Entities;
using Repositories.PostgreSQL;

namespace SEP4.Tests;

public class RoomRepositoryIntegrationTests
{
    private readonly string _connectionString =
        "Host=localhost;Port=1324;Database=user_data;Username=postgres;Password=postgres";

    [Fact]
    public async Task GetSingle_ShouldReturnRoom()
    {
        // Arrange
        var userRepo = new UserRepository(_connectionString);
        var roomRepo = new RoomRepository(_connectionString);

        var user = await userRepo.CreateAsync(new User
        {
            Name = Guid.NewGuid().ToString("N")[..10],
            Email = $"{Guid.NewGuid().ToString("N")[..8]}@t.com",
            PasswordHash = "hashedPassword"
        });

        var roomName = Guid.NewGuid().ToString("N")[..8];

        var createdRoom = await roomRepo.CreateAsync(new Room
        {
            UserId = user.Id,
            Name = roomName
        });

        // Act
        var fetchedRoom = await roomRepo.GetSingle(createdRoom.Id);

        // Assert
        Assert.Equal(createdRoom.Id, fetchedRoom.Id);
        Assert.Equal(roomName, fetchedRoom.Name);
    }

    [Fact]
    public async Task GetSingle_WithInvalidId_ShouldThrowException()
    {
        // Arrange
        var roomRepo = new RoomRepository(_connectionString);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            roomRepo.GetSingle("invalid_room"));
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteRoom()
    {
        // Arrange
        var userRepo = new UserRepository(_connectionString);
        var roomRepo = new RoomRepository(_connectionString);

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

        // Act
        await roomRepo.DeleteAsync(room.Id);

        // Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            roomRepo.GetSingle(room.Id));
    }

    [Fact]
    public async Task UpdateContentAsync_ShouldUpdateRoom()
    {
        // Arrange
        var userRepo = new UserRepository(_connectionString);
        var roomRepo = new RoomRepository(_connectionString);

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

        var updatedRoomName = Guid.NewGuid().ToString("N")[..8];

        room.Name = updatedRoomName;

        // Act
        var updatedRoom = await roomRepo.UpdateContentAsync(room);

        // Assert
        Assert.Equal(room.Id, updatedRoom.Id);
        Assert.Equal(updatedRoomName, updatedRoom.Name);
    }

    [Fact]
    public async Task UpdateContentAsync_WithInvalidId_ShouldThrowException()
    {
        // Arrange
        var roomRepo = new RoomRepository(_connectionString);

        var room = new Room
        {
            Id = "invalid_room",
            UserId = "invalid_user",
            Name = "Room"
        };

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            roomRepo.UpdateContentAsync(room));
    }

    [Fact]
    public async Task GetManyAsync_ShouldReturnRooms()
    {
        // Arrange
        var userRepo = new UserRepository(_connectionString);
        var roomRepo = new RoomRepository(_connectionString);

        var user = await userRepo.CreateAsync(new User
        {
            Name = Guid.NewGuid().ToString("N")[..10],
            Email = $"{Guid.NewGuid().ToString("N")[..8]}@t.com",
            PasswordHash = "hashedPassword"
        });

        await roomRepo.CreateAsync(new Room
        {
            UserId = user.Id,
            Name = Guid.NewGuid().ToString("N")[..8]
        });

        // Act
        var rooms = await roomRepo.GetManyAsync();

        // Assert
        Assert.NotEmpty(rooms);
    }
}