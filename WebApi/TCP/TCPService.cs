using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using Entities;
using Entities.Domain.Scenario;

namespace WebApi.TCP;

public class TCPService : BackgroundService
{
    private const string SharedRoomId = "shared";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TCPService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
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
                var measurement = JsonSerializer.Deserialize<Measurement>(line, JsonOptions);

                if (measurement == null)
                    continue;

                var scenario = await GetCurrentScenarioFromMal(stoppingToken);

                if (scenario != null)
                {
                    await ApplyScenarioActions(measurement, scenario, stoppingToken);
                }
                else
                {
                    Console.WriteLine("No scenario received from MAL server.");
                }

                await SendMeasurementToCloud(measurement, stoppingToken);

                Console.WriteLine(DateTime.UtcNow + " Measurement sent to cloud.");
            }
            catch (JsonException)
            {
                Console.WriteLine("Invalid JSON received.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to process measurement: " + ex.Message);
            }
        }
    }

    private async Task<Scenario?> GetCurrentScenarioFromMal(CancellationToken cancellationToken)
    {
        var endpoint = GetRequiredConfig("Mal:ScenarioEndpoint");

        var client = _httpClientFactory.CreateClient();

        try
        {
            using var response = await client.GetAsync(endpoint, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"MAL API returned {(int)response.StatusCode}: {responseText}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Scenario>(
                JsonOptions,
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to get scenario from MAL: " + ex.Message);
            return null;
        }
    }

    private async Task ApplyScenarioActions(
        Measurement measurement,
        Scenario scenario,
        CancellationToken cancellationToken)
    {
        Console.WriteLine(
            $"Scenario received. PrefTemp={scenario.PrefTemperature}, PrefHumidity={scenario.PrefHumidity}"
        );

        if (measurement.Temperature > scenario.PrefTemperature)
        {
            await SendDeviceActionToCloud(DeviceType.Heater, DeviceState.Off, cancellationToken);
            await SendDeviceActionToCloud(DeviceType.Window, DeviceState.Open, cancellationToken);
        }
        else if (measurement.Temperature < scenario.PrefTemperature)
        {
            await SendDeviceActionToCloud(DeviceType.Window, DeviceState.Closed, cancellationToken);
            await SendDeviceActionToCloud(DeviceType.Heater, DeviceState.On, cancellationToken);
        }

        if (measurement.Humidity > scenario.PrefHumidity)
        {
            await SendDeviceActionToCloud(DeviceType.Humidifier, DeviceState.Off, cancellationToken);
        }
        else if (measurement.Humidity < scenario.PrefHumidity)
        {
            await SendDeviceActionToCloud(DeviceType.Humidifier, DeviceState.On, cancellationToken);
        }
    }

    private async Task SendDeviceActionToCloud(
        DeviceType device,
        DeviceState state,
        CancellationToken cancellationToken)
    {
        var endpoint = GetRequiredConfig("CloudApi:DeviceActionEndpoint");
        var apiKey = GetRequiredConfig("IoT:ApiKey");

        var client = _httpClientFactory.CreateClient();

        var payload = new
        {
            roomId = SharedRoomId,
            device = device.ToString(),
            state = state.ToString()
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Add("X-Api-Key", apiKey);
        request.Content = JsonContent.Create(payload);

        try
        {
            var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

                Console.WriteLine(
                    $"Cloud device action API returned {(int)response.StatusCode}: {responseText}"
                );

                return;
            }

            Console.WriteLine($"Device action sent: {device} -> {state}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send device action {device} -> {state}: {ex.Message}");
        }
    }

    private async Task SendMeasurementToCloud(
        Measurement measurement,
        CancellationToken cancellationToken)
    {
        var endpoint = GetRequiredConfig("CloudApi:MeasurementEndpoint");
        var apiKey = GetRequiredConfig("IoT:ApiKey");

        var client = _httpClientFactory.CreateClient();

        var payload = new
        {
            temperature = measurement.Temperature,
            humidity = measurement.Humidity,
            light = measurement.Light
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Add("X-Api-Key", apiKey);
        request.Content = JsonContent.Create(payload);

        var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new Exception(
                $"Cloud measurement API returned {(int)response.StatusCode}: {responseText}"
            );
        }
    }

    private string GetRequiredConfig(string key)
    {
        return _configuration[key]
            ?? throw new InvalidOperationException($"Missing configuration value: {key}");
    }
}