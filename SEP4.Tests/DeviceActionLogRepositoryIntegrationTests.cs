using Entities;
using Moq;
using RepositoryContracts;

namespace SEP4.Tests;

public class DeviceActionLogRepositoryIntegrationTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateLog()
    {
        // Arrange
        var log = new DeviceActionLog
        {
            RoomId = "room1",
            DeviceType = DeviceType.Heater,
            PreviousState = DeviceState.Off,
            NewState = DeviceState.On
        };

        var mockLogRepo = new Mock<IDeviceActionLogRepository>();

        mockLogRepo.Setup(r =>
                r.CreateAsync(It.IsAny<DeviceActionLog>()))
            .ReturnsAsync(log);

        // Act
        var createdLog = await mockLogRepo.Object
            .CreateAsync(log);

        // Assert
        Assert.NotNull(createdLog);
        Assert.Equal(DeviceType.Heater, createdLog.DeviceType);
        Assert.Equal(DeviceState.On, createdLog.NewState);
    }

    [Fact]
    public async Task GetByRoomIdAsync_ShouldReturnLogs()
    {
        // Arrange
        var logs = new List<DeviceActionLog>
        {
            new DeviceActionLog
            {
                RoomId = "room1",
                DeviceType = DeviceType.Window,
                PreviousState = DeviceState.Closed,
                NewState = DeviceState.Open
            }
        };

        var mockLogRepo = new Mock<IDeviceActionLogRepository>();

        mockLogRepo.Setup(r =>
                r.GetByRoomIdAsync(It.IsAny<string>()))
            .ReturnsAsync(logs);

        // Act
        var result = await mockLogRepo.Object
            .GetByRoomIdAsync("room1");

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task CreateAsync_WithNullPreviousState_ShouldCreateLog()
    {
        // Arrange
        var log = new DeviceActionLog
        {
            RoomId = "room1",
            DeviceType = DeviceType.Heater,
            PreviousState = null,
            NewState = DeviceState.On
        };

        var mockLogRepo = new Mock<IDeviceActionLogRepository>();

        mockLogRepo.Setup(r =>
                r.CreateAsync(It.IsAny<DeviceActionLog>()))
            .ReturnsAsync(log);

        // Act
        var createdLog = await mockLogRepo.Object
            .CreateAsync(log);

        // Assert
        Assert.Null(createdLog.PreviousState);
        Assert.Equal(DeviceState.On, createdLog.NewState);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnLogs()
    {
        // Arrange
        var logs = new List<DeviceActionLog>
        {
            new DeviceActionLog
            {
                RoomId = "room1",
                DeviceType = DeviceType.Heater,
                PreviousState = DeviceState.Off,
                NewState = DeviceState.On
            }
        };

        var mockLogRepo = new Mock<IDeviceActionLogRepository>();

        mockLogRepo.Setup(r =>
                r.GetAllAsync())
            .ReturnsAsync(logs);

        // Act
        var result = await mockLogRepo.Object
            .GetAllAsync();

        // Assert
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetByRoomIdAsync_WithInvalidRoom_ShouldReturnEmptyList()
    {
        // Arrange
        var mockLogRepo = new Mock<IDeviceActionLogRepository>();

        mockLogRepo.Setup(r =>
                r.GetByRoomIdAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<DeviceActionLog>());

        // Act
        var result = await mockLogRepo.Object
            .GetByRoomIdAsync("invalid_room");

        // Assert
        Assert.Empty(result);
    }
}