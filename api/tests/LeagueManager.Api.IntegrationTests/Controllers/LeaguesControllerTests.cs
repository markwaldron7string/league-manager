using System.Net;
using System.Net.Http.Json;
using LeagueManager.Api.Data;
using LeagueManager.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

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
    public async Task CreateLeague_ValidName_SetsTimestampsFromClock()
    {
        // Arrange
        var expectedTime = new DateTimeOffset(2026, 3, 1, 14, 30, 0, TimeSpan.Zero);
        _factory.Clock.SetUtcNow(expectedTime);
        var request = new CreateLeagueRequest("Timestamp League");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/leagues", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var league = await response.Content.ReadFromJsonAsync<LeagueResponse>();
        Assert.NotNull(league);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<LeagueManagerDbContext>();
        var savedLeague = await dbContext.Leagues.FindAsync(league.Id);

        Assert.NotNull(savedLeague);
        Assert.Equal(expectedTime.UtcDateTime, savedLeague.CreatedAt);
        Assert.Equal(expectedTime.UtcDateTime, savedLeague.UpdatedAt);
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

    [Theory]
    [InlineData("Test League")]
    [InlineData("test league")]
    public async Task CreateLeague_DuplicateName_Returns409(string duplicateName)
    {
        // Arrange
        var originalRequest = new CreateLeagueRequest("Test League");
        var originalResponse = await _client.PostAsJsonAsync("/api/v1/leagues", originalRequest);
        Assert.Equal(HttpStatusCode.Created, originalResponse.StatusCode);

        var duplicateRequest = new CreateLeagueRequest(duplicateName);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/leagues", duplicateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetLeagueById_ExistingId_Returns200WithLeague()
    {
        // Arrange
        var request = new CreateLeagueRequest("Test League");
        var createResponse = await _client.PostAsJsonAsync("/api/v1/leagues", request);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdLeague = await createResponse.Content.ReadFromJsonAsync<LeagueResponse>();
        Assert.NotNull(createdLeague);

        // Act
        var response = await _client.GetAsync($"/api/v1/leagues/{createdLeague.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var league = await response.Content.ReadFromJsonAsync<LeagueResponse>();
        Assert.NotNull(league);
        Assert.Equal(createdLeague.Id, league.Id);
        Assert.Equal(request.Name, league.Name);
    }
}