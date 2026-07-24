using Moq;
using ScreenSound.Shared.Modelos.Response;
using System.Text;

namespace ScreenSound.Tests.Integracao;

public class ArtistasAPITest
{
    private static HttpClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> handler, Uri? baseAddress = null)
    {
        var messageHandler = new StubHttpMessageHandler(handler);
        var client = new HttpClient(messageHandler);
        if (baseAddress != null) client.BaseAddress = baseAddress;
        return client;
    }

    [Fact]
    public async Task GetArtistasAsync_DeveChamarEndpointECapturarLista()
    {
        // Arrange
        var artistasEsperados = new List<ArtistaResponse>
        {
            new ArtistaResponse(1, "Artista 1", "", null),
            new ArtistaResponse(2, "Artista 2", "", null)
        };

        var handler = new Func<HttpRequestMessage, HttpResponseMessage>(request =>
        {
            if (request.Method == HttpMethod.Get && request.RequestUri!.PathAndQuery == "/artistas")
            {
                var json = System.Text.Json.JsonSerializer.Serialize(artistasEsperados);
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
        });

        var httpClient = CreateClient(handler, new Uri("http://localhost/"));

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("API")).Returns(httpClient);

        var api = new WebAssembly.Services.ArtistasAPI(factory.Object);

        // Act
        var artistasObtidos = await api.GetArtistasAsync();

        // Assert
        Assert.NotNull(artistasObtidos);
        Assert.Equal(artistasEsperados.Count, artistasObtidos.Count);
        for (int i = 0; i < artistasEsperados.Count; i++)
        {
            Assert.Equal(artistasEsperados[i].Id, artistasObtidos.ElementAt(i).Id);
            Assert.Equal(artistasEsperados[i].Nome, artistasObtidos.ElementAt(i).Nome);
        }
    }
}

sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
    public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        _handler = handler;
    }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(_handler(request));
}
