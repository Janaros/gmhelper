using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Core.Enums;
using GMHelper.Core.Models;

namespace GMHelper.Services;

/// <summary>
/// Wertet einen Sammelversuch aus, angelehnt an die Nahrungssuche-Regel: ein
/// Weisheit(Überleben)-Wurf gegen den SG des Geländes, wobei die Höhe des Erfolgs bestimmt,
/// wie viel und wie Seltenes gefunden wird. Übung mit dem Kräuterkundeset gibt Vorteil.
/// </summary>
public class HerbalismHarvestService : IHerbalismHarvestService
{
    /// <summary>Je volle 5 Punkte über dem SG eine Zutat mehr — gedeckelt, damit ein
    /// Ausnahmewurf nicht die halbe Tabelle ausräumt.</summary>
    private const int MarginPerAdditionalFind = 5;
    private const int MaxFinds = 4;

    /// <summary>Mindest-Überschuss über den SG, ab dem eine Seltenheitsstufe überhaupt
    /// auftauchen kann. Ein knapper Erfolg liefert also nur Alltagskräuter.</summary>
    private static readonly Dictionary<IngredientRarity, int> RarityMarginThresholds = new()
    {
        [IngredientRarity.Common] = 0,
        [IngredientRarity.Uncommon] = 5,
        [IngredientRarity.Rare] = 10,
        [IngredientRarity.VeryRare] = 15,
    };

    /// <summary>Ziehgewicht innerhalb der freigeschalteten Zutaten: Seltenes bleibt selten,
    /// auch wenn der Wurf es freigeschaltet hat.</summary>
    private static readonly Dictionary<IngredientRarity, int> RarityWeights = new()
    {
        [IngredientRarity.Common] = 8,
        [IngredientRarity.Uncommon] = 4,
        [IngredientRarity.Rare] = 2,
        [IngredientRarity.VeryRare] = 1,
    };

    private readonly IDiceRoller _diceRoller;

    public HerbalismHarvestService(IDiceRoller diceRoller)
    {
        _diceRoller = diceRoller;
    }

    public HarvestOutcome Resolve(HarvestAttempt attempt)
    {
        var firstRoll = _diceRoller.Roll(20);

        int diceRoll;
        int? discardedRoll = null;
        if (attempt.UseHerbalismKit)
        {
            var secondRoll = _diceRoller.Roll(20);
            diceRoll = Math.Max(firstRoll, secondRoll);
            discardedRoll = Math.Min(firstRoll, secondRoll);
        }
        else
        {
            diceRoll = firstRoll;
        }

        var total = diceRoll + attempt.SkillModifier;
        var difficultyClass = attempt.Region.DifficultyClass;
        var margin = total - difficultyClass;

        if (margin < 0)
        {
            return new HarvestOutcome(diceRoll, discardedRoll, attempt.SkillModifier, total, difficultyClass, margin, IsSuccess: false, []);
        }

        var candidates = attempt.AvailableIngredients
            .Where(ingredient => ingredient.Kind.MatchesFilter(attempt.KindFilter))
            .Where(ingredient => margin >= RarityMarginThresholds[ingredient.Rarity])
            .ToList();

        var findCount = Math.Min(MaxFinds, 1 + (margin / MarginPerAdditionalFind));
        var quantities = new Dictionary<int, int>();
        var drawOrder = new List<HerbalismIngredient>();

        for (var i = 0; i < findCount && candidates.Count > 0; i++)
        {
            var picked = PickWeighted(candidates);
            if (quantities.TryGetValue(picked.Id, out var quantity))
            {
                quantities[picked.Id] = quantity + 1;
            }
            else
            {
                quantities[picked.Id] = 1;
                drawOrder.Add(picked);
            }
        }

        var finds = drawOrder
            .Select(ingredient => new HarvestFind(ingredient, quantities[ingredient.Id]))
            .ToList();

        return new HarvestOutcome(diceRoll, discardedRoll, attempt.SkillModifier, total, difficultyClass, margin, IsSuccess: true, finds);
    }

    private HerbalismIngredient PickWeighted(IReadOnlyList<HerbalismIngredient> candidates)
    {
        var totalWeight = candidates.Sum(candidate => RarityWeights[candidate.Rarity]);
        var pick = _diceRoller.Roll(totalWeight);

        var cursor = 0;
        foreach (var candidate in candidates)
        {
            cursor += RarityWeights[candidate.Rarity];
            if (pick <= cursor)
            {
                return candidate;
            }
        }

        // Nur erreichbar, wenn der Würfel über den Gesamtbereich hinaus liefert.
        return candidates[^1];
    }
}
