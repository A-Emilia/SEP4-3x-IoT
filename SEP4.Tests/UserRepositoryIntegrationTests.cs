using Entities;
using Moq;
using RepositoryContracts;

namespace SEP4.Tests;

public class UserRepositoryIntegrationTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateUser()
    {
        // Arrange
        var fakeUser = new User
        {
            Id = "user1",
            Name = "Attila",
            Email = "test@test.com",
            PasswordHash = "hashedPassword"
        };

        var mockUserRepo = new Mock<IUserRepository>();

        mockUserRepo.Setup(r =>
                r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(fakeUser);

        // Act
        var createdUser = await mockUserRepo.Object.CreateAsync(fakeUser);

        // Assert
        Assert.NotNull(createdUser);
        Assert.Equal(fakeUser.Name, createdUser.Name);
        Assert.Equal(fakeUser.Email, createdUser.Email);
        Assert.False(string.IsNullOrWhiteSpace(createdUser.Id));
    }

    [Fact]
    public async Task GetSingle_ShouldReturnUser()
    {
        // Arrange
        var fakeUser = new User
        {
            Id = "user1",
            Name = "Attila",
            Email = "test@test.com",
            PasswordHash = "hashedPassword"
        };

        var mockUserRepo = new Mock<IUserRepository>();

        mockUserRepo.Setup(r =>
                r.GetSingle("user1"))
            .ReturnsAsync(fakeUser);

        // Act
        var fetchedUser = await mockUserRepo.Object.GetSingle("user1");

        // Assert
        Assert.NotNull(fetchedUser);
        Assert.Equal(fakeUser.Id, fetchedUser.Id);
        Assert.Equal(fakeUser.Name, fetchedUser.Name);
        Assert.Equal(fakeUser.Email, fetchedUser.Email);
    }

    [Fact]
    public async Task GetSingle_WithInvalidId_ShouldThrowException()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();

        mockUserRepo.Setup(r =>
                r.GetSingle(It.IsAny<string>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            mockUserRepo.Object.GetSingle("invalid_id"));
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser()
    {
        // Arrange
        var fakeUser = new User
        {
            Id = "user1",
            Name = "Attila",
            Email = "test@test.com",
            PasswordHash = "hashedPassword"
        };

        var mockUserRepo = new Mock<IUserRepository>();

        mockUserRepo.Setup(r =>
                r.GetByEmailAsync(fakeUser.Email))
            .ReturnsAsync(fakeUser);

        // Act
        var fetchedUser = await mockUserRepo.Object
            .GetByEmailAsync(fakeUser.Email);

        // Assert
        Assert.NotNull(fetchedUser);
        Assert.Equal(fakeUser.Email, fetchedUser!.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_WithInvalidEmail_ShouldReturnNull()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();

        mockUserRepo.Setup(r =>
                r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await mockUserRepo.Object
            .GetByEmailAsync("doesnotexist@test.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteUser()
    {
        // Arrange
        var deletedUser = new User
        {
            Id = "user1",
            Name = "Attila",
            Email = "test@test.com",
            PasswordHash = "hashedPassword"
        };

        var mockUserRepo = new Mock<IUserRepository>();

        mockUserRepo.Setup(r =>
                r.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(deletedUser);

        // Act
        var result = await mockUserRepo.Object
            .DeleteAsync("user1");

        // Assert
        Assert.Equal(deletedUser.Id, result.Id);
    }

    [Fact]
    public async Task UpdateContentAsync_ShouldUpdateUser()
    {
        // Arrange
        var updatedUser = new User
        {
            Id = "user1",
            Name = "UpdatedAttila",
            Email = "updated@test.com",
            PasswordHash = "hashedPassword"
        };

        var mockUserRepo = new Mock<IUserRepository>();

        mockUserRepo.Setup(r =>
                r.UpdateContentAsync(It.IsAny<User>()))
            .ReturnsAsync(updatedUser);

        // Act
        var result = await mockUserRepo.Object
            .UpdateContentAsync(updatedUser);

        // Assert
        Assert.Equal(updatedUser.Name, result.Name);
        Assert.Equal(updatedUser.Email, result.Email);
    }
}