using LeagueManager.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LeagueManager.Api.Data;

public class LeagueManagerDbContext : DbContext
{
    public LeagueManagerDbContext(DbContextOptions<LeagueManagerDbContext> options)
        : base(options)
    {
    }

    public DbSet<League> Leagues => Set<League>();

    public DbSet<Season> Seasons => Set<Season>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LeagueManagerDbContext).Assembly);
    }
}