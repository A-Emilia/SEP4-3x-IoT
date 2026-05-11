using Microsoft.AspNetCore.Mvc;
using Repositories;
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
        // TODO: This is where it currently breaks.
        var latest = await _measurementRepo.GetMostRecent();
        

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
       // var measurements = _measurementRepo.GetMany()
       //     .Where(m => m.TimestampUtc >= from && m.TimestampUtc <= to)
       //     .ToList();

       // return Ok(measurements);
       throw new NotImplementedException();
    }
}