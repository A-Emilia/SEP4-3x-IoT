using Entities;

namespace RepositoryContracts;

public interface IMeasurementRepository
{
    Task<Measurement> CreateAsync(Measurement measurement);
    Task<Measurement> GetMostRecent(string roomId);
    Task<List<Measurement>> GetMany(string roomId, DateTime start, DateTime end);
}