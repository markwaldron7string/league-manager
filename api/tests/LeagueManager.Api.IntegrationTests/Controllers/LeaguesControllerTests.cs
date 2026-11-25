using System.Net;
using System.Net.Http.Json;
using LeagueManager.Api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.Api.IntegrationTests.Controllers;

public class LeaguesControllerTests : IClassFixture<LeagueManagerApiFactory>, IAsyncLifetime
{
    private readonly LeagueManagerApiFactory _factory;
    private readonly HttpClient _client;

    public LeaguesControllerTests(LeagueManagerApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

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

    [Fact]
    public async Task CreateLeague_BlankName_Returns400()
    {
        // Arrange
        var request = new CreateLeagueRequest("   ");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/leagues", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("Name", problem.Errors.Keys);
    }

    [Fact]
    public async Task CreateLeague_NameTooLong_Returns400()
    {
        // Arrange
        var request = new CreateLeagueRequest(new string('a', 101));

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/leagues", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("Name", problem.Errors.Keys);
    }
}