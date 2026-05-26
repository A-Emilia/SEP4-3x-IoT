using System.Security.Claims;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Npgsql;
using RepositoryContracts;

namespace Controllers;

[Authorize]
[ApiController]
[Route("rooms")]
public class RoomController : ControllerBase
{
    private readonly IRoomRepository _roomRepo;

    public RoomController(IRoomRepository roomRepository)
    {
        _roomRepo = roomRepository;
    }

    // GET /rooms
    [HttpGet]
    public async Task<IActionResult> GetAllRooms()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized("User id was not found in token.");

        var rooms = await _roomRepo.GetManyByUserIdAsync(userId);

        return Ok(rooms);
    }

    // GET /rooms/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoomById(string id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized("User id was not found in token.");

        try
        {
            var room = await _roomRepo.GetSingleForUserAsync(id, userId);
            return Ok(room);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // POST /rooms
    [HttpPost]
    public async Task<IActionResult> CreateRoom([FromBody] Room room)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized("User id was not found in token.");

        if (string.IsNullOrWhiteSpace(room.Name))
            return BadRequest("Room name is required.");

        if (room.Name.Length > 16)
            return BadRequest("Room name cannot be longer than 16 characters.");

        room.UserId = userId;

        try
        {
            var createdRoom = await _roomRepo.CreateAsync(room);

            return Ok(new
            {
                message = "Room created.",
                room = createdRoom
            });
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict("A room with this id or name already exists.");
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            return BadRequest("The current user does not exist.");
        }
    }

    // PUT /rooms/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoom(string id, [FromBody] Room room)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized("User id was not found in token.");

        if (string.IsNullOrWhiteSpace(room.Name))
            return BadRequest("Room name is required.");

        if (room.Name.Length > 16)
            return BadRequest("Room name cannot be longer than 16 characters.");

        room.Id = id;
        room.UserId = userId;

        try
        {
            var updatedRoom = await _roomRepo.UpdateContentAsync(room);

            return Ok(new
            {
                message = "Room updated.",
                room = updatedRoom
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict("A room with this name already exists.");
        }
    }

    // DELETE /rooms/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(string id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized("User id was not found in token.");

        try
        {
            var deletedRoom = await _roomRepo.DeleteAsync(id, userId);

            return Ok(new
            {
                message = "Room deleted.",
                room = deletedRoom
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            return Conflict("This room cannot be deleted because it still has actuators assigned.");
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst("userId")?.Value
               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
    }
}