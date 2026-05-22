using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace Controllers;

[Authorize]
[ApiController]
[Route("device-logs")]
public class DeviceActionLogController : ControllerBase
{
    private readonly IDeviceActionLogRepository _actionLogRepo;

    public DeviceActionLogController(IDeviceActionLogRepository actionLogRepository)
    {
        _actionLogRepo = actionLogRepository;
    }

    // GET /device-logs
    [HttpGet]
    public async Task<IActionResult> GetAllDeviceLogs()
    {
        var logs = await _actionLogRepo.GetAllAsync();

        return Ok(logs);
    }

    // GET /device-logs/room/{roomId}
    [HttpGet("room/{roomId}")]
    public async Task<IActionResult> GetDeviceLogsByRoom(string roomId)
    {
        if (string.IsNullOrWhiteSpace(roomId))
            return BadRequest("Room id is required.");

        var logs = await _actionLogRepo.GetByRoomIdAsync(roomId);

        return Ok(logs);
    }

    // GET /device-logs/timestamp?from=2026-05-01T00:00:00Z&to=2026-05-22T23:59:59Z
    [HttpGet("timestamp")]
    public async Task<IActionResult> GetDeviceLogsByTimestamp(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        if (from == default)
            return BadRequest("'from' query parameter is required.");

        if (to == default)
            return BadRequest("'to' query parameter is required.");

        if (from > to)
            return BadRequest("'from' cannot be later than 'to'.");

        var logs = await _actionLogRepo.GetByTimestampAsync(from, to);

        return Ok(logs);
    }

    // GET /device-logs/room/{roomId}/timestamp?from=2026-05-01T00:00:00Z&to=2026-05-22T23:59:59Z
    [HttpGet("room/{roomId}/timestamp")]
    public async Task<IActionResult> GetDeviceLogsByRoomAndTimestamp(
        string roomId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        if (string.IsNullOrWhiteSpace(roomId))
            return BadRequest("Room id is required.");

        if (from == default)
            return BadRequest("'from' query parameter is required.");

        if (to == default)
            return BadRequest("'to' query parameter is required.");

        if (from > to)
            return BadRequest("'from' cannot be later than 'to'.");

        var logs = await _actionLogRepo.GetByRoomIdAndTimestampAsync(roomId, from, to);

        return Ok(logs);
    }
}