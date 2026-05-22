using Entities;
using MongoDB.Driver;
using RepositoryContracts;

namespace Repositories;

public class MeasurementRepository : IMeasurementRepository
{
    private readonly IMongoCollection<Measurement> _measurements;

    public MeasurementRepository(IMongoDatabase database)
    {
        _measurements = database.GetCollection<Measurement>("measurements");
    }

    public async Task<Measurement> CreateAsync(Measurement measurement)
    {
        await _measurements.InsertOneAsync(measurement);
        return measurement;
    }

    public async Task<Measurement?> GetMostRecent(string roomId)
    {
        return await _measurements
            .Find(x => x.RoomId == roomId)
            .SortByDescending(x => x.TimestampUtc)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Measurement>> GetMany(string roomId, DateTime start, DateTime end)
    {
        return await _measurements
            .Find(x =>
                x.RoomId == roomId &&
                x.TimestampUtc >= start &&
                x.TimestampUtc <= end)
            .SortByDescending(x => x.TimestampUtc)
            .ToListAsync();
    }
}