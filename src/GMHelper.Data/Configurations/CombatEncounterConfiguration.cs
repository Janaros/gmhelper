using GMHelper.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GMHelper.Data.Configurations;

public class CombatEncounterConfiguration : IEntityTypeConfiguration<CombatEncounter>
{
    public void Configure(EntityTypeBuilder<CombatEncounter> builder)
    {
        builder.HasIndex(e => new { e.CampaignId, e.IsActive });

        builder.HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(e => e.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
