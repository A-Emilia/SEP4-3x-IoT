using System;
using Entities;
using RepositoryContracts;

namespace Repositories.PostgreSQL;

public class DeviceRepository : IDeviceRepository {

    private readonly string _connectionString;

    public DeviceRepository(string connectionstring) {
        _connectionString = connectionstring;
    }
    public Task<Device> CreateAsync(Device device) {
        throw new NotImplementedException();
    }

    public Task<Dictionary<DeviceType, DeviceState>> GetAllDevices() {
        throw new NotImplementedException();
    }

    public Task<Device> GetDevice(int id) {
        throw new NotImplementedException();
    }

    public Task<DeviceState> GetDeviceState(DeviceType device) {
        throw new NotImplementedException();
    }

    public Task SetState(DeviceType device, DeviceState state) {
        throw new NotImplementedException();
    }
}
