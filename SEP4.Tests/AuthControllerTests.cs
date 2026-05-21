using Controllers;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RepositoryContracts;
using Microsoft.Extensions.Configuration;
using Entities;

namespace SEP4.Tests;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_WithEmptyName_ShouldReturnBadRequest()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Jwt:Key", "ThisIsASuperSecretKeyForTesting123"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"},
            {"Jwt:ExpiresInMinutes", "60"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var jwtService = new JwtTokenService(configuration);

        var controller = new AuthController(
            mockRepo.Object,
            jwtService
        );

        var request = new RegisterRequest
        {
            Name = "",
            Email = "test@test.com",
            Password = "123456"
        };

        // Act
        var result = await controller.Register(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_WithShortPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();

        var inMemorySettings = new Dictionary<string, string?>
    {
        {"Jwt:Key", "ThisIsASuperSecretKeyForTesting123"},
        {"Jwt:Issuer", "TestIssuer"},
        {"Jwt:Audience", "TestAudience"},
        {"Jwt:ExpiresInMinutes", "60"}
    };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var jwtService = new JwtTokenService(configuration);

        var controller = new AuthController(
            mockRepo.Object,
            jwtService
        );

        var request = new RegisterRequest
        {
            Name = "Attila",
            Email = "test@test.com",
            Password = "123"
        };

        // Act
        var result = await controller.Register(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();

        var inMemorySettings = new Dictionary<string, string?>
    {
        {"Jwt:Key", "ThisIsASuperSecretKeyForTesting123"},
        {"Jwt:Issuer", "TestIssuer"},
        {"Jwt:Audience", "TestAudience"},
        {"Jwt:ExpiresInMinutes", "60"}
    };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var jwtService = new JwtTokenService(configuration);

        var controller = new AuthController(
            mockRepo.Object,
            jwtService
        );

        var request = new RegisterRequest
        {
            Name = "Meowzers",
            Email = "notAnEmail",
            Password = "123456"
        };

        // Act
        var result = await controller.Register(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_WithTooLongName_ShouldReturnBadRequest()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();

        var inMemorySettings = new Dictionary<string, string?>
    {
        {"Jwt:Key", "ThisIsASuperSecretKeyForTesting123"},
        {"Jwt:Issuer", "TestIssuer"},
        {"Jwt:Audience", "TestAudience"},
        {"Jwt:ExpiresInMinutes", "60"}
    };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var jwtService = new JwtTokenService(configuration);

        var controller = new AuthController(
            mockRepo.Object,
            jwtService
        );

        var request = new RegisterRequest
        {
            Name = "ThisNameIsWayTooLong",
            Email = "test@test.com",
            Password = "123456"
        };

        // Act
        var result = await controller.Register(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_WithUnknownUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();

        mockRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var inMemorySettings = new Dictionary<string, string?>
    {
        {"Jwt:Key", "ThisIsASuperSecretKeyForTesting123"},
        {"Jwt:Issuer", "TestIssuer"},
        {"Jwt:Audience", "TestAudience"},
        {"Jwt:ExpiresInMinutes", "60"}
    };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var jwtService = new JwtTokenService(configuration);

        var controller = new AuthController(
            mockRepo.Object,
            jwtService
        );

        var request = new LoginRequest
        {
            Email = "unknown@test.com",
            Password = "magyarpeter"
        };

        // Act
        var result = await controller.Login(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctPassword");

        var fakeUser = new User
        {
            Id = "1",
            Name = "Attila",
            Email = "test@test.com",
            PasswordHash = hashedPassword
        };

        mockRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(fakeUser);

        var inMemorySettings = new Dictionary<string, string?>
    {
        {"Jwt:Key", "ThisIsASuperSecretKeyForTesting123"},
        {"Jwt:Issuer", "TestIssuer"},
        {"Jwt:Audience", "TestAudience"},
        {"Jwt:ExpiresInMinutes", "60"}
    };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var jwtService = new JwtTokenService(configuration);

        var controller = new AuthController(
            mockRepo.Object,
            jwtService
        );

        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "wrongPassword"
        };

        // Act
        var result = await controller.Login(request);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOk()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctPassword");

        var fakeUser = new User
        {
            Id = "1",
            Name = "Attila",
            Email = "test@test.com",
            PasswordHash = hashedPassword
        };

        mockRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(fakeUser);

        var inMemorySettings = new Dictionary<string, string?>
    {
        {"Jwt:Key", "ThisIsASuperSecretKeyForTesting123"},
        {"Jwt:Issuer", "TestIssuer"},
        {"Jwt:Audience", "TestAudience"},
        {"Jwt:ExpiresInMinutes", "60"}
    };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var jwtService = new JwtTokenService(configuration);

        var controller = new AuthController(
            mockRepo.Object,
            jwtService
        );

        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "correctPassword"
        };

        // Act
        var result = await controller.Login(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();

        var fakeCreatedUser = new User
        {
            Id = "1",
            Name = "Attila",
            Email = "test@test.com",
            PasswordHash = "hashedPassword"
        };

        mockRepo.Setup(r => r.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(fakeCreatedUser);

        var inMemorySettings = new Dictionary<string, string?>
    {
        {"Jwt:Key", "ThisIsASuperSecretKeyForTesting123"},
        {"Jwt:Issuer", "TestIssuer"},
        {"Jwt:Audience", "TestAudience"},
        {"Jwt:ExpiresInMinutes", "60"}
    };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var jwtService = new JwtTokenService(configuration);

        var controller = new AuthController(
            mockRepo.Object,
            jwtService
        );

        var request = new RegisterRequest
        {
            Name = "Attila",
            Email = "test@test.com",
            Password = "123456"
        };

        // Act
        var result = await controller.Register(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Register_WithEmptyEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();

        var inMemorySettings = new Dictionary<string, string?>
    {
        {"Jwt:Key", "ThisIsASuperSecretKeyForTesting123"},
        {"Jwt:Issuer", "TestIssuer"},
        {"Jwt:Audience", "TestAudience"},
        {"Jwt:ExpiresInMinutes", "60"}
    };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var jwtService = new JwtTokenService(configuration);

        var controller = new AuthController(
            mockRepo.Object,
            jwtService
        );

        var request = new RegisterRequest
        {
            Name = "Attila",
            Email = "",
            Password = "123456"
        };

        // Act
        var result = await controller.Register(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_WithEmptyPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();

        var inMemorySettings = new Dictionary<string, string?>
    {
        {"Jwt:Key", "ThisIsASuperSecretKeyForTesting123"},
        {"Jwt:Issuer", "TestIssuer"},
        {"Jwt:Audience", "TestAudience"},
        {"Jwt:ExpiresInMinutes", "60"}
    };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var jwtService = new JwtTokenService(configuration);

        var controller = new AuthController(
            mockRepo.Object,
            jwtService
        );

        var request = new RegisterRequest
        {
            Name = "Attila",
            Email = "test@test.com",
            Password = ""
        };

        // Act
        var result = await controller.Register(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_WithEmptyEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();

        var inMemorySettings = new Dictionary<string, string?>
    {
        {"Jwt:Key", "ThisIsASuperSecretKeyForTesting123"},
        {"Jwt:Issuer", "TestIssuer"},
        {"Jwt:Audience", "TestAudience"},
        {"Jwt:ExpiresInMinutes", "60"}
    };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var jwtService = new JwtTokenService(configuration);

        var controller = new AuthController(
            mockRepo.Object,
            jwtService
        );

        var request = new LoginRequest
        {
            Email = "",
            Password = "123456"
        };

        // Act
        var result = await controller.Login(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var mockRepo = new Mock<IUserRepository>();

        var inMemorySettings = new Dictionary<string, string?>
    {
        {"Jwt:Key", "ThisIsASuperSecretKeyForTesting123"},
        {"Jwt:Issuer", "TestIssuer"},
        {"Jwt:Audience", "TestAudience"},
        {"Jwt:ExpiresInMinutes", "60"}
    };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var jwtService = new JwtTokenService(configuration);

        var controller = new AuthController(
            mockRepo.Object,
            jwtService
        );

        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = ""
        };

        // Act
        var result = await controller.Login(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}