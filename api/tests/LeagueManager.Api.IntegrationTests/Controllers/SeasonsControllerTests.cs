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

    [Fact]
    public async Task CreateSeason_LeagueNotFound_Returns404()
    {
        // Arrange
        var missingLeagueId = 99999;
        var request = new CreateSeasonRequest(
            "Spring Season",
            new DateOnly(2027, 3, 1),
            new DateOnly(2027, 6, 30));

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/leagues/{missingLeagueId}/seasons",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal($"League {missingLeagueId} was not found.", problem.Detail);
    }

    [Theory]
    [InlineData("Spring 2027")]
    [InlineData("spring 2027")]
    public async Task CreateSeason_DuplicateNameInSameLeague_Returns409(string duplicateName)
    {
        // Arrange
        var league = await CreateLeagueAsync("Test League");
        await CreateSeasonAsync(league.Id, "Spring 2027");
        var request = new CreateSeasonRequest(
            duplicateName,
            new DateOnly(2027, 3, 1),
            new DateOnly(2027, 6, 30));

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/leagues/{league.Id}/seasons",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateSeason_SameNameInDifferentLeague_Returns201()
    {
        // Arrange
        var firstLeague = await CreateLeagueAsync("First League");
        var secondLeague = await CreateLeagueAsync("Second League");
        await CreateSeasonAsync(firstLeague.Id, "Spring 2027");

        var request = new CreateSeasonRequest(
            "Spring 2027",
            new DateOnly(2027, 3, 1),
            new DateOnly(2027, 6, 30));

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/leagues/{secondLeague.Id}/seasons",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var season = await response.Content.ReadFromJsonAsync<SeasonResponse>();
        Assert.NotNull(season);
        Assert.Equal(secondLeague.Id, season.LeagueId);
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
    public async Task CreateSeason_MissingName_Returns400()
    {
        // Arrange
        var league = await CreateLeagueAsync("Test League");
        var request = new CreateSeasonRequest(
            null!,
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

    private async Task<SeasonResponse> CreateSeasonAsync(int leagueId, string name)
    {
        var request = new CreateSeasonRequest(
            name,
            new DateOnly(2027, 3, 1),
            new DateOnly(2027, 6, 30));
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/leagues/{leagueId}/seasons",
            request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var season = await response.Content.ReadFromJsonAsync<SeasonResponse>();
        Assert.NotNull(season);
        return season;
    }
}
