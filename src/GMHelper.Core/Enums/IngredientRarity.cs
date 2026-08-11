namespace GMHelper.Core.Enums;

/// <summary>
/// Seltenheit einer Zutat. Steuert beim Sammelwurf zweierlei: ab welchem Überschuss über den
/// SG die Zutat überhaupt gefunden werden kann, und wie wahrscheinlich sie gezogen wird.
/// </summary>
public enum IngredientRarity
{
    Common = 0,
    Uncommon = 1,
    Rare = 2,
    VeryRare = 3,
}
