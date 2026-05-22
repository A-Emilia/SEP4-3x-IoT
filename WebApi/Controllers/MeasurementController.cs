using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RepositoryContracts;

namespace Controllers;

[Authorize]
[ApiController]
[Route("sensor-data")]
public class MeasurementController : ControllerBase
{
    private const string SharedMeasurementRoomId = "shared";

    private readonly IMeasurementRepository _measurementRepo;
    private readonly IRoomRepository _roomRepo;

    public MeasurementController(
        IMeasurementRepository measurementRepo,
        IRoomRepository roomRepo)
    {
        _measurementRepo = measurementRepo;
        _roomRepo = roomRepo;
    }

    // GET /sensor-data/current?roomId={roomId}
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentAsync([FromQuery] string roomId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized("User id was not found in token.");

        if (string.IsNullOrWhiteSpace(roomId))
            return BadRequest("'roomId' query parameter is required.");

        try
        {
            await _roomRepo.GetSingleForUserAsync(roomId, userId);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Room was not found for this user.");
        }

        var latest = await _measurementRepo.GetMostRecent(SharedMeasurementRoomId);

        if (latest == null)
            return NotFound("No measurements yet.");

        return Ok(latest);
    }

    // GET /sensor-data/history?roomId={roomId}&from=&to=
    [HttpGet("history")]
    public async Task<IActionResult> GetHistoryBasedOnTimestamp(
        [FromQuery] string roomId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized("User id was not found in token.");

        if (string.IsNullOrWhiteSpace(roomId))
            return BadRequest("'roomId' query parameter is required.");

        if (from == default)
            return BadRequest("'from' query parameter is required.");

        if (to == default)
            return BadRequest("'to' query parameter is required.");

        if (from > to)
            return BadRequest("'from' cannot be later than 'to'.");

        try
        {
            await _roomRepo.GetSingleForUserAsync(roomId, userId);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Room was not found for this user.");
        }

        var measurements = await _measurementRepo.GetMany(
            SharedMeasurementRoomId,
            from,
            to
        );

        return Ok(measurements);
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst("userId")?.Value
               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
    }
}