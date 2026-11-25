using System.Net;
using System.Net.Http.Json;
using LeagueManager.Api.Dtos;

namespace LeagueManager.Api.IntegrationTests.Controllers;

public class LeaguesControllerTests : IClassFixture<LeagueManagerApiFactory>
{
    private readonly HttpClient _client;

    public LeaguesControllerTests(LeagueManagerApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateLeague_ValidName_Returns201WithLeague()
    {
        // Arrange
        var request = new CreateLeagueRequest("Test League");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/leagues", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var league = await response.Content.ReadFromJsonAsync<LeagueResponse>();
        Assert.NotNull(league);
        Assert.InRange(league.Id, 1, int.MaxValue);
        Assert.Equal(request.Name, league.Name);
    }
}