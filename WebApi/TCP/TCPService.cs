using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using Entities;

namespace WebApi.TCP;

public class TCPService : BackgroundService
{
    private const string CloudEndpoint =
        "https://sep4x-iot.azurewebsites.net/sensor-data/iot";

    private const string ApiKey =
        "sep4-iot-secret";

    private readonly IHttpClientFactory _httpClientFactory;

    public TCPService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
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

                await SendMeasurementToCloud(measurement, stoppingToken);

                Console.WriteLine(DateTime.UtcNow + " Measurement sent to cloud.");
            }
            catch (JsonException)
            {
                Console.WriteLine("Invalid JSON received.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to send measurement to cloud: " + ex.Message);
            }
        }
    }

    private async Task SendMeasurementToCloud(
        Measurement measurement,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();

        var payload = new
        {
            temperature = measurement.Temperature,
            humidity = measurement.Humidity,
            light = measurement.Light
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, CloudEndpoint);
        request.Headers.Add("X-Api-Key", ApiKey);
        request.Content = JsonContent.Create(payload);

        var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Cloud API returned {(int)response.StatusCode}: {responseText}");
        }
    }
}