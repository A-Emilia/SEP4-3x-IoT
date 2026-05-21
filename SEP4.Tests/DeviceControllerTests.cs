using Controllers;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RepositoryContracts;

namespace SEP4.Tests;

public class DeviceControllerTests
{
    [Fact]
    public async Task CreateDevice_WithInvalidState_ShouldReturnBadRequest()
    {
        // Arrange
        var mockDeviceRepo = new Mock<IDeviceRepository>();
        var mockActionLogRepo = new Mock<IDeviceActionLogRepository>();

        var controller = new DeviceController(
            mockDeviceRepo.Object,
            mockActionLogRepo.Object
        );

        var device = new Device
        {
            Id = "device1",
            RoomId = "room1",
            Type = DeviceType.Heater,
            State = DeviceState.Open
        };

        // Act
        var result = await controller.CreateDevice(device);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SendDeviceAction_WhenStateAlreadySet_ShouldReturnOk()
    {
        // Arrange
        var mockDeviceRepo = new Mock<IDeviceRepository>();
        var mockActionLogRepo = new Mock<IDeviceActionLogRepository>();

        mockDeviceRepo.Setup(r =>
                r.GetDeviceState("room1", DeviceType.Heater))
            .ReturnsAsync(DeviceState.On);

        var controller = new DeviceController(
            mockDeviceRepo.Object,
            mockActionLogRepo.Object
        );

        var request = new DeviceActionRequest
        {
            RoomId = "room1",
            Device = DeviceType.Heater,
            State = DeviceState.On
        };

        // Act
        var result = await controller.SendDeviceAction(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task SendDeviceAction_WithInvalidState_ShouldReturnBadRequest()
    {
        // Arrange
        var mockDeviceRepo = new Mock<IDeviceRepository>();
        var mockActionLogRepo = new Mock<IDeviceActionLogRepository>();

        var controller = new DeviceController(
            mockDeviceRepo.Object,
            mockActionLogRepo.Object
        );

        var request = new DeviceActionRequest
        {
            RoomId = "room1",
            Device = DeviceType.Window,
            State = DeviceState.On
        };

        // Act
        var result = await controller.SendDeviceAction(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetDeviceById_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        var mockDeviceRepo = new Mock<IDeviceRepository>();
        var mockActionLogRepo = new Mock<IDeviceActionLogRepository>();

        mockDeviceRepo.Setup(r => r.GetDevice("unknownDevice"))
            .ThrowsAsync(new KeyNotFoundException("Device not found."));

        var controller = new DeviceController(
            mockDeviceRepo.Object,
            mockActionLogRepo.Object
        );

        // Act
        var result = await controller.GetDeviceById("unknownDevice");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreateDevice_WithMissingRoomId_ShouldReturnBadRequest()
    {
        // Arrange
        var mockDeviceRepo = new Mock<IDeviceRepository>();
        var mockActionLogRepo = new Mock<IDeviceActionLogRepository>();

        var controller = new DeviceController(
            mockDeviceRepo.Object,
            mockActionLogRepo.Object
        );

        var device = new Device
        {
            Id = "device1",
            RoomId = "",
            Type = DeviceType.Heater,
            State = DeviceState.On
        };

        // Act
        var result = await controller.CreateDevice(device);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SendDeviceAction_SuccessfullyUpdatesState_ShouldReturnOk()
    {
        // Arrange
        var mockDeviceRepo = new Mock<IDeviceRepository>();
        var mockActionLogRepo = new Mock<IDeviceActionLogRepository>();

        mockDeviceRepo.Setup(r =>
                r.GetDeviceState("room1", DeviceType.Heater))
            .ReturnsAsync(DeviceState.Off);

        mockDeviceRepo.Setup(r =>
                r.SetState("room1", DeviceType.Heater, DeviceState.On))
            .Returns(Task.CompletedTask);

        mockActionLogRepo.Setup(r =>
                r.CreateAsync(It.IsAny<DeviceActionLog>()))
            .ReturnsAsync(new DeviceActionLog
            {
                RoomId = "room1",
                DeviceType = DeviceType.Heater,
                PreviousState = DeviceState.Off,
                NewState = DeviceState.On
            });

        var controller = new DeviceController(
            mockDeviceRepo.Object,
            mockActionLogRepo.Object
        );

        var request = new DeviceActionRequest
        {
            RoomId = "room1",
            Device = DeviceType.Heater,
            State = DeviceState.On
        };

        // Act
        var result = await controller.SendDeviceAction(request);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task SendDeviceAction_WithUnknownRoom_ShouldReturnNotFound()
    {
        // Arrange
        var mockDeviceRepo = new Mock<IDeviceRepository>();
        var mockActionLogRepo = new Mock<IDeviceActionLogRepository>();

        mockDeviceRepo.Setup(r =>
                r.GetDeviceState("unknownRoom", DeviceType.Heater))
            .ThrowsAsync(new KeyNotFoundException("Room not found."));

        var controller = new DeviceController(
            mockDeviceRepo.Object,
            mockActionLogRepo.Object
        );

        var request = new DeviceActionRequest
        {
            RoomId = "unknownRoom",
            Device = DeviceType.Heater,
            State = DeviceState.On
        };

        // Act
        var result = await controller.SendDeviceAction(request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task CreateDevice_WhenRepositoryThrows_ShouldReturnBadRequest()
    {
        // Arrange
        var mockDeviceRepo = new Mock<IDeviceRepository>();
        var mockActionLogRepo = new Mock<IDeviceActionLogRepository>();

        mockDeviceRepo.Setup(r =>
                r.CreateAsync(It.IsAny<Device>()))
            .ThrowsAsync(new Exception("Database error."));

        var controller = new DeviceController(
            mockDeviceRepo.Object,
            mockActionLogRepo.Object
        );

        var device = new Device
        {
            Id = "device1",
            RoomId = "room1",
            Type = DeviceType.Heater,
            State = DeviceState.On
        };

        // Act
        var result = await controller.CreateDevice(device);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateDevice_WithMissingDeviceId_ShouldReturnBadRequest()
    {
        // Arrange
        var mockDeviceRepo = new Mock<IDeviceRepository>();
        var mockActionLogRepo = new Mock<IDeviceActionLogRepository>();

        var controller = new DeviceController(
            mockDeviceRepo.Object,
            mockActionLogRepo.Object
        );

        var device = new Device
        {
            Id = "",
            RoomId = "room1",
            Type = DeviceType.Heater,
            State = DeviceState.On
        };

        // Act
        var result = await controller.CreateDevice(device);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SendDeviceAction_WithMissingRoomId_ShouldReturnBadRequest()
    {
        // Arrange
        var mockDeviceRepo = new Mock<IDeviceRepository>();
        var mockActionLogRepo = new Mock<IDeviceActionLogRepository>();

        var controller = new DeviceController(
            mockDeviceRepo.Object,
            mockActionLogRepo.Object
        );

        var request = new DeviceActionRequest
        {
            RoomId = "",
            Device = DeviceType.Heater,
            State = DeviceState.On
        };

        // Act
        var result = await controller.SendDeviceAction(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SendDeviceAction_WhenInvalidOperationOccurs_ShouldReturnBadRequest()
    {
        // Arrange
        var mockDeviceRepo = new Mock<IDeviceRepository>();
        var mockActionLogRepo = new Mock<IDeviceActionLogRepository>();

        mockDeviceRepo.Setup(r =>
                r.GetDeviceState("room1", DeviceType.Heater))
            .ReturnsAsync(DeviceState.Off);

        mockDeviceRepo.Setup(r =>
                r.SetState("room1", DeviceType.Heater, DeviceState.On))
            .ThrowsAsync(new InvalidOperationException("Invalid operation."));

        var controller = new DeviceController(
            mockDeviceRepo.Object,
            mockActionLogRepo.Object
        );

        var request = new DeviceActionRequest
        {
            RoomId = "room1",
            Device = DeviceType.Heater,
            State = DeviceState.On
        };

        // Act
        var result = await controller.SendDeviceAction(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}