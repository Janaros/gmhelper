using GMHelper.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GMHelper.Data.Configurations;

public class SessionNoteConfiguration : IEntityTypeConfiguration<SessionNote>
{
    public void Configure(EntityTypeBuilder<SessionNote> builder)
    {
        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.MarkdownContent).IsRequired();

        builder.HasIndex(n => new { n.CampaignId, n.SessionDate });

        builder.HasOne<Campaign>()
            .WithMany()
            .HasForeignKey(n => n.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
