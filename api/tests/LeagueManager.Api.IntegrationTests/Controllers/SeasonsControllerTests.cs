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

    [Fact]
    public async Task CreateSeason_ValidRequest_Returns201WithSeason()
    {
        // Arrange
        var league = await CreateLeagueAsync("Test League");
        var startDate = new DateOnly(2027, 3, 1);
        var endDate = new DateOnly(2027, 6, 30);
        var request = new CreateSeasonRequest("Spring Season", startDate, endDate);

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/leagues/{league.Id}/seasons",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var season = await response.Content.ReadFromJsonAsync<SeasonResponse>();
        Assert.NotNull(season);
        Assert.InRange(season.Id, 1, int.MaxValue);
        Assert.Equal(league.Id, season.LeagueId);
        Assert.Equal(request.Name, season.Name);
        Assert.Equal(startDate, season.StartDate);
        Assert.Equal(endDate, season.EndDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateSeason_EndDateNotAfterStartDate_Returns400(int daysAfterStart)
    {
        // Arrange
        var league = await CreateLeagueAsync("Test League");

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

    [Fact]
    public async Task CreateSeason_BlankName_Returns400()
    {
        // Arrange
        var league = await CreateLeagueAsync("Test League");
        var request = new CreateSeasonRequest(
            " ",
            new DateOnly(2027, 3, 1),
            new DateOnly(2027, 6, 30));

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/leagues/{league.Id}/seasons",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("Name", problem.Errors.Keys);
    }

    [Fact]
    public async Task CreateSeason_NameTooLong_Returns400()
    {
        // Arrange
        var league = await CreateLeagueAsync("Test League");
        var request = new CreateSeasonRequest(
            new string('a', 101),
            new DateOnly(2027, 3, 1),
            new DateOnly(2027, 6, 30));

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/leagues/{league.Id}/seasons",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("Name", problem.Errors.Keys);
    }

    [Theory]
    [InlineData("StartDate")]
    [InlineData("EndDate")]
    public async Task CreateSeason_MissingDate_Returns400(string missingField)
    {
        // Arrange
        var league = await CreateLeagueAsync("Test League");
        var validDate = new DateOnly(2027, 3, 1);
        var request = new CreateSeasonRequest(
            "Test Season",
            missingField == "StartDate" ? (DateOnly?)null : validDate,
            missingField == "EndDate" ? (DateOnly?)null : validDate.AddMonths(3));

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/leagues/{league.Id}/seasons",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains(missingField, problem.Errors.Keys);
    }

    private async Task<LeagueResponse> CreateLeagueAsync(string name)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/v1/leagues",
            new CreateLeagueRequest(name));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var league = await response.Content.ReadFromJsonAsync<LeagueResponse>();
        Assert.NotNull(league);
        return league;
    }
}
