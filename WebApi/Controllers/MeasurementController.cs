using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RepositoryContracts;

namespace Controllers;

[Authorize]
[ApiController]
[Route("sensor-data")]
public class MeasurementController : ControllerBase
{
    private readonly IMeasurementRepository _measurementRepo;

    public MeasurementController(IMeasurementRepository measurementRepo)
    {
        _measurementRepo = measurementRepo;
    }

    // GET /sensor-data/current?roomId={roomId}
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentAsync([FromQuery] string roomId)
    {
        if (string.IsNullOrWhiteSpace(roomId))
            return BadRequest("'roomId' query parameter is required.");

        var latest = await _measurementRepo.GetMostRecent(roomId);

        if (latest == null)
            return NotFound("No measurements yet for this room.");

        return Ok(latest);
    }

    // GET /sensor-data/history?roomId={roomId}&from=&to=
    [HttpGet("history")]
    public async Task<IActionResult> GetHistoryBasedOnTimestamp(
        [FromQuery] string roomId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        if (string.IsNullOrWhiteSpace(roomId))
            return BadRequest("'roomId' query parameter is required.");

        if (from == default)
            return BadRequest("'from' query parameter is required.");

        if (to == default)
            return BadRequest("'to' query parameter is required.");

        if (from > to)
            return BadRequest("'from' cannot be later than 'to'.");

        var measurements = await _measurementRepo.GetMany(roomId, from, to);

        return Ok(measurements);
    }
}