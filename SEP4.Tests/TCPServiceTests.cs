using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Entities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RepositoryContracts;
using WebApi.TCP;

namespace SEP4.Tests;

public class TCPServiceTests
{
    [Fact]
    public async Task HandleClientAsync_WithInvalidJson_ShouldNotSaveMeasurement()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        var mockDeviceRepo = new Mock<IDeviceRepository>();

        var services = new ServiceCollection();

        services.AddSingleton(mockMeasurementRepo.Object);
        services.AddSingleton(mockDeviceRepo.Object);

        var provider = services.BuildServiceProvider();

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        var tcpService = new TCPService(scopeFactory);

        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", port);

        var serverClient = await listener.AcceptTcpClientAsync();

        var stream = client.GetStream();

        var invalidJson = "{ invalid json }\n";

        var bytes = Encoding.UTF8.GetBytes(invalidJson);

        await stream.WriteAsync(bytes);

        var method = typeof(TCPService)
            .GetMethod(
                "HandleClientAsync",
                BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.NotNull(method);

        _ = (Task)method.Invoke(
            tcpService,
            new object[]
            {
                serverClient,
                CancellationToken.None
            })!;

        await Task.Delay(200);

        // Assert
        mockMeasurementRepo.Verify(r =>
            r.CreateAsync(It.IsAny<Measurement>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleClientAsync_ShouldAssignSharedRoomId()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        Measurement? savedMeasurement = null;

        mockMeasurementRepo.Setup(r =>
                r.CreateAsync(It.IsAny<Measurement>()))
            .Callback<Measurement>(m => savedMeasurement = m)
            .ReturnsAsync((Measurement m) => m);

        var mockDeviceRepo = new Mock<IDeviceRepository>();

        mockDeviceRepo.Setup(r =>
                r.GetDeviceState(
                    It.IsAny<string>(),
                    It.IsAny<DeviceType>()))
            .ReturnsAsync(DeviceState.Off);

        var services = new ServiceCollection();

        services.AddSingleton(mockMeasurementRepo.Object);
        services.AddSingleton(mockDeviceRepo.Object);

        var provider = services.BuildServiceProvider();

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        var tcpService = new TCPService(scopeFactory);

        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", port);

        var serverClient = await listener.AcceptTcpClientAsync();

        var json = """
        {"temperature":20,"humidity":50,"light":100}
        """ + "\n";

        var bytes = Encoding.UTF8.GetBytes(json);

        await client.GetStream().WriteAsync(bytes);

        var method = typeof(TCPService)
            .GetMethod(
                "HandleClientAsync",
                BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.NotNull(method);

        _ = (Task)method.Invoke(
            tcpService,
            new object[]
            {
                serverClient,
                CancellationToken.None
            })!;

        await Task.Delay(200);

        // Assert
        Assert.NotNull(savedMeasurement);
        Assert.Equal("shared", savedMeasurement!.RoomId);
    }

    [Fact]
    public async Task HandleClientAsync_WithHeaterOn_ShouldIncreaseTemperature()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        Measurement? savedMeasurement = null;

        mockMeasurementRepo.Setup(r =>
                r.CreateAsync(It.IsAny<Measurement>()))
            .Callback<Measurement>(m => savedMeasurement = m)
            .ReturnsAsync((Measurement m) => m);

        var mockDeviceRepo = new Mock<IDeviceRepository>();

        mockDeviceRepo.Setup(r =>
                r.GetDeviceState(
                    It.IsAny<string>(),
                    DeviceType.Heater))
            .ReturnsAsync(DeviceState.On);

        mockDeviceRepo.Setup(r =>
                r.GetDeviceState(
                    It.IsAny<string>(),
                    It.Is<DeviceType>(d => d != DeviceType.Heater)))
            .ReturnsAsync(DeviceState.Off);

        var services = new ServiceCollection();

        services.AddSingleton(mockMeasurementRepo.Object);
        services.AddSingleton(mockDeviceRepo.Object);

        var provider = services.BuildServiceProvider();

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        var tcpService = new TCPService(scopeFactory);

        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", port);

        var serverClient = await listener.AcceptTcpClientAsync();

        var json = """
        {"temperature":20,"humidity":50,"light":100}
        """ + "\n";

        var bytes = Encoding.UTF8.GetBytes(json);

        await client.GetStream().WriteAsync(bytes);

        var method = typeof(TCPService)
            .GetMethod(
                "HandleClientAsync",
                BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.NotNull(method);

        _ = (Task)method.Invoke(
            tcpService,
            new object[]
            {
                serverClient,
                CancellationToken.None
            })!;

        await Task.Delay(200);

        // Assert
        Assert.NotNull(savedMeasurement);
        Assert.Equal(22, savedMeasurement!.Temperature);
    }

    [Fact]
    public async Task HandleClientAsync_ShouldAssignTimestamp()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        Measurement? savedMeasurement = null;

        mockMeasurementRepo.Setup(r =>
                r.CreateAsync(It.IsAny<Measurement>()))
            .Callback<Measurement>(m => savedMeasurement = m)
            .ReturnsAsync((Measurement m) => m);

        var mockDeviceRepo = new Mock<IDeviceRepository>();

        mockDeviceRepo.Setup(r =>
                r.GetDeviceState(
                    It.IsAny<string>(),
                    It.IsAny<DeviceType>()))
            .ReturnsAsync(DeviceState.Off);

        var services = new ServiceCollection();

        services.AddSingleton(mockMeasurementRepo.Object);
        services.AddSingleton(mockDeviceRepo.Object);

        var provider = services.BuildServiceProvider();

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        var tcpService = new TCPService(scopeFactory);

        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", port);

        var serverClient = await listener.AcceptTcpClientAsync();

        var json = """
        {"temperature":20,"humidity":50,"light":100}
        """ + "\n";

        var bytes = Encoding.UTF8.GetBytes(json);

        await client.GetStream().WriteAsync(bytes);

        var method = typeof(TCPService)
            .GetMethod(
                "HandleClientAsync",
                BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.NotNull(method);

        _ = (Task)method.Invoke(
            tcpService,
            new object[]
            {
                serverClient,
                CancellationToken.None
            })!;

        await Task.Delay(200);

        // Assert
        Assert.NotNull(savedMeasurement);
        Assert.True(savedMeasurement!.TimestampUtc > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task HandleClientAsync_WithWindowOpen_ShouldDecreaseTemperature()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        Measurement? savedMeasurement = null;

        mockMeasurementRepo.Setup(r =>
                r.CreateAsync(It.IsAny<Measurement>()))
            .Callback<Measurement>(m => savedMeasurement = m)
            .ReturnsAsync((Measurement m) => m);

        var mockDeviceRepo = new Mock<IDeviceRepository>();

        mockDeviceRepo.Setup(r =>
                r.GetDeviceState(
                    It.IsAny<string>(),
                    DeviceType.Window))
            .ReturnsAsync(DeviceState.Open);

        mockDeviceRepo.Setup(r =>
                r.GetDeviceState(
                    It.IsAny<string>(),
                    It.Is<DeviceType>(d => d != DeviceType.Window)))
            .ReturnsAsync(DeviceState.Off);

        var services = new ServiceCollection();

        services.AddSingleton(mockMeasurementRepo.Object);
        services.AddSingleton(mockDeviceRepo.Object);

        var provider = services.BuildServiceProvider();

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        var tcpService = new TCPService(scopeFactory);

        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var client = new TcpClient();

        await client.ConnectAsync("127.0.0.1", port);

        var serverClient = await listener.AcceptTcpClientAsync();

        var json = """
    {"temperature":20,"humidity":50,"light":100}
    """ + "\n";

        var bytes = Encoding.UTF8.GetBytes(json);

        await client.GetStream().WriteAsync(bytes);

        var method = typeof(TCPService)
            .GetMethod(
                "HandleClientAsync",
                BindingFlags.NonPublic | BindingFlags.Instance);

        _ = (Task)method!.Invoke(
            tcpService,
            new object[]
            {
            serverClient,
            CancellationToken.None
            })!;

        await Task.Delay(200);

        // Assert
        Assert.NotNull(savedMeasurement);
        Assert.Equal(18, savedMeasurement!.Temperature);
    }

    [Fact]
    public async Task HandleClientAsync_WithCurtainClosed_ShouldReduceLight()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        Measurement? savedMeasurement = null;

        mockMeasurementRepo.Setup(r =>
                r.CreateAsync(It.IsAny<Measurement>()))
            .Callback<Measurement>(m => savedMeasurement = m)
            .ReturnsAsync((Measurement m) => m);

        var mockDeviceRepo = new Mock<IDeviceRepository>();

        mockDeviceRepo.Setup(r =>
                r.GetDeviceState(
                    It.IsAny<string>(),
                    DeviceType.Curtain))
            .ReturnsAsync(DeviceState.Closed);

        mockDeviceRepo.Setup(r =>
                r.GetDeviceState(
                    It.IsAny<string>(),
                    It.Is<DeviceType>(d => d != DeviceType.Curtain)))
            .ReturnsAsync(DeviceState.Off);

        var services = new ServiceCollection();

        services.AddSingleton(mockMeasurementRepo.Object);
        services.AddSingleton(mockDeviceRepo.Object);

        var provider = services.BuildServiceProvider();

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        var tcpService = new TCPService(scopeFactory);

        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var client = new TcpClient();

        await client.ConnectAsync("127.0.0.1", port);

        var serverClient = await listener.AcceptTcpClientAsync();

        var json = """
    {"temperature":20,"humidity":50,"light":100}
    """ + "\n";

        var bytes = Encoding.UTF8.GetBytes(json);

        await client.GetStream().WriteAsync(bytes);

        var method = typeof(TCPService)
            .GetMethod(
                "HandleClientAsync",
                BindingFlags.NonPublic | BindingFlags.Instance);

        _ = (Task)method!.Invoke(
            tcpService,
            new object[]
            {
            serverClient,
            CancellationToken.None
            })!;

        await Task.Delay(200);

        // Assert
        Assert.NotNull(savedMeasurement);
        Assert.Equal(30, savedMeasurement!.Light);
    }

    [Fact]
    public async Task HandleClientAsync_WhenSaveFails_ShouldNotCrash()
    {
        // Arrange
        var mockMeasurementRepo = new Mock<IMeasurementRepository>();

        mockMeasurementRepo.Setup(r =>
                r.CreateAsync(It.IsAny<Measurement>()))
            .ThrowsAsync(new Exception("DB fail"));

        var mockDeviceRepo = new Mock<IDeviceRepository>();

        mockDeviceRepo.Setup(r =>
                r.GetDeviceState(
                    It.IsAny<string>(),
                    It.IsAny<DeviceType>()))
            .ReturnsAsync(DeviceState.Off);

        var services = new ServiceCollection();

        services.AddSingleton(mockMeasurementRepo.Object);
        services.AddSingleton(mockDeviceRepo.Object);

        var provider = services.BuildServiceProvider();

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        var tcpService = new TCPService(scopeFactory);

        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var client = new TcpClient();

        await client.ConnectAsync("127.0.0.1", port);

        var serverClient = await listener.AcceptTcpClientAsync();

        var json = """
    {"temperature":20,"humidity":50,"light":100}
    """ + "\n";

        var bytes = Encoding.UTF8.GetBytes(json);

        await client.GetStream().WriteAsync(bytes);

        var method = typeof(TCPService)
            .GetMethod(
                "HandleClientAsync",
                BindingFlags.NonPublic | BindingFlags.Instance);

        var exception = await Record.ExceptionAsync(async () =>
        {
            _ = (Task)method!.Invoke(
                tcpService,
                new object[]
                {
                serverClient,
                CancellationToken.None
                })!;

            await Task.Delay(200);
        });

        // Assert
        Assert.Null(exception);
    }
}