using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Data;
using Microsoft.EntityFrameworkCore;

namespace GMHelper.Services;

public class HerbalismRegionService : IHerbalismRegionService
{
    /// <summary>SG eines neu angelegten Gebiets: "normales" Gelände als neutraler Startwert.</summary>
    private const int DefaultDifficultyClass = 15;

    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public HerbalismRegionService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyList<HerbalismRegion>> GetRegionsAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.HerbalismRegions
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<HerbalismRegion> CreateRegionAsync(string name, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var region = new HerbalismRegion
        {
            Name = name,
            DifficultyClass = DefaultDifficultyClass,
            Source = "Manual",
            CreatedAt = DateTime.UtcNow,
        };

        db.HerbalismRegions.Add(region);
        await db.SaveChangesAsync(cancellationToken);

        return region;
    }

    public async Task UpdateRegionAsync(int regionId, string name, string? terrain, string? description, int difficultyClass, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var region = await db.HerbalismRegions.FindAsync([regionId], cancellationToken);
        if (region is null)
        {
            return;
        }

        region.Name = name;
        region.Terrain = terrain;
        region.Description = description;
        region.DifficultyClass = difficultyClass;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteRegionAsync(int regionId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var region = await db.HerbalismRegions.FindAsync([regionId], cancellationToken);
        if (region is null)
        {
            return;
        }

        // Die Fundtabelle hängt per Kaskade am Gebiet, siehe HerbalismIngredientConfiguration.
        db.HerbalismRegions.Remove(region);
        await db.SaveChangesAsync(cancellationToken);
    }
}
