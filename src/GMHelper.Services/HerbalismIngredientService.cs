using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Core.Enums;
using GMHelper.Data;
using Microsoft.EntityFrameworkCore;

namespace GMHelper.Services;

public class HerbalismIngredientService : IHerbalismIngredientService
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public HerbalismIngredientService(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyList<HerbalismIngredient>> GetIngredientsAsync(int regionId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.HerbalismIngredients
            .AsNoTracking()
            .Where(i => i.HerbalismRegionId == regionId)
            .OrderBy(i => i.Rarity)
            .ThenBy(i => i.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<HerbalismIngredient> CreateIngredientAsync(int regionId, string name, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var ingredient = new HerbalismIngredient
        {
            HerbalismRegionId = regionId,
            Name = name,
            Kind = IngredientKind.PotionIngredient,
            Rarity = IngredientRarity.Common,
            CreatedAt = DateTime.UtcNow,
        };

        db.HerbalismIngredients.Add(ingredient);
        await db.SaveChangesAsync(cancellationToken);

        return ingredient;
    }

    public async Task UpdateIngredientAsync(
        int ingredientId,
        string name,
        IngredientKind kind,
        IngredientRarity rarity,
        string? effect,
        string? notes,
        int? valueInGoldPieces,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var ingredient = await db.HerbalismIngredients.FindAsync([ingredientId], cancellationToken);
        if (ingredient is null)
        {
            return;
        }

        ingredient.Name = name;
        ingredient.Kind = kind;
        ingredient.Rarity = rarity;
        ingredient.Effect = effect;
        ingredient.Notes = notes;
        ingredient.ValueInGoldPieces = valueInGoldPieces;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteIngredientAsync(int ingredientId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var ingredient = await db.HerbalismIngredients.FindAsync([ingredientId], cancellationToken);
        if (ingredient is null)
        {
            return;
        }

        db.HerbalismIngredients.Remove(ingredient);
        await db.SaveChangesAsync(cancellationToken);
    }
}
