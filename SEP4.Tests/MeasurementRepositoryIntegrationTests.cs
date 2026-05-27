using Entities;
using Moq;
using RepositoryContracts;

namespace SEP4.Tests;

public class MeasurementRepositoryIntegrationTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateMeasurement()
    {
        // Arrange
        var measurement = new Measurement
        {
            RoomId = "room1",
            Temperature = 25,
            Humidity = 50,
            TimestampUtc = DateTime.UtcNow
        };

        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        mockMeasurementRepo.Setup(r =>
                r.CreateAsync(It.IsAny<Measurement>()))
            .ReturnsAsync(measurement);

        // Act
        var createdMeasurement = await mockMeasurementRepo.Object
            .CreateAsync(measurement);

        // Assert
        Assert.NotNull(createdMeasurement);
        Assert.Equal(measurement.Temperature, createdMeasurement.Temperature);
        Assert.Equal(measurement.Humidity, createdMeasurement.Humidity);
        Assert.Equal("room1", createdMeasurement.RoomId);
    }

    [Fact]
    public async Task GetMostRecent_ShouldReturnLatestMeasurement()
    {
        // Arrange
        var latestMeasurement = new Measurement
        {
            RoomId = "room1",
            Temperature = 30,
            Humidity = 60,
            TimestampUtc = DateTime.UtcNow
        };

        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        mockMeasurementRepo.Setup(r =>
                r.GetMostRecent("room1"))
            .ReturnsAsync(latestMeasurement);

        // Act
        var result = await mockMeasurementRepo.Object
            .GetMostRecent("room1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(30, result!.Temperature);
    }

    [Fact]
    public async Task GetMany_ShouldReturnMeasurementsInRange()
    {
        // Arrange
        var measurements = new List<Measurement>
        {
            new Measurement
            {
                RoomId = "room1",
                Temperature = 22,
                Humidity = 55,
                TimestampUtc = DateTime.UtcNow
            }
        };

        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        mockMeasurementRepo.Setup(r =>
                r.GetMany(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
            .ReturnsAsync(measurements);

        var from = DateTime.UtcNow.AddHours(-1);
        var to = DateTime.UtcNow.AddHours(1);

        // Act
        var result = await mockMeasurementRepo.Object
            .GetMany("room1", from, to);

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetMostRecent_WhenNoMeasurementsExist_ShouldReturnNull()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        mockMeasurementRepo.Setup(r =>
                r.GetMostRecent(It.IsAny<string>()))
            .ReturnsAsync((Measurement?)null);

        // Act
        var result = await mockMeasurementRepo.Object
            .GetMostRecent("room1");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMany_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        mockMeasurementRepo.Setup(r =>
                r.GetMany(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Measurement>());

        var from = DateTime.UtcNow.AddYears(-10);
        var to = DateTime.UtcNow.AddYears(-9);

        // Act
        var result = await mockMeasurementRepo.Object
            .GetMany("room1", from, to);

        // Assert
        Assert.Empty(result);
    }
}