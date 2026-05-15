using Entities;
using MongoDB.Driver;
using RepositoryContracts;

namespace Repositories;

public class MeasurementRepository : IMeasurementRepository {

    private readonly IMongoCollection<Measurement> _measurements;

    public MeasurementRepository(IMongoDatabase database) {
        _measurements = database.GetCollection<Measurement>("measurements");
    }

    public async Task<Measurement> CreateAsync(Measurement measurement) {
        await _measurements.InsertOneAsync(measurement);
        return measurement;
    }

    public async Task<List<Measurement>> GetMany(DateTime start, DateTime end) {
        return await _measurements
            .Find(x => x.TimestampUtc >= start && x.TimestampUtc <= end)
            .ToListAsync();
    }

    public async Task<Measurement> GetMostRecent() {
        return await _measurements
        // MongoDB's C# driver just always requires a filter, hence why the Find() is needed.
            .Find(_ => true)
            .SortByDescending(x => x.TimestampUtc)
            .FirstOrDefaultAsync();
    }
}