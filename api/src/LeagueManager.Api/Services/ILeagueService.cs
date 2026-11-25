using LeagueManager.Api.Dtos;

namespace LeagueManager.Api.Services;

public interface ILeagueService
{
    Task<LeagueResponse> CreateLeagueAsync(CreateLeagueRequest request, CancellationToken cancellationToken);
    Task<LeagueResponse> GetLeagueByIdAsync(int id, CancellationToken cancellationToken);
}