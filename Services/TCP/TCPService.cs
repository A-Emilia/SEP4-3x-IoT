using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Entities;
using Repositories;

namespace SensorBackend.Api.TCP;

public class TCPService : BackgroundService
{
    private readonly JSONRepo _store;

    public TCPService(JSONRepo store)
    {
        _store = store;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var listener = new TcpListener(IPAddress.Any, 5000);
        listener.Start();

        Console.WriteLine("TCP server listening on port 5000");

        while (!stoppingToken.IsCancellationRequested)
        {
            var client = await listener.AcceptTcpClientAsync();

            _ = Task.Run(async () =>
            {
                using var tcpClient = client;
                using var stream = tcpClient.GetStream();
                using var reader = new StreamReader(stream);

                while (true)
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

                        _store.Add(measurement);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid JSON received.");
                    }
                }
            });
        }
    }
}