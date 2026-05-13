using Entities;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using RepositoryContracts;

namespace Controllers;

[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    // GET /user/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        try
        {
            var user = await _userRepository.GetSingle(id);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // POST /user/create
    [HttpPost("create")]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
            return BadRequest("User id is required.");

        if (string.IsNullOrWhiteSpace(user.Name))
            return BadRequest("User name is required.");

        try
        {
            var createdUser = await _userRepository.CreateAsync(user);

            return Ok(new
            {
                message = "User created.",
                user = createdUser
            });
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict("A user with this id or name already exists.");
        }
    }

    // PUT /user/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] User user)
    {
        if (string.IsNullOrWhiteSpace(user.Name))
            return BadRequest("User name is required.");

        user.Id = id;

        try
        {
            var updatedUser = await _userRepository.UpdateContentAsync(user);

            return Ok(new
            {
                message = "User updated.",
                user = updatedUser
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
            var deletedUser = await _userRepository.DeleteAsync(id);

            return Ok(new
            {
                message = "User deleted.",
                user = deletedUser
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        //TODO add room deletion here aswell?
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            return Conflict("This user cannot be deleted because they still have rooms assigned.");
        }
    }
}