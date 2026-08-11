using GMHelper.Core.Entities;

namespace GMHelper.Core.Models;

/// <summary>Ergebnis eines Sammelversuchs, inklusive der Würfel für die Nachvollziehbarkeit am Tisch.</summary>
/// <param name="DiceRoll">Gewerteter d20.</param>
/// <param name="DiscardedDiceRoll">Der verworfene zweite d20 bei Vorteil, sonst <c>null</c>.</param>
/// <param name="Total">Gewerteter Würfel plus Modifikator.</param>
/// <param name="Margin">Überschuss über den SG; negativ bei Misserfolg.</param>
public record HarvestOutcome(
    int DiceRoll,
    int? DiscardedDiceRoll,
    int SkillModifier,
    int Total,
    int DifficultyClass,
    int Margin,
    bool IsSuccess,
    IReadOnlyList<HarvestFind> Finds);

/// <summary>Eine gefundene Zutat samt Menge; mehrfach gezogene Zutaten werden hier gestapelt.</summary>
public record HarvestFind(HerbalismIngredient Ingredient, int Quantity);
