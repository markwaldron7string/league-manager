namespace LeagueManager.Api.Dtos;

public record SeasonResponse(
    int Id,
    int LeagueId,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate);