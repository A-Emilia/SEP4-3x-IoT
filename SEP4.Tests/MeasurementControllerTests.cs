using Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RepositoryContracts;
using Entities;

namespace SEP4.Tests;

public class MeasurementControllerTests
{
    [Fact]
    public async Task GetHistory_WithInvalidDateRange_ShouldReturnBadRequest()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        var controller = new MeasurementController(
            mockMeasurementRepo.Object
        );

        var from = DateTime.Now;
        var to = DateTime.Now.AddDays(-1);

        // Act
        var result = await controller.GetHistoryBasedOnTimestamp(from, to);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetHistory_WithNoMeasurements_ShouldReturnNotFound()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        mockMeasurementRepo.Setup(r =>
                r.GetMany(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Measurement>());

        var controller = new MeasurementController(
            mockMeasurementRepo.Object
        );

        var from = DateTime.Now.AddDays(-1);
        var to = DateTime.Now;

        // Act
        var result = await controller.GetHistoryBasedOnTimestamp(from, to);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetHistory_WithValidRange_ShouldReturnOk()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        var measurements = new List<Measurement>
    {
        new Measurement(),
        new Measurement()
    };

        mockMeasurementRepo.Setup(r =>
                r.GetMany(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(measurements);

        var controller = new MeasurementController(
            mockMeasurementRepo.Object
        );

        var from = DateTime.Now.AddDays(-1);
        var to = DateTime.Now;

        // Act
        var result = await controller.GetHistoryBasedOnTimestamp(from, to);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetCurrent_WhenNoMeasurementsExist_ShouldReturnNotFound()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        mockMeasurementRepo.Setup(r => r.GetMostRecent())
            .ReturnsAsync((Measurement?)null);

        var controller = new MeasurementController(
            mockMeasurementRepo.Object
        );

        // Act
        var result = await controller.GetCurrentAsync();

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetCurrent_WithMeasurements_ShouldReturnOk()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        mockMeasurementRepo.Setup(r => r.GetMostRecent())
            .ReturnsAsync((Measurement?)new Measurement());

        var controller = new MeasurementController(
            mockMeasurementRepo.Object
        );

        // Act
        var result = await controller.GetCurrentAsync();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetHistory_WithMissingFromDate_ShouldReturnBadRequest()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        var controller = new MeasurementController(
            mockMeasurementRepo.Object
        );

        var from = default(DateTime);
        var to = DateTime.Now;

        // Act
        var result = await controller.GetHistoryBasedOnTimestamp(from, to);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetHistory_WithMissingToDate_ShouldReturnBadRequest()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        var controller = new MeasurementController(
            mockMeasurementRepo.Object
        );

        var from = DateTime.Now;
        var to = default(DateTime);

        // Act
        var result = await controller.GetHistoryBasedOnTimestamp(from, to);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}