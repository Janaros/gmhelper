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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
