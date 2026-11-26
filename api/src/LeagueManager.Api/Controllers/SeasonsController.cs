using LeagueManager.Api.Dtos;
using LeagueManager.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.Api.Controllers;

[ApiController]
[Route("api/v1/leagues/{leagueId:int}/seasons")]
public class SeasonsController : ControllerBase
{
    private readonly ISeasonService _seasonService;

    public SeasonsController(ISeasonService seasonService)
    {
        _seasonService = seasonService;
    }

    [HttpPost]
    [ProducesResponseType<SeasonResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SeasonResponse>> CreateSeason(
        int leagueId,
        CreateSeasonRequest request,
        CancellationToken cancellationToken)
    {
        var season = await _seasonService.CreateSeasonAsync(leagueId, request, cancellationToken);
        return Created($"/api/v1/leagues/{leagueId}/seasons/{season.Id}", season);
    }
}