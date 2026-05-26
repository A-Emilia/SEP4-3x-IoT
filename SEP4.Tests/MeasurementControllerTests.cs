using Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RepositoryContracts;
using Entities;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SEP4.Tests;

public class MeasurementControllerTests
{
    private static void SetupUser(ControllerBase controller)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "test-user-id")
                }, "mock"))
            }
        };
    }

    [Fact]
    public async Task GetHistory_WithInvalidDateRange_ShouldReturnBadRequest()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        var mockRoomRepo = new Mock<IRoomRepository>();

        var controller = new MeasurementController(
            mockMeasurementRepo.Object,
            mockRoomRepo.Object
        );

        SetupUser(controller);

        var from = DateTime.Now;
        var to = DateTime.Now.AddDays(-1);

        // Act
        var result = await controller.GetHistoryBasedOnTimestamp(
            "room1",
            from,
            to
        );

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetHistory_WithNoMeasurements_ShouldReturnOk()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        mockMeasurementRepo.Setup(r =>
                r.GetMany(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()
                ))
            .ReturnsAsync(new List<Measurement>());

        var mockRoomRepo = new Mock<IRoomRepository>();

        var controller = new MeasurementController(
            mockMeasurementRepo.Object,
            mockRoomRepo.Object
        );

        SetupUser(controller);

        var from = DateTime.Now.AddDays(-1);
        var to = DateTime.Now;

        // Act
        var result = await controller.GetHistoryBasedOnTimestamp(
            "room1",
            from,
            to
        );

        // Assert
        Assert.IsType<OkObjectResult>(result);
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
                r.GetMany(
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()
                ))
            .ReturnsAsync(measurements);

        var mockRoomRepo = new Mock<IRoomRepository>();

        var controller = new MeasurementController(
            mockMeasurementRepo.Object,
            mockRoomRepo.Object
        );

        SetupUser(controller);

        var from = DateTime.Now.AddDays(-1);
        var to = DateTime.Now;

        // Act
        var result = await controller.GetHistoryBasedOnTimestamp(
            "room1",
            from,
            to
        );

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetCurrent_WhenNoMeasurementsExist_ShouldReturnNotFound()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        mockMeasurementRepo.Setup(r =>
                r.GetMostRecent(It.IsAny<string>()))
            .ReturnsAsync((Measurement?)null);

        var mockRoomRepo = new Mock<IRoomRepository>();

        var controller = new MeasurementController(
            mockMeasurementRepo.Object,
            mockRoomRepo.Object
        );

        SetupUser(controller);

        // Act
        var result = await controller.GetCurrentAsync("room1");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetCurrent_WithMeasurements_ShouldReturnOk()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        mockMeasurementRepo.Setup(r =>
                r.GetMostRecent(It.IsAny<string>()))
            .ReturnsAsync(new Measurement
            {
                Temperature = 20,
                Humidity = 50,
                Light = 100
            });

        var mockRoomRepo = new Mock<IRoomRepository>();

        var controller = new MeasurementController(
            mockMeasurementRepo.Object,
            mockRoomRepo.Object
        );

        SetupUser(controller);

        // Act
        var result = await controller.GetCurrentAsync("room1");

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetHistory_WithMissingFromDate_ShouldReturnBadRequest()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        var mockRoomRepo = new Mock<IRoomRepository>();

        var controller = new MeasurementController(
            mockMeasurementRepo.Object,
            mockRoomRepo.Object
        );

        SetupUser(controller);

        var from = default(DateTime);
        var to = DateTime.Now;

        // Act
        var result = await controller.GetHistoryBasedOnTimestamp(
            "room1",
            from,
            to
        );

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetHistory_WithMissingToDate_ShouldReturnBadRequest()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        var mockRoomRepo = new Mock<IRoomRepository>();

        var controller = new MeasurementController(
            mockMeasurementRepo.Object,
            mockRoomRepo.Object
        );

        SetupUser(controller);

        var from = DateTime.Now;
        var to = default(DateTime);

        // Act
        var result = await controller.GetHistoryBasedOnTimestamp(
            "room1",
            from,
            to
        );

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}