namespace LeagueManager.Api.Models;

public class League
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}