using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Entities;
using Repositories;

namespace SensorBackend.Api.TCP;

public class TCPService : BackgroundService
{
    private readonly JSONRepo _store;
    private readonly DeviceStateRepo _deviceStateRepo;

    public TCPService(JSONRepo store, DeviceStateRepo deviceStateRepo)
    {
        _store = store;
        _deviceStateRepo = deviceStateRepo;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();

        Console.WriteLine("TCP server listening on port 5000");

        while (!stoppingToken.IsCancellationRequested)
        {
            var client = await listener.AcceptTcpClientAsync(stoppingToken);

            _ = Task.Run(async () =>
            {
                using var tcpClient = client;
                using var stream = tcpClient.GetStream();
                using var reader = new StreamReader(stream);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();

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

                        measurement.Id = Guid.NewGuid();
                        measurement.TimestampUtc = DateTime.UtcNow;

                        ApplyDeviceEffects(measurement);

                        _store.Add(measurement);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid JSON received.");
                    }
                }
            }, stoppingToken);
        }
    }

    private void ApplyDeviceEffects(Measurement measurement)
    {
        var heaterState = _deviceStateRepo.GetState(DeviceType.Heater);
        var windowState = _deviceStateRepo.GetState(DeviceType.Window);
        var curtainState = _deviceStateRepo.GetState(DeviceType.Curtain);

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

        measurement.Temperature = Math.Round(measurement.Temperature, 2);
        measurement.Light = Math.Round(measurement.Light, 2);
    }
}