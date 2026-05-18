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
        var rooms = await _roomRepo.GetManyAsync();

        return Ok(rooms);
    }

    // GET /rooms/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoomById(string id)
    {
        try
        {
            var room = await _roomRepo.GetSingle(id);
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
        if (string.IsNullOrWhiteSpace(room.UserId))
            return BadRequest("User id is required.");

        if (string.IsNullOrWhiteSpace(room.Name))
            return BadRequest("Room name is required.");

        if (room.Name.Length > 16)
            return BadRequest("Room name cannot be longer than 16 characters.");

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
        //TODO chain deletion here?
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            return BadRequest("The given user id does not exist.");
        }
    }

    // PUT /rooms/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRoom(string id, [FromBody] Room room)
    {
        if (string.IsNullOrWhiteSpace(room.UserId))
            return BadRequest("User id is required.");

        if (string.IsNullOrWhiteSpace(room.Name))
            return BadRequest("Room name is required.");

        if (room.Name.Length > 16)
            return BadRequest("Room name cannot be longer than 16 characters.");

        room.Id = id;

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
        //TODO might need to implement stuff into postgre aswell
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            return BadRequest("The given user id does not exist.");
        }
    }

    // DELETE /rooms/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoom(string id)
    {
        try
        {
            var deletedRoom = await _roomRepo.DeleteAsync(id);

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
        //TODO chain deletion?
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            return Conflict("This room cannot be deleted because it still has actuators assigned.");
        }
    }
}