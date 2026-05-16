using Entities;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using RepositoryContracts;
using Microsoft.AspNetCore.Authorization;

namespace Controllers;

[AllowAnonymous]
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly JwtTokenService _jwtService;

    public AuthController(IUserRepository userRepository, JwtTokenService jwtService)
    {
        _userRepo = userRepository;
        _jwtService = jwtService;
    }

    // POST /auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        if (request.Name.Length > 16)
            return BadRequest("Name cannot be longer than 16 characters.");

        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email is required.");

        if (!IsValidEmail(request.Email))
            return BadRequest("Invalid email format.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Password is required.");

        if (request.Password.Length < 6)
            return BadRequest("Password must be at least 6 characters long.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = passwordHash
        };

        try
        {
            var createdUser = await _userRepo.CreateAsync(user);

            return Ok(new
            {
                success = true,
                message = "User registered.",
                user = UserResponse.FromUser(createdUser)
            });
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict("A user with this name or email already exists.");
        }
    }

    // POST /auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email is required.");

        if (!IsValidEmail(request.Email))
            return BadRequest("Invalid email format.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Password is required.");

        var user = await _userRepo.GetByEmailAsync(request.Email);

        if (user == null)
            return Unauthorized("Invalid email or password.");

        var passwordIsValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash
        );

        if (!passwordIsValid)
            return Unauthorized("Invalid email or password.");

        var token = _jwtService.CreateToken(user);

        return Ok(new
        {
            success = true,
            message = "Login successful.",
            user = UserResponse.FromUser(user),
            token = token
        });
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}