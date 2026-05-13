using Entities;

public interface IDeviceRepository
{
    Task<Device> CreateAsync(Device device);
    Task<Device> GetDevice(string id);
    Task<DeviceState> GetDeviceState(string roomId, DeviceType device);
    Task SetState(string roomId, DeviceType device, DeviceState state);
    Task<Dictionary<DeviceType, DeviceState>> GetAllDevices(string roomId);
}