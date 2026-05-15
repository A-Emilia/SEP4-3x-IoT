using Entities;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using RepositoryContracts;

namespace Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepo;

    public AuthController(IUserRepository userRepository)
    {
        _userRepo = userRepository;
    }

    // POST /auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        if (request.Name.Length > 16)
            return BadRequest("Name cannot be longer than 16 characters.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Password is required.");

        if (request.Password.Length < 6)
            return BadRequest("Password must be at least 6 characters long.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Name = request.Name,
            PasswordHash = passwordHash
        };

        try
        {
            var createdUser = await _userRepo.CreateAsync(user);

            return Ok(new
            {
                message = "User registered.",
                user = UserResponse.FromUser(createdUser)
            });
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict("A user with this name already exists.");
        }
    }

    // POST /auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Password is required.");

        var user = await _userRepo.GetByNameAsync(request.Name);

        if (user == null)
            return Unauthorized("Invalid name or password.");

        var passwordIsValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash
        );

        if (!passwordIsValid)
            return Unauthorized("Invalid name or password.");

        return Ok(new
        {
            message = "Login successful.",
            user = UserResponse.FromUser(user)
        });
    }
}