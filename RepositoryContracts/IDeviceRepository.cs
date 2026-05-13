using System;
using Entities;

namespace RepositoryContracts;

public interface IDeviceRepository {
    Task<Device> CreateAsync(Device device);
    Task<Device> GetDevice(int id);
    Task<DeviceState> GetDeviceState(DeviceType device);
    Task SetState(DeviceType device, DeviceState state); 
    Task<Dictionary<DeviceType, DeviceState>> GetAllDevices();

}
