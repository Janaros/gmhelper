using GMHelper.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GMHelper.Data.Configurations;

public class CombatParticipantConfiguration : IEntityTypeConfiguration<CombatParticipant>
{
    public void Configure(EntityTypeBuilder<CombatParticipant> builder)
    {
        builder.Property(p => p.DisplayName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.ConditionsText).HasMaxLength(2000);
        builder.Property(p => p.TokenNumber).HasMaxLength(2);

        builder.HasIndex(p => p.CombatEncounterId);

        builder.HasOne<CombatEncounter>()
            .WithMany()
            .HasForeignKey(p => p.CombatEncounterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
