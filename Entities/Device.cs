namespace Entities;

public class Device
{
    public string Id { get; set; } = "";
    public string RoomId { get; set; } = "";
    public DeviceState State { get; set; }
    public DeviceType Type { get; set; }
}