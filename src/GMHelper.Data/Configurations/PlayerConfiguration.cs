using GMHelper.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GMHelper.Data.Configurations;

public class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.Property(p => p.CharacterName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.PlayerName).HasMaxLength(200);
        builder.Property(p => p.Notes).HasMaxLength(4000);

        builder.HasIndex(p => p.CampaignId);

        builder.HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(p => p.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
