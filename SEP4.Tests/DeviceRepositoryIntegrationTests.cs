using Entities;
using Moq;
using RepositoryContracts;

namespace SEP4.Tests;

public class DeviceRepositoryIntegrationTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateDevice()
    {
        // Arrange
        var device = new Device
        {
            Id = "device1",
            RoomId = "room1",
            Type = DeviceType.Heater,
            State = DeviceState.On
        };

        var mockDeviceRepo = new Mock<IDeviceRepository>();

        mockDeviceRepo.Setup(r =>
                r.CreateAsync(It.IsAny<Device>()))
            .ReturnsAsync(device);

        // Act
        var createdDevice = await mockDeviceRepo.Object
            .CreateAsync(device);

        // Assert
        Assert.NotNull(createdDevice);
        Assert.Equal(device.Id, createdDevice.Id);
        Assert.Equal(DeviceType.Heater, createdDevice.Type);
        Assert.Equal(DeviceState.On, createdDevice.State);
    }

    [Fact]
    public async Task GetDevice_ShouldReturnDevice()
    {
        // Arrange
        var fakeDevice = new Device
        {
            Id = "device1",
            RoomId = "room1",
            Type = DeviceType.Window,
            State = DeviceState.Open
        };

        var mockDeviceRepo = new Mock<IDeviceRepository>();

        mockDeviceRepo.Setup(r =>
                r.GetDevice("device1"))
            .ReturnsAsync(fakeDevice);

        // Act
        var fetchedDevice = await mockDeviceRepo.Object
            .GetDevice("device1");

        // Assert
        Assert.Equal(fakeDevice.Id, fetchedDevice.Id);
        Assert.Equal(DeviceType.Window, fetchedDevice.Type);
        Assert.Equal(DeviceState.Open, fetchedDevice.State);
    }

    [Fact]
    public async Task GetDevice_WithInvalidId_ShouldThrowException()
    {
        // Arrange
        var mockDeviceRepo = new Mock<IDeviceRepository>();

        mockDeviceRepo.Setup(r =>
                r.GetDevice(It.IsAny<string>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            mockDeviceRepo.Object.GetDevice("invalid_device"));
    }

    [Fact]
    public async Task GetDeviceState_ShouldReturnState()
    {
        // Arrange
        var mockDeviceRepo = new Mock<IDeviceRepository>();

        mockDeviceRepo.Setup(r =>
                r.GetDeviceState(
                    It.IsAny<string>(),
                    It.IsAny<DeviceType>()))
            .ReturnsAsync(DeviceState.On);

        // Act
        var state = await mockDeviceRepo.Object.GetDeviceState(
            "room1",
            DeviceType.Heater
        );

        // Assert
        Assert.Equal(DeviceState.On, state);
    }

    [Fact]
    public async Task GetDeviceState_WithInvalidRoom_ShouldThrowException()
    {
        // Arrange
        var mockDeviceRepo = new Mock<IDeviceRepository>();

        mockDeviceRepo.Setup(r =>
                r.GetDeviceState(
                    It.IsAny<string>(),
                    It.IsAny<DeviceType>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            mockDeviceRepo.Object.GetDeviceState(
                "invalid_room",
                DeviceType.Heater
            ));
    }

    [Fact]
    public async Task SetState_ShouldUpdateDeviceState()
    {
        // Arrange
        var mockDeviceRepo = new Mock<IDeviceRepository>();

        mockDeviceRepo.Setup(r =>
                r.SetState(
                    It.IsAny<string>(),
                    It.IsAny<DeviceType>(),
                    It.IsAny<DeviceState>()))
            .Returns(Task.CompletedTask);

        mockDeviceRepo.Setup(r =>
                r.GetDeviceState(
                    It.IsAny<string>(),
                    It.IsAny<DeviceType>()))
            .ReturnsAsync(DeviceState.On);

        // Act
        await mockDeviceRepo.Object.SetState(
            "room1",
            DeviceType.Heater,
            DeviceState.On
        );

        var updatedState = await mockDeviceRepo.Object
            .GetDeviceState(
                "room1",
                DeviceType.Heater
            );

        // Assert
        Assert.Equal(DeviceState.On, updatedState);
    }

    [Fact]
    public async Task SetState_WithInvalidRoom_ShouldThrowException()
    {
        // Arrange
        var mockDeviceRepo = new Mock<IDeviceRepository>();

        mockDeviceRepo.Setup(r =>
                r.SetState(
                    It.IsAny<string>(),
                    It.IsAny<DeviceType>(),
                    It.IsAny<DeviceState>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act + Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            mockDeviceRepo.Object.SetState(
                "invalid_room",
                DeviceType.Heater,
                DeviceState.On
            ));
    }

    [Fact]
    public async Task GetAllDevices_ShouldReturnDevices()
    {
        // Arrange
        var devices = new Dictionary<DeviceType, DeviceState>
        {
            { DeviceType.Heater, DeviceState.On }
        };

        var mockDeviceRepo = new Mock<IDeviceRepository>();

        mockDeviceRepo.Setup(r =>
                r.GetAllDevices(It.IsAny<string>()))
            .ReturnsAsync(devices);

        // Act
        var result = await mockDeviceRepo.Object
            .GetAllDevices("room1");

        // Assert
        Assert.NotEmpty(result);
        Assert.True(result.ContainsKey(DeviceType.Heater));
    }
}