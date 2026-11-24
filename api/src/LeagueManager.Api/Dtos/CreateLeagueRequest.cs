using System.ComponentModel.DataAnnotations;

namespace LeagueManager.Api.Dtos;

public record CreateLeagueRequest(
    [Required] [StringLength(100)] string Name);