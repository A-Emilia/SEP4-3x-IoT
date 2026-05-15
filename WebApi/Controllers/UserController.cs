using Entities;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using RepositoryContracts;

namespace Controllers;

[Authorize]
[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepo;

    public UserController(IUserRepository userRepository)
    {
        _userRepo = userRepository;
    }

    // GET /user/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        try
        {
            var user = await _userRepo.GetSingle(id);
            return Ok(UserResponse.FromUser(user));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // PUT /user/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] User user)
    {
        if (string.IsNullOrWhiteSpace(user.Name))
            return BadRequest("User name is required.");

        if (user.Name.Length > 16)
            return BadRequest("User name cannot be longer than 16 characters.");

        user.Id = id;

        try
        {
            var updatedUser = await _userRepo.UpdateContentAsync(user);

            return Ok(new
            {
                message = "User updated.",
                user = UserResponse.FromUser(updatedUser)
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict("A user with this name already exists.");
        }
    }

    // DELETE /user/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            var deletedUser = await _userRepo.DeleteAsync(id);

            return Ok(new
            {
                message = "User deleted.",
                user = UserResponse.FromUser(deletedUser)
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            return Conflict("This user cannot be deleted because they still have rooms assigned.");
        }
    }
}