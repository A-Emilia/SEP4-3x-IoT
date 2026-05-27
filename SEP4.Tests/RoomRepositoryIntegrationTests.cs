using Entities;
using Moq;
using RepositoryContracts;

namespace SEP4.Tests;

public class RoomRepositoryIntegrationTests
{
    [Fact]
    public async Task GetSingle_ShouldReturnRoom()
    {
        // Arrange
        var fakeRoom = new Room
        {
            Id = "room1",
            UserId = "user1",
            Name = "Living Room"
        };

        var mockRoomRepo = new Mock<IRoomRepository>();

        mockRoomRepo.Setup(r =>
                r.GetSingle("room1"))
            .ReturnsAsync(fakeRoom);

        // Act
        var fetchedRoom = await mockRoomRepo.Object.GetSingle("room1");

        // Assert
        Assert.Equal(fakeRoom.Id, fetchedRoom.Id);
        Assert.Equal(fakeRoom.Name, fetchedRoom.Name);
    }

    [Fact]
    public async Task GetSingle_WithInvalidId_ShouldThrowException()
    {
        // Arrange
        var mockRoomRepo = new Mock<IRoomRepository>();

        mockRoomRepo.Setup(r =>
                r.GetSingle(It.IsAny<string>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            mockRoomRepo.Object.GetSingle("invalid_room"));
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteRoom()
    {
        // Arrange
        var deletedRoom = new Room
        {
            Id = "room1",
            UserId = "user1",
            Name = "Living Room"
        };

        var mockRoomRepo = new Mock<IRoomRepository>();

        mockRoomRepo.Setup(r =>
                r.DeleteAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
            .ReturnsAsync(deletedRoom);

        // Act
        var result = await mockRoomRepo.Object.DeleteAsync(
            "room1",
            "user1"
        );

        // Assert
        Assert.Equal(deletedRoom.Id, result.Id);
    }

    [Fact]
    public async Task UpdateContentAsync_ShouldUpdateRoom()
    {
        // Arrange
        var updatedRoom = new Room
        {
            Id = "room1",
            UserId = "user1",
            Name = "Updated Room"
        };

        var mockRoomRepo = new Mock<IRoomRepository>();

        mockRoomRepo.Setup(r =>
                r.UpdateContentAsync(It.IsAny<Room>()))
            .ReturnsAsync(updatedRoom);

        // Act
        var result = await mockRoomRepo.Object.UpdateContentAsync(updatedRoom);

        // Assert
        Assert.Equal(updatedRoom.Id, result.Id);
        Assert.Equal(updatedRoom.Name, result.Name);
    }

    [Fact]
    public async Task UpdateContentAsync_WithInvalidId_ShouldThrowException()
    {
        // Arrange
        var mockRoomRepo = new Mock<IRoomRepository>();

        mockRoomRepo.Setup(r =>
                r.UpdateContentAsync(It.IsAny<Room>()))
            .ThrowsAsync(new KeyNotFoundException());

        var room = new Room
        {
            Id = "invalid_room",
            UserId = "invalid_user",
            Name = "Room"
        };

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            mockRoomRepo.Object.UpdateContentAsync(room));
    }

    [Fact]
    public async Task GetManyByUserIdAsync_ShouldReturnRooms()
    {
        // Arrange
        var rooms = new List<Room>
    {
        new Room
        {
            Id = "room1",
            UserId = "user1",
            Name = "Living Room"
        },
        new Room
        {
            Id = "room2",
            UserId = "user1",
            Name = "Kitchen"
        }
    };

        var mockRoomRepo = new Mock<IRoomRepository>();

        mockRoomRepo.Setup(r =>
                r.GetManyByUserIdAsync(It.IsAny<string>()))
            .ReturnsAsync(rooms);

        // Act
        var result = await mockRoomRepo.Object
            .GetManyByUserIdAsync("user1");

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
    }
}