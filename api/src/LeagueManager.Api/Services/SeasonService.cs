using LeagueManager.Api.Data;
using LeagueManager.Api.Dtos;
using LeagueManager.Api.Exceptions;
using LeagueManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Api.Services;

public class SeasonService : ISeasonService
{
    private readonly LeagueManagerDbContext _db;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<SeasonService> _logger;

    public SeasonService(
        LeagueManagerDbContext db,
        TimeProvider timeProvider,
        ILogger<SeasonService> logger)
    {
        _db = db;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<SeasonResponse> CreateSeasonAsync(
        int leagueId,
        CreateSeasonRequest request,
        CancellationToken cancellationToken)
    {
        var startDate = request.StartDate
            ?? throw new ArgumentException("StartDate is required.", nameof(request));
        var endDate = request.EndDate
            ?? throw new ArgumentException("EndDate is required.", nameof(request));

        var leagueExists = await _db.Leagues
            .AnyAsync(league => league.Id == leagueId, cancellationToken);

        if (!leagueExists)
        {
            throw new NotFoundException($"League {leagueId} was not found.");
        }

        var name = request.Name.Trim();

        var nameTaken = await _db.Seasons
            .AnyAsync(
                season => season.LeagueId == leagueId && season.Name == name,
                cancellationToken);

        if (nameTaken)
        {
            throw new ConflictException(
                $"A season named \"{name}\" already exists in league {leagueId}.");
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var season = new Season
        {
            LeagueId = leagueId,
            Name = name,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = now,
            UpdatedAt = now,
        };

        _db.Seasons.Add(season);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created season {SeasonId} in league {LeagueId}",
            season.Id,
            leagueId);

        return new SeasonResponse(
            season.Id,
            season.LeagueId,
            season.Name,
            season.StartDate,
            season.EndDate);
    }
}