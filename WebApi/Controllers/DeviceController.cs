using Entities;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("devices")]
public class DeviceController : ControllerBase
{
    // POST /devices/action
    [HttpPost("action")]
    public IActionResult SendDeviceAction([FromBody] DeviceActionRequest request)
    {
        return Ok($"Command received: {request.Device} - {request.DeviceAction}");
    }
}