using Entities;

namespace RepositoryContracts;

public interface IDeviceActionLogRepository
{
    Task<DeviceActionLog> CreateAsync(DeviceActionLog log);

    Task<List<DeviceActionLog>> GetByRoomIdAsync(string roomId);

    Task<List<DeviceActionLog>> GetAllAsync();
}