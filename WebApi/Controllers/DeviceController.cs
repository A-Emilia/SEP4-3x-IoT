using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace Controllers;

[ApiController]
[Route("devices")]
public class DeviceController : ControllerBase
{
    private readonly IDeviceRepository _deviceRepo;
    private readonly IDeviceActionLogRepository _actionLogRepo;

    public DeviceController(
        IDeviceRepository deviceRepository,
        IDeviceActionLogRepository actionLogRepository)
    {
        _deviceRepo = deviceRepository;
        _actionLogRepo = actionLogRepository;
    }

    // POST /devices
    [HttpPost]
    public async Task<IActionResult> CreateDevice([FromBody] Device device)
    {
        if (string.IsNullOrWhiteSpace(device.Id))
            return BadRequest("Device id is required.");

        if (string.IsNullOrWhiteSpace(device.RoomId))
            return BadRequest("Room id is required.");

        if (!IsValidStateForDevice(device.Type, device.State))
            return BadRequest($"{device.State} is not a valid state for {device.Type}.");

        try
        {
            var createdDevice = await _deviceRepo.CreateAsync(device);

            return Ok(new
            {
                message = "Device created.",
                device = createdDevice
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET /devices/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDeviceById(string id)
    {
        try
        {
            var device = await _deviceRepo.GetDevice(id);
            return Ok(device);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // GET /devices/room/{roomId}
    [HttpGet("room/{roomId}")]
    public async Task<IActionResult> GetAllDevicesForRoom(string roomId)
    {
        try
        {
            var devices = await _deviceRepo.GetAllDevices(roomId);
            return Ok(devices);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET /devices/room/{roomId}/state?device=Heater
    [HttpGet("room/{roomId}/state")]
    public async Task<IActionResult> GetDeviceState(
        string roomId,
        [FromQuery] DeviceType device)
    {
        try
        {
            var state = await _deviceRepo.GetDeviceState(roomId, device);

            return Ok(new
            {
                roomId,
                device,
                state
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // POST /devices/action
    [HttpPost("action")]
    public async Task<IActionResult> SendDeviceAction([FromBody] DeviceActionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RoomId))
            return BadRequest("Room id is required.");

        if (!IsValidStateForDevice(request.Device, request.State))
            return BadRequest($"{request.State} is not a valid state for {request.Device}.");

        try
        {
            var previousState = await _deviceRepo.GetDeviceState(request.RoomId, request.Device);

            if (previousState == request.State)
            {
                return Ok(new
                {
                    message = "Device state is already set.",
                    roomId = request.RoomId, //can adjust to MAL's needs
                    device = request.Device,
                    state = request.State
                });
            }

            await _deviceRepo.SetState(request.RoomId, request.Device, request.State);

            var log = await _actionLogRepo.CreateAsync(new DeviceActionLog
            {
                RoomId = request.RoomId,
                DeviceType = request.Device,
                PreviousState = previousState,
                NewState = request.State
            });

            return Ok(new
            {
                message = "Device state updated.",
                actionLog = log //can remove based on if MAL needs it or not
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // GET /devices/actions
    [HttpGet("actions")]
    public async Task<IActionResult> GetAllDeviceActions()
    {
        var logs = await _actionLogRepo.GetAllAsync();
        return Ok(logs);
    }

    // GET /devices/room/{roomId}/actions
    [HttpGet("room/{roomId}/actions")]
    public async Task<IActionResult> GetDeviceActionsForRoom(string roomId)
    {
        var logs = await _actionLogRepo.GetByRoomIdAsync(roomId);
        return Ok(logs);
    }

    private static bool IsValidStateForDevice(DeviceType device, DeviceState state)
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