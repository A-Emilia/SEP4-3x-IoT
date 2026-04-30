using Entities;
using System.Text.Json;

namespace Repositories;

public class JSONRepo //MeasurementsRepo
{
    private readonly string _filePath = "measurements.json";
    private readonly object _lock = new();

    public JSONRepo()
    {
        if (!File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, "[]");
        }
    }

    public List<Measurement> GetAll()
    {
        lock (_lock)
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Measurement>>(json) ?? new List<Measurement>();
        }
    }

    public Measurement? GetLatest()
    {
        return GetAll()
            .OrderByDescending(x => x.TimestampUtc)
            .FirstOrDefault();
    }

    public void Add(Measurement measurement)
    {
        lock (_lock)
        {
            var json = File.ReadAllText(_filePath);
            var list = JsonSerializer.Deserialize<List<Measurement>>(json) ?? new List<Measurement>();

            list.Add(measurement);

            var newJson = JsonSerializer.Serialize(list, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_filePath, newJson);
        }
    }
}