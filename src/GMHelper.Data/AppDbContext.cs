using GMHelper.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GMHelper.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<PdfDocument> PdfDocuments => Set<PdfDocument>();
    public DbSet<ImageAsset> ImageAssets => Set<ImageAsset>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<StatField> StatFields => Set<StatField>();
    public DbSet<Monster> Monsters => Set<Monster>();
    public DbSet<CombatEncounter> CombatEncounters => Set<CombatEncounter>();
    public DbSet<CombatParticipant> CombatParticipants => Set<CombatParticipant>();
    public DbSet<SessionNote> SessionNotes => Set<SessionNote>();
    public DbSet<PdfJumpMarker> PdfJumpMarkers => Set<PdfJumpMarker>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
