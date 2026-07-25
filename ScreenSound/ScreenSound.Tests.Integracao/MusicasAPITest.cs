using Moq;
using ScreenSound.Shared.Modelos.Response;
using ScreenSound.WebAssembly.Services;
using System.Text;

namespace ScreenSound.Tests.Integracao;

public class MusicasAPITest
{
    private static HttpClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> handler, Uri? baseAddress = null)
    {
        var messageHandler = new StubHttpMessageHandlerMusica(handler);
        var client = new HttpClient(messageHandler);
        if (baseAddress != null) client.BaseAddress = baseAddress;
        return client;
    }

    [Fact]
    [Trait("Categoria", "Integração")]
    public async Task GetMusicasAsync_DeveChamarEndpointECapturarLista()
    {
        // Arrange
        var musicasEsperadas = new List<MusicaResponse>
        {
            new MusicaResponse(1, "Musica 1", 1, "nome_artista", 2026, null),
            new MusicaResponse(2, "Musica 2", 1, "nome_artista", 2026, null)
        };

        var handler = new Func<HttpRequestMessage, HttpResponseMessage>(request =>
        {
            if (request.Method == HttpMethod.Get && request.RequestUri!.PathAndQuery == "/musicas")
            {
                var json = System.Text.Json.JsonSerializer.Serialize(musicasEsperadas);
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

        var ilogFactory = new Mock<Microsoft.Extensions.Logging.ILogger<MusicasAPI>>();

        var api = new MusicasAPI(factory.Object, ilogFactory.Object);

        // Act
        var musicasObtidas = await api.GetMusicasAsync();

        // Assert
        Assert.NotNull(musicasObtidas);
        Assert.Equal(musicasEsperadas.Count, musicasObtidas.Count);
        for (int i = 0; i < musicasEsperadas.Count; i++)
        {
            Assert.Equal(musicasEsperadas[i].Id, musicasObtidas.ElementAt(i).Id);
            Assert.Equal(musicasEsperadas[i].Nome, musicasObtidas.ElementAt(i).Nome);
        }
    }
}

sealed class StubHttpMessageHandlerMusica : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
    public StubHttpMessageHandlerMusica(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        _handler = handler;
    }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(_handler(request));
}
