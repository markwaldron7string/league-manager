using LeagueManager.Api.Data;
using LeagueManager.Api.Dtos;
using LeagueManager.Api.Exceptions;
using LeagueManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Api.Services;

public class LeagueService : ILeagueService
{
    private readonly LeagueManagerDbContext _db;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<LeagueService> _logger;

    public LeagueService(
        LeagueManagerDbContext db,
        TimeProvider timeProvider,
        ILogger<LeagueService> logger)
    {
        _db = db;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<LeagueResponse> CreateLeagueAsync(
        CreateLeagueRequest request,
        CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        var nameTaken = await _db.Leagues.AnyAsync(
            league => league.Name == name,
            cancellationToken);

        if (nameTaken)
        {
            throw new ConflictException($"A league named '{name}' already exists.");
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var league = new League
        {
            Name = name,
            CreatedAt = now,
            UpdatedAt = now,
        };

        _db.Leagues.Add(league);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created league {LeagueId}", league.Id);

        return new LeagueResponse(league.Id, league.Name);
    }

    public async Task<LeagueResponse> GetLeagueByIdAsync(int id, CancellationToken cancellationToken)
    {
        var league = await _db.Leagues
            .AsNoTracking()
            .FirstOrDefaultAsync(league => league.Id == id, cancellationToken)
            ?? throw new NotFoundException($"League {id} was not found.");

        return new LeagueResponse(league.Id, league.Name);
    }
}