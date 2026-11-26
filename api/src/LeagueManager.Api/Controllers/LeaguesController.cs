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

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<LeagueResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LeagueResponse>>> GetLeagues(
        CancellationToken cancellationToken)
    {
        var leagues = await _leagueService.GetLeaguesAsync(cancellationToken);
        return Ok(leagues);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<LeagueResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeagueResponse>> GetLeagueById(
        int id,
        CancellationToken cancellationToken)
    {
        var league = await _leagueService.GetLeagueByIdAsync(id, cancellationToken);
        return Ok(league);
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

        return CreatedAtAction(nameof(GetLeagueById), new { id = league.Id }, league);
    }
}