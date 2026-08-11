using GMHelper.Core.Enums;

namespace GMHelper.App.ViewModels;

/// <summary>Deutsche Beschriftungen der Zutaten-Enums — an einer Stelle, damit Tabelle,
/// Bearbeitungsformular und Fundliste dieselben Begriffe verwenden.</summary>
public static class HerbalismLabels
{
    public static string For(IngredientKind kind) => kind switch
    {
        IngredientKind.PotionIngredient => "Trankzutat",
        IngredientKind.SpellComponent => "Zauberzutat",
        IngredientKind.Both => "Trank + Zauber",
        _ => kind.ToString(),
    };

    public static string For(IngredientRarity rarity) => rarity switch
    {
        IngredientRarity.Common => "Gewöhnlich",
        IngredientRarity.Uncommon => "Ungewöhnlich",
        IngredientRarity.Rare => "Selten",
        IngredientRarity.VeryRare => "Sehr selten",
        _ => rarity.ToString(),
    };
}

/// <summary>Eintrag der Filter-Auswahlbox über der Fundtabelle. <c>null</c> steht für "alles".</summary>
public record KindFilterOption(string Label, IngredientKind? Value);

/// <summary>Eintrag der Auswahlbox "Art" im Bearbeitungsformular.</summary>
public record KindOption(string Label, IngredientKind Value);

/// <summary>Eintrag der Auswahlbox "Seltenheit" im Bearbeitungsformular.</summary>
public record RarityOption(string Label, IngredientRarity Value);
