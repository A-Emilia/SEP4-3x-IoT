using System.Text.Json;
using System.Text.Json.Serialization;
using Entities;

namespace Repositories;

public class DeviceStateRepo
{
    private readonly string _filePath = "devices.json";
    private readonly object _lock = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public DeviceStateRepo()
    {
        if (!File.Exists(_filePath))
        {
            var initialStates = new Dictionary<DeviceType, DeviceState>
            {
                { DeviceType.Heater, DeviceState.Off },
                { DeviceType.Window, DeviceState.Closed },
                { DeviceType.Curtain, DeviceState.Open }
            };

            SaveAll(initialStates);
        }
    }

    public Dictionary<DeviceType, DeviceState> GetAll()
    {
        lock (_lock)
        {
            var json = File.ReadAllText(_filePath);

            return JsonSerializer.Deserialize<Dictionary<DeviceType, DeviceState>>(json, _jsonOptions)
                   ?? new Dictionary<DeviceType, DeviceState>();
        }
    }

    public DeviceState GetState(DeviceType device)
    {
        var states = GetAll();

        return states.TryGetValue(device, out var state)
            ? state
            : GetDefaultState(device);
    }

    public void SetState(DeviceType device, DeviceState state)
    {
        lock (_lock)
        {
            var states = GetAll();

            states[device] = state;

            SaveAll(states);
        }
    }

    private void SaveAll(Dictionary<DeviceType, DeviceState> states)
    {
        var json = JsonSerializer.Serialize(states, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }

    private DeviceState GetDefaultState(DeviceType device)
    {
        return device switch
        {
            DeviceType.Heater => DeviceState.Off,
            DeviceType.Window => DeviceState.Closed,
            DeviceType.Curtain => DeviceState.Open,
            _ => DeviceState.Off
        };
    }
}