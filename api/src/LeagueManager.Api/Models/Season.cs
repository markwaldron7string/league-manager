namespace LeagueManager.Api.Models;

public class Season
{
    public int Id { get; set; }
    public int LeagueId { get; set; }
    public required string Name { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}