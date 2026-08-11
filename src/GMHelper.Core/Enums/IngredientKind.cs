namespace GMHelper.Core.Enums;

/// <summary>
/// Wofür eine gesammelte Zutat taugt. <see cref="Both"/> ist ein eigener Wert statt zweier
/// Flags, damit eine Zutat genau eine Zeile in der Gebietstabelle belegt und der Filter
/// "nur Trankzutaten" sie trotzdem findet.
/// </summary>
public enum IngredientKind
{
    PotionIngredient = 0,
    SpellComponent = 1,
    Both = 2,
}
