using LeagueManager.Api.Dtos;

namespace LeagueManager.Api.Services;

public interface ISeasonService
{
    Task<SeasonResponse> CreateSeasonAsync(
        int leagueId,
        CreateSeasonRequest request,
        CancellationToken cancellationToken);
}