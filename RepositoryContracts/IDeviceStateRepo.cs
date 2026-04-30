using Entities;

namespace RepositoryContracts;

public interface IDeviceStateRepo
{
    Dictionary<DeviceType, DeviceState> GetAll();

    DeviceState GetState(DeviceType device);

    void SetState(DeviceType device, DeviceState state);
}