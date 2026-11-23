using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Api.Data;

public class LeagueManagerDbContext : DbContext
{
    public LeagueManagerDbContext(DbContextOptions<LeagueManagerDbContext> options)
        : base(options)
    {
    }
}