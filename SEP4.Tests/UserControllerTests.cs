using Controllers;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RepositoryContracts;

namespace SEP4.Tests;

public class UserControllerTests
{
    [Fact]
    public async Task GetUserById_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();

        mockUserRepo.Setup(r => r.GetSingle("unknownUser"))
            .ThrowsAsync(new KeyNotFoundException("User not found."));

        var controller = new UserController(
            mockUserRepo.Object
        );

        // Act
        var result = await controller.GetUserById("unknownUser");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

        [Fact]
    public async Task UpdateUser_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();

        var controller = new UserController(
            mockUserRepo.Object
        );

        var user = new User
        {
            Name = "",
            Email = "test@test.com"
        };

        // Act
        var result = await controller.UpdateUser("user1", user);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

        [Fact]
    public async Task UpdateUser_WithTooLongName_ShouldReturnBadRequest()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();

        var controller = new UserController(
            mockUserRepo.Object
        );

        var user = new User
        {
            Name = "ThisUserNameIsWayTooLong",
            Email = "test@test.com"
        };

        // Act
        var result = await controller.UpdateUser("user1", user);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

        [Fact]
    public async Task UpdateUser_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();

        var controller = new UserController(
            mockUserRepo.Object
        );

        var user = new User
        {
            Name = "Attila",
            Email = "notAnEmail"
        };

        // Act
        var result = await controller.UpdateUser("user1", user);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

        [Fact]
    public async Task UpdateUser_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();

        var updatedUser = new User
        {
            Id = "user1",
            Name = "Attila",
            Email = "test@test.com"
        };

        mockUserRepo.Setup(r =>
                r.UpdateContentAsync(It.IsAny<User>()))
            .ReturnsAsync(updatedUser);

        var controller = new UserController(
            mockUserRepo.Object
        );

        var user = new User
        {
            Name = "Attila",
            Email = "test@test.com"
        };

        // Act
        var result = await controller.UpdateUser("user1", user);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

        [Fact]
    public async Task DeleteUser_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();

        mockUserRepo.Setup(r => r.DeleteAsync("unknownUser"))
            .ThrowsAsync(new KeyNotFoundException("User not found."));

        var controller = new UserController(
            mockUserRepo.Object
        );

        // Act
        var result = await controller.DeleteUser("unknownUser");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}