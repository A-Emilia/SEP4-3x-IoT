using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Entities;
using RepositoryContracts;

namespace WebApi.TCP;

public class TCPService : BackgroundService
{
    private const string SharedRoomId = "shared";

    private readonly IServiceScopeFactory _scopeFactory;

    public TCPService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();

        Console.WriteLine("TCP server listening on port 5000");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(stoppingToken);

                _ = Task.Run(() => HandleClientAsync(client, stoppingToken), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("TCP server stopped.");
        }
        finally
        {
            listener.Stop();
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken stoppingToken)
    {
        using var tcpClient = client;
        using var stream = tcpClient.GetStream();
        using var reader = new StreamReader(stream);

        while (!stoppingToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(stoppingToken);

            if (line == null)
                break;

            Console.WriteLine("TCP RX: " + line);

            try
            {
                var measurement = JsonSerializer.Deserialize<Measurement>(line, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (measurement == null)
                    continue;

                measurement.Id = null!;
                measurement.RoomId = SharedRoomId;
                measurement.TimestampUtc = DateTime.UtcNow;

                using var scope = _scopeFactory.CreateScope();

                var deviceRepository = scope.ServiceProvider
                    .GetRequiredService<IDeviceRepository>();

                await ApplyDeviceEffects(measurement, deviceRepository);

                var measurementRepository = scope.ServiceProvider
                    .GetRequiredService<IMeasurementRepository>();

                await measurementRepository.CreateAsync(measurement);

                Console.WriteLine(DateTime.UtcNow + " Measurement saved to MongoDB.");
            }
            catch (JsonException)
            {
                Console.WriteLine("Invalid JSON received.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to save measurement: " + ex.Message);
            }
        }
    }

    private static async Task ApplyDeviceEffects(
        Measurement measurement,
        IDeviceRepository deviceRepository)
    {
        var heaterState = await deviceRepository.GetDeviceState(SharedRoomId, DeviceType.Heater);
        var windowState = await deviceRepository.GetDeviceState(SharedRoomId, DeviceType.Window);
        var curtainState = await deviceRepository.GetDeviceState(SharedRoomId, DeviceType.Curtain);
        var humidifierState = await deviceRepository.GetDeviceState(SharedRoomId, DeviceType.Humidifier);
        
        if (heaterState == DeviceState.On)
        {
            measurement.Temperature *= 1.1m;
        }

        if (windowState == DeviceState.Open)
        {
            measurement.Temperature *= 0.9m;
        }

        if (curtainState == DeviceState.Closed)
        {
            measurement.Light *= 0.3;
        }
        
        if (curtainState == DeviceState.Closed)
        {
             measurement.Humidity *= 1.4m;
        }

        measurement.Temperature = Math.Round(measurement.Temperature, 2);
        measurement.Light = Math.Round(measurement.Light, 2);
    }
}