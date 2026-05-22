using Entities;

namespace RepositoryContracts;

public interface IDeviceActionLogRepository
{
    Task<DeviceActionLog> CreateAsync(DeviceActionLog log);

    Task<List<DeviceActionLog>> GetAllAsync();

    Task<List<DeviceActionLog>> GetByRoomIdAsync(string roomId);

    Task<List<DeviceActionLog>> GetByTimestampAsync(DateTime from, DateTime to);

    Task<List<DeviceActionLog>> GetByRoomIdAndTimestampAsync(string roomId, DateTime from, DateTime to);
}