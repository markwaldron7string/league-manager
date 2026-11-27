using System.Net;
using System.Net.Http.Json;
using LeagueManager.Api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.Api.IntegrationTests.Controllers;

public class SeasonsControllerTests : IClassFixture<LeagueManagerApiFactory>, IAsyncLifetime
{
    private readonly LeagueManagerApiFactory _factory;
    private readonly HttpClient _client;

    public SeasonsControllerTests(LeagueManagerApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateSeason_EndDateNotAfterStartDate_Returns400(int daysAfterStart)
    {
        // Arrange
        var leagueResponse = await _client.PostAsJsonAsync(
            "/api/v1/leagues",
            new CreateLeagueRequest("Test League"));
        Assert.Equal(HttpStatusCode.Created, leagueResponse.StatusCode);

        var league = await leagueResponse.Content.ReadFromJsonAsync<LeagueResponse>();
        Assert.NotNull(league);

        var startDate = new DateOnly(2026, 1, 1);
        var request = new CreateSeasonRequest(
            "Test Season",
            startDate,
            startDate.AddDays(daysAfterStart));

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/leagues/{league.Id}/seasons",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("EndDate", problem.Errors.Keys);
    }
}
