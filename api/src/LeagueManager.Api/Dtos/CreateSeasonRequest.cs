using System.ComponentModel.DataAnnotations;

namespace LeagueManager.Api.Dtos;

public record CreateSeasonRequest(
    [Required][StringLength(100)] string Name,
    [Required] DateOnly? StartDate,
    [Required] DateOnly? EndDate);