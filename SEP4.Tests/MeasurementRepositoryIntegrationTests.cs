using Entities;
using MongoDB.Driver;
using Repositories;

namespace SEP4.Tests;

public class MeasurementRepositoryIntegrationTests
{
    private readonly string _connectionString =
        "mongodb://mongodb:mongodb@localhost:27018/measurement_data?authSource=admin";

    private readonly IMongoDatabase _database;

    public MeasurementRepositoryIntegrationTests()
    {
        var client = new MongoClient(_connectionString);
        _database = client.GetDatabase("measurement_data");
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateMeasurement()
    {
        // Arrange
        await _database.DropCollectionAsync("measurements");
        var repo = new MeasurementRepository(_database);

        var measurement = new Measurement
        {
            Temperature = 25,
            Humidity = 50,
            TimestampUtc = DateTime.UtcNow
        };

        // Act
        var createdMeasurement = await repo.CreateAsync(measurement);

        // Assert
        Assert.NotNull(createdMeasurement);
        Assert.Equal(measurement.Temperature, createdMeasurement.Temperature);
        Assert.Equal(measurement.Humidity, createdMeasurement.Humidity);
    }

    [Fact]
    public async Task GetMostRecent_ShouldReturnLatestMeasurement()
    {
        // Arrange
        await _database.DropCollectionAsync("measurements");
        var repo = new MeasurementRepository(_database);

        var oldMeasurement = new Measurement
        {
            Temperature = 20,
            Humidity = 40,
            TimestampUtc = DateTime.UtcNow.AddMinutes(-10)
        };

        var newMeasurement = new Measurement
        {
            Temperature = 30,
            Humidity = 60,
            TimestampUtc = DateTime.UtcNow
        };

        await repo.CreateAsync(oldMeasurement);
        await repo.CreateAsync(newMeasurement);

        // Act
        var result = await repo.GetMostRecent();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(30, result.Temperature);
    }

    [Fact]
    public async Task GetMany_ShouldReturnMeasurementsInRange()
    {
        // Arrange
        await _database.DropCollectionAsync("measurements");
        var repo = new MeasurementRepository(_database);

        var measurement = new Measurement
        {
            Temperature = 22,
            Humidity = 55,
            TimestampUtc = DateTime.UtcNow
        };

        await repo.CreateAsync(measurement);

        var from = DateTime.UtcNow.AddHours(-1);
        var to = DateTime.UtcNow.AddHours(1);

        // Act
        var result = await repo.GetMany(from, to);

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
public async Task GetMostRecent_WhenNoMeasurementsExist_ShouldReturnNull()
{
    // Arrange
    await _database.DropCollectionAsync("measurements");
    var repo = new MeasurementRepository(_database);

    // Act
    var result = await repo.GetMostRecent();

    // Assert
    Assert.Null(result);
}

[Fact]
public async Task GetMany_WithNoMatches_ShouldReturnEmptyList()
{
    // Arrange
    await _database.DropCollectionAsync("measurements");
    var repo = new MeasurementRepository(_database);

    var from = DateTime.UtcNow.AddYears(-10);
    var to = DateTime.UtcNow.AddYears(-9);

    // Act
    var result = await repo.GetMany(from, to);

    // Assert
    Assert.Empty(result);
}
}