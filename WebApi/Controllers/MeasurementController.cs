using Microsoft.AspNetCore.Mvc;
using Repositories;

namespace Controllers;

[ApiController]
[Route("sensor-data")]
public class MeasurementController : ControllerBase
{
    private readonly JSONRepo _store;

    public MeasurementController(JSONRepo store)
    {
        _store = store;
    }

    // GET /sensor-data/current
    [HttpGet("current")]
    public IActionResult GetCurrent()
    {
        var latest = _store.GetLatest();

        if (latest == null)
            return NotFound("No measurements yet.");

        return Ok(latest);
    }

    // GET /sensor-data/history?from=...&to=...
    [HttpGet("history")]
    public IActionResult GetHistoryBasedOnTimestamp(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var measurements = _store.GetAll()
            .Where(m => m.TimestampUtc >= from && m.TimestampUtc <= to)
            .ToList();

        return Ok(measurements);
    }
}