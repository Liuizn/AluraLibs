using FluentAssertions;

namespace ScreenSound.Tests.Integracao;

public class EndPointArtistasTest
{
    [Fact]
    public async Task GetArtitas_Deve_Retornar_Ok_Mesmo_Sem_Existir_Artistas()
    {
        // Arrange
        var client = new HttpClient();
        var url = "http://localhost:5241/artistas";

        // Act
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().NotBeNull();
    }
}
