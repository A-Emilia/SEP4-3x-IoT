using Controllers;
using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RepositoryContracts;
using System.Security.Claims;

namespace SEP4.Tests;

public class RoomControllerTests
{
    private static void SetupUser(ControllerBase controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "test-user-id")
                }, "mock"))
            }
        };
    }

    [Fact]
    public async Task CreateRoom_WithMissingUserId_ShouldReturnBadRequest()
    {
        // Arrange
        var mockRoomRepo = new Mock<IRoomRepository>();

        var controller = new RoomController(
            mockRoomRepo.Object
        );

        SetupUser(controller);

        var room = new Room
        {
            UserId = "",
            Name = "Living Room"
        };

        // Act
        var result = await controller.CreateRoom(room);

        // Assert
        Assert.IsType<OkObjectResult>(result); ;
    }

    [Fact]
    public async Task CreateRoom_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var mockRoomRepo = new Mock<IRoomRepository>();

        var controller = new RoomController(
            mockRoomRepo.Object
        );

        SetupUser(controller);

        var room = new Room
        {
            UserId = "user1",
            Name = ""
        };

        // Act
        var result = await controller.CreateRoom(room);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateRoom_WithTooLongName_ShouldReturnBadRequest()
    {
        // Arrange
        var mockRoomRepo = new Mock<IRoomRepository>();

        var controller = new RoomController(
            mockRoomRepo.Object
        );

        SetupUser(controller);

        var room = new Room
        {
            UserId = "user1",
            Name = "ThisRoomNameIsWayTooLong"
        };

        // Act
        var result = await controller.CreateRoom(room);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateRoom_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var mockRoomRepo = new Mock<IRoomRepository>();

        var fakeRoom = new Room
        {
            Id = "room1",
            UserId = "user1",
            Name = "Living Room"
        };

        mockRoomRepo.Setup(r =>
                r.CreateAsync(It.IsAny<Room>()))
            .ReturnsAsync(fakeRoom);

        var controller = new RoomController(
            mockRoomRepo.Object
        );

        SetupUser(controller);

        var room = new Room
        {
            UserId = "user1",
            Name = "Living Room"
        };

        // Act
        var result = await controller.CreateRoom(room);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetRoomById_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        var mockRoomRepo = new Mock<IRoomRepository>();

        mockRoomRepo.Setup(r =>
         r.GetSingle(It.IsAny<string>()))
             .ThrowsAsync(new KeyNotFoundException("Room not found."));

        var controller = new RoomController(
            mockRoomRepo.Object
        );

        SetupUser(controller);

        // Act
        var result = await controller.GetRoomById("unknownRoom");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteRoom_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        var mockRoomRepo = new Mock<IRoomRepository>();

        mockRoomRepo.Setup(r =>
                r.DeleteAsync("unknownRoom", It.IsAny<string>()))
            .ThrowsAsync(new KeyNotFoundException("Room not found."));

        var controller = new RoomController(
            mockRoomRepo.Object
        );

        SetupUser(controller);

        // Act
        var result = await controller.DeleteRoom("unknownRoom");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateRoom_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var mockRoomRepo = new Mock<IRoomRepository>();

        var updatedRoom = new Room
        {
            Id = "room1",
            UserId = "user1",
            Name = "Updated Room"
        };

        mockRoomRepo.Setup(r =>
                r.UpdateContentAsync(It.IsAny<Room>()))
            .ReturnsAsync(updatedRoom);

        var controller = new RoomController(
            mockRoomRepo.Object
        );

        SetupUser(controller);

        var room = new Room
        {
            UserId = "user1",
            Name = "Updated Room"
        };

        // Act
        var result = await controller.UpdateRoom("room1", room);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateRoom_WithTooLongName_ShouldReturnBadRequest()
    {
        // Arrange
        var mockRoomRepo = new Mock<IRoomRepository>();

        var controller = new RoomController(
            mockRoomRepo.Object
        );

        SetupUser(controller);

        var room = new Room
        {
            UserId = "user1",
            Name = "ThisRoomNameIsDefinitelyTooLong"
        };

        // Act
        var result = await controller.UpdateRoom("room1", room);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteRoom_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var mockRoomRepo = new Mock<IRoomRepository>();

        var deletedRoom = new Room
        {
            Id = "room1",
            UserId = "user1",
            Name = "Living Room"
        };

        mockRoomRepo.Setup(r =>
                r.DeleteAsync("room1", It.IsAny<string>()))
            .ReturnsAsync(deletedRoom);

        var controller = new RoomController(
            mockRoomRepo.Object
        );

        SetupUser(controller);

        // Act
        var result = await controller.DeleteRoom("room1");

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateUser_WithEmptyEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();

        var controller = new UserController(
            mockUserRepo.Object
        );

        var user = new User
        {
            Name = "Attila",
            Email = ""
        };

        // Act
        var result = await controller.UpdateUser("user1", user);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateUser_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();

        mockUserRepo.Setup(r =>
                r.UpdateContentAsync(It.IsAny<User>()))
            .ThrowsAsync(new KeyNotFoundException("User not found."));

        var controller = new UserController(
            mockUserRepo.Object
        );

        var user = new User
        {
            Name = "Attila",
            Email = "test@test.com"
        };

        // Act
        var result = await controller.UpdateUser("unknownUser", user);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteUser_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();

        var deletedUser = new User
        {
            Id = "user1",
            Name = "Attila",
            Email = "test@test.com"
        };

        mockUserRepo.Setup(r => r.DeleteAsync("user1"))
            .ReturnsAsync(deletedUser);

        var controller = new UserController(
            mockUserRepo.Object
        );

        // Act
        var result = await controller.DeleteUser("user1");

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetUserById_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();

        var fakeUser = new User
        {
            Id = "user1",
            Name = "Attila",
            Email = "test@test.com"
        };

        mockUserRepo.Setup(r => r.GetSingle("user1"))
            .ReturnsAsync(fakeUser);

        var controller = new UserController(
            mockUserRepo.Object
        );

        // Act
        var result = await controller.GetUserById("user1");

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}
