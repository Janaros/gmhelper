using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Data;
using Microsoft.EntityFrameworkCore;

namespace GMHelper.Services;

/// <inheritdoc cref="IHerbalismSeeder"/>
public class HerbalismSeeder : IHerbalismSeeder
{
    /// <summary>Markiert die mitgelieferten Gebiete, siehe <see cref="HerbalismRegion.Source"/>.</summary>
    public const string SeedSource = "Seed";

    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public HerbalismSeeder(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task EnsureSeededAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Nur befüllen, solange der GM noch nichts angelegt hat. Wer alle Gebiete löscht,
        // bekommt die Startdaten beim nächsten Öffnen bewusst wieder — das ist die einzige
        // Möglichkeit, sie ohne Neuinstallation zurückzuholen.
        if (await db.HerbalismRegions.AnyAsync(cancellationToken))
        {
            return;
        }

        var createdAt = DateTime.UtcNow;

        foreach (var seedRegion in HerbalismSeedData.Regions)
        {
            var region = new HerbalismRegion
            {
                Name = seedRegion.Name,
                Terrain = seedRegion.Terrain,
                Description = seedRegion.Description,
                DifficultyClass = seedRegion.DifficultyClass,
                Source = SeedSource,
                CreatedAt = createdAt,
            };

            db.HerbalismRegions.Add(region);
            // Zwischenspeichern, damit region.Id für die Zutaten feststeht.
            await db.SaveChangesAsync(cancellationToken);

            foreach (var seedIngredient in seedRegion.Ingredients)
            {
                db.HerbalismIngredients.Add(new HerbalismIngredient
                {
                    HerbalismRegionId = region.Id,
                    Name = seedIngredient.Name,
                    Kind = seedIngredient.Kind,
                    Rarity = seedIngredient.Rarity,
                    Effect = seedIngredient.Effect,
                    ValueInGoldPieces = seedIngredient.ValueInGoldPieces,
                    CreatedAt = createdAt,
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
