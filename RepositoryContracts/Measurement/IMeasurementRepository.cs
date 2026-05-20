using Entities;

namespace RepositoryContracts;

public interface IMeasurementRepository
{
    Task<Measurement> CreateAsync(Measurement measurement);
    Task<Measurement> GetMostRecent();
    Task<List<Measurement>> GetMany(DateTime start, DateTime end);
}