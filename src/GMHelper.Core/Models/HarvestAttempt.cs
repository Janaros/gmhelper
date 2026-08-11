using GMHelper.Core.Entities;
using GMHelper.Core.Enums;

namespace GMHelper.Core.Models;

/// <summary>
/// Eingabe eines Sammelversuchs. Die Zutatentabelle wird als fertige Liste übergeben statt
/// über eine Id nachgeladen — so bleibt die Wurflogik frei von Datenbankzugriff und ist mit
/// einem gestellten Würfel vollständig deterministisch testbar.
/// </summary>
/// <param name="Region">Gebiet, dessen SG gegen den Wurf geprüft wird.</param>
/// <param name="AvailableIngredients">Fundtabelle des Gebiets.</param>
/// <param name="SkillModifier">Modifikator des Weisheit(Überleben)-Wurfs.</param>
/// <param name="UseHerbalismKit">Übung mit dem Kräuterkundeset — würfelt mit Vorteil.</param>
/// <param name="KindFilter">Auf Trank- bzw. Zauberzutaten beschränken; <c>null</c> sucht alles.</param>
public record HarvestAttempt(
    HerbalismRegion Region,
    IReadOnlyList<HerbalismIngredient> AvailableIngredients,
    int SkillModifier,
    bool UseHerbalismKit,
    IngredientKind? KindFilter);
