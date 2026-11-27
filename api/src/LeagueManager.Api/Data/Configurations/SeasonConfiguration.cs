using LeagueManager.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeagueManager.Api.Data.Configurations;

public class SeasonConfiguration : IEntityTypeConfiguration<Season>
{
    public void Configure(EntityTypeBuilder<Season> builder)
    {
        builder.Property(season => season.Name)
            .HasMaxLength(100);

        builder.HasOne<League>()
            .WithMany()
            .HasForeignKey(season => season.LeagueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(season => new { season.LeagueId, season.Name })
            .IsUnique();
    }
}