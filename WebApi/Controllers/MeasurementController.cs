using Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;


[ApiController]
[Route("api/measurements")]

public class MeasurementController : ControllerBase
{
    private readonly JSONRepo _store;

    public MeasurementController(JSONRepo store)
    {
        _store = store;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_store.GetAll());
    }

    [HttpGet("current")]
    public IActionResult GetCurrent()
    {
        var latest = _store.GetLatest();

        if (latest == null)
            return NotFound("No measurements yet.");

        return Ok(latest);
    }   
}