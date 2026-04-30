using Entities;
using Microsoft.AspNetCore.Mvc;
using Repositories;

namespace Controllers;

[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly UserRepo _userRepo;

    public UserController(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    // GET /user/{id}
    [HttpGet("{id}")]
    public IActionResult GetUserById(string id)
    {
        var user = _userRepo.GetById(id);

        if (user == null)
            return NotFound($"User with id '{id}' was not found.");

        return Ok(user);
    }

    // POST /user/create
    [HttpPost("create")]
    public IActionResult CreateUser([FromBody] User user)
    {
        if (string.IsNullOrWhiteSpace(user.Id.ToString()))
            return BadRequest("User id is required.");

        if (string.IsNullOrWhiteSpace(user.Username))
            return BadRequest("User name is required.");

        try
        {
            _userRepo.Add(user);

            return Ok(new
            {
                message = "User created.",
                user
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}