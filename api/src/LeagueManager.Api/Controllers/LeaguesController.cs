using LeagueManager.Api.Dtos;
using LeagueManager.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeagueManager.Api.Controllers;

[ApiController]
[Route("api/v1/leagues")]
public class LeaguesController : ControllerBase
{
    private readonly ILeagueService _leagueService;

    public LeaguesController(ILeagueService leagueService)
    {
        _leagueService = leagueService;
    }

    [HttpPost]
    [ProducesResponseType<LeagueResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LeagueResponse>> CreateLeague(
        CreateLeagueRequest request,
        CancellationToken cancellationToken)
    {
        var league = await _leagueService.CreateLeagueAsync(request, cancellationToken);

        return Created($"/api/v1/leagues/{league.Id}", league);
    }
}