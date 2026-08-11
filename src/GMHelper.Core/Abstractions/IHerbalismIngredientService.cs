using GMHelper.Core.Entities;
using GMHelper.Core.Enums;

namespace GMHelper.Core.Abstractions;

public interface IHerbalismIngredientService
{
    Task<IReadOnlyList<HerbalismIngredient>> GetIngredientsAsync(int regionId, CancellationToken cancellationToken = default);
    Task<HerbalismIngredient> CreateIngredientAsync(int regionId, string name, CancellationToken cancellationToken = default);

    Task UpdateIngredientAsync(
        int ingredientId,
        string name,
        IngredientKind kind,
        IngredientRarity rarity,
        string? effect,
        string? notes,
        int? valueInGoldPieces,
        CancellationToken cancellationToken = default);

    Task DeleteIngredientAsync(int ingredientId, CancellationToken cancellationToken = default);
}
