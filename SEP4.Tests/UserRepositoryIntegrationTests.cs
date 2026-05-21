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
}