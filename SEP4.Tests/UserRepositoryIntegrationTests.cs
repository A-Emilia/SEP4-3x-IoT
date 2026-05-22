using Entities;
using Repositories.PostgreSQL;

namespace SEP4.Tests;

public class UserRepositoryIntegrationTests
{
    private readonly string _connectionString =
        "Host=localhost;Port=1324;Database=user_data;Username=postgres;Password=postgres";

    [Fact]
    public async Task CreateAsync_ShouldCreateUser()
    {
        // Arrange
        var repo = new UserRepository(_connectionString);

        var user = new User
        {
            Name = Guid.NewGuid().ToString("N")[..10],
            Email = $"{Guid.NewGuid().ToString("N")[..8]}@t.com",
            PasswordHash = "hashedPassword"
        };

        // Act
        var createdUser = await repo.CreateAsync(user);

        // Assert
        Assert.NotNull(createdUser);
        Assert.Equal(user.Name, createdUser.Name);
        Assert.Equal(user.Email, createdUser.Email);
        Assert.False(string.IsNullOrWhiteSpace(createdUser.Id));
    }

    [Fact]
    public async Task GetSingle_ShouldReturnUser()
    {
        // Arrange
        var repo = new UserRepository(_connectionString);

        var user = new User
        {
            Name = Guid.NewGuid().ToString("N")[..10],
            Email = $"{Guid.NewGuid().ToString("N")[..8]}@t.com",
            PasswordHash = "hashedPassword"
        };

        var createdUser = await repo.CreateAsync(user);

        // Act
        var fetchedUser = await repo.GetSingle(createdUser.Id);

        // Assert
        Assert.NotNull(fetchedUser);
        Assert.Equal(createdUser.Id, fetchedUser.Id);
        Assert.Equal(createdUser.Name, fetchedUser.Name);
        Assert.Equal(createdUser.Email, fetchedUser.Email);
    }

    [Fact]
    public async Task GetSingle_WithInvalidId_ShouldThrowException()
    {
        // Arrange
        var repo = new UserRepository(_connectionString);

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            repo.GetSingle("invalid_id"));
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser()
    {
        // Arrange
        var repo = new UserRepository(_connectionString);

        var user = new User
        {
            Name = Guid.NewGuid().ToString("N")[..10],
            Email = $"{Guid.NewGuid().ToString("N")[..8]}@t.com",
            PasswordHash = "hashedPassword"
        };

        var createdUser = await repo.CreateAsync(user);

        // Act
        var fetchedUser = await repo.GetByEmailAsync(createdUser.Email);

        // Assert
        Assert.NotNull(fetchedUser);
        Assert.Equal(createdUser.Email, fetchedUser!.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_WithInvalidEmail_ShouldReturnNull()
    {
        // Arrange
        var repo = new UserRepository(_connectionString);

        // Act
        var result = await repo.GetByEmailAsync("doesnotexist@test.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteUser()
    {
        // Arrange
        var repo = new UserRepository(_connectionString);

        var user = new User
        {
            Name = Guid.NewGuid().ToString("N")[..10],
            Email = $"{Guid.NewGuid().ToString("N")[..8]}@t.com",
            PasswordHash = "hashedPassword"
        };

        var createdUser = await repo.CreateAsync(user);

        // Act
        var deletedUser = await repo.DeleteAsync(createdUser.Id);

        // Assert
        Assert.Equal(createdUser.Id, deletedUser.Id);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            repo.GetSingle(createdUser.Id));
    }

    [Fact]
    public async Task UpdateContentAsync_ShouldUpdateUser()
    {
        // Arrange
        var repo = new UserRepository(_connectionString);

        var user = new User
        {
            Name = Guid.NewGuid().ToString("N")[..10],
            Email = $"{Guid.NewGuid().ToString("N")[..8]}@t.com",
            PasswordHash = "hashedPassword"
        };

        var createdUser = await repo.CreateAsync(user);

        createdUser.Name = Guid.NewGuid().ToString("N")[..10];
        createdUser.Email = $"{Guid.NewGuid().ToString("N")[..8]}@t.com";

        // Act
        var updatedUser = await repo.UpdateContentAsync(createdUser);

        // Assert
        Assert.Equal(createdUser.Name, updatedUser.Name);
        Assert.Equal(createdUser.Email, updatedUser.Email);
    }
}