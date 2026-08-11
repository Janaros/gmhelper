namespace GMHelper.Core.Enums;

public static class IngredientKindExtensions
{
    /// <summary>
    /// Einzige Stelle, an der die Filterregel definiert ist — sowohl die Tabellenanzeige als
    /// auch der Sammelwurf müssen dieselbe Menge treffen, sonst findet die App etwas, das der
    /// GM in der Tabelle gar nicht sieht.
    /// </summary>
    /// <param name="kind">Art der Zutat.</param>
    /// <param name="filter">Gesuchte Art; <c>null</c> lässt alles durch. Ein Filter auf
    /// <see cref="IngredientKind.Both"/> sucht gezielt nur die Doppelverwender, während ein
    /// Filter auf eine Einzelart die Doppelverwender mit einschließt.</param>
    public static bool MatchesFilter(this IngredientKind kind, IngredientKind? filter) => filter switch
    {
        null => true,
        IngredientKind.Both => kind == IngredientKind.Both,
        _ => kind == filter || kind == IngredientKind.Both,
    };
}
