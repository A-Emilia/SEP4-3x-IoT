namespace Entities;

public class DeviceActionLog
{
    public int Id { get; set; }

    public string RoomId { get; set; } = "";

    public DeviceType DeviceType { get; set; }

    public DeviceState? PreviousState { get; set; }

    public DeviceState NewState { get; set; }

    public DateTime TimestampUtc { get; set; }
}