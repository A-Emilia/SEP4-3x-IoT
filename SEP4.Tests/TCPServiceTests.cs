using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Moq;
using Moq.Protected;
using WebApi.TCP;

namespace SEP4.Tests;

public class TCPServiceTests
{
    [Fact]
    public async Task HandleClientAsync_WithInvalidJson_ShouldNotSendRequest()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(handlerMock.Object);

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();

        httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var tcpService = new TCPService(httpClientFactoryMock.Object);

        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();

        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var client = new TcpClient();

        await client.ConnectAsync("127.0.0.1", port);

        var serverClient = await listener.AcceptTcpClientAsync();

        var invalidJson = "{ invalid json }\n";

        var bytes = Encoding.UTF8.GetBytes(invalidJson);

        await client.GetStream().WriteAsync(bytes);

        var method = typeof(TCPService)
            .GetMethod(
                "HandleClientAsync",
                BindingFlags.NonPublic | BindingFlags.Instance);

        Assert.NotNull(method);

        _ = (Task)method!.Invoke(
            tcpService,
            new object[]
            {
                serverClient,
                CancellationToken.None
            })!;

        await Task.Delay(200);

        // Assert
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task HandleClientAsync_WithValidJson_ShouldSendPostRequest()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var httpClient = new HttpClient(handlerMock.Object);

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();

        httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var tcpService = new TCPService(httpClientFactoryMock.Object);

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

        _ = (Task)method!.Invoke(
            tcpService,
            new object[]
            {
                serverClient,
                CancellationToken.None
            })!;

        await Task.Delay(200);

        // Assert
        handlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task HandleClientAsync_WhenCloudFails_ShouldNotCrash()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(handlerMock.Object);

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();

        httpClientFactoryMock
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var tcpService = new TCPService(httpClientFactoryMock.Object);

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