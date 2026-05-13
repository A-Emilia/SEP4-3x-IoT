using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace Controllers;

[ApiController]
[Route("sensor-data")]
public class MeasurementController : ControllerBase
{
    private readonly IMeasurementRepository _measurementRepo;

    public MeasurementController(IMeasurementRepository measurementRepo)
    {
        _measurementRepo = measurementRepo;
    }

    // GET /sensor-data/current
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentAsync()
    {
        var latest = await _measurementRepo.GetMostRecent();

        if (latest == null)
            return NotFound("No measurements yet.");

        return Ok(latest);
    }

    // GET /sensor-data/history?from=&to=
    // For example: /sensor-data/history?from=2026-04-22T00:00:00Z&to=2026-04-22T23:59:59Z
    [HttpGet("history")]
    public async Task<IActionResult> GetHistoryBasedOnTimestamp(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        if (from == default)
            return BadRequest("'from' query parameter is required.");

        if (to == default)
            return BadRequest("'to' query parameter is required.");

        if (from > to)
            return BadRequest("'from' cannot be later than 'to'.");

        var measurements = await _measurementRepo.GetMany(from, to);

        if (measurements.Count == 0)
            return NotFound("No measurements found in the given time period.");

        return Ok(measurements);
    }
}