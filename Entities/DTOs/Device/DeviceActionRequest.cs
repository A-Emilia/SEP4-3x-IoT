namespace Entities;

public class DeviceActionRequest
{
    public string RoomId { get; set; } = "";
    public DeviceType Device { get; set; }
    public DeviceState State { get; set; }
}