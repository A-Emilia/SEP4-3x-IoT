using Entities;
using Microsoft.AspNetCore.Mvc;
using Repositories;

namespace Controllers;

[ApiController]
[Route("devices")]
public class DeviceController : ControllerBase
{
    private readonly DeviceStateRepo _deviceStateRepo;

    public DeviceController(DeviceStateRepo deviceStateRepo)
    {
        _deviceStateRepo = deviceStateRepo;
    }

    // POST /devices/action
    [HttpPost("action")]
    public IActionResult SendDeviceAction([FromBody] DeviceActionRequest request)
    {
        if (!IsValidStateForDevice(request.Device, request.State))
        {
            return BadRequest($"{request.State} is not a valid state for {request.Device}.");
        }

        _deviceStateRepo.SetState(request.Device, request.State);

        return Ok(new
        {
            message = "Device state updated.",
            device = request.Device,
            state = request.State
        });
    }

    private bool IsValidStateForDevice(DeviceType device, DeviceState state)
    {
        return device switch
        {
            DeviceType.Heater => state is DeviceState.On or DeviceState.Off,
            DeviceType.Window => state is DeviceState.Open or DeviceState.Closed,
            DeviceType.Curtain => state is DeviceState.Open or DeviceState.Closed,
            _ => false
        };
    }
}