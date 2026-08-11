using GMHelper.Core.Enums;

namespace GMHelper.Core.Entities;

/// <summary>
/// Eine Zeile in der Fundtabelle eines Gebiets. Bewusst an genau ein Gebiet gebunden: dieselbe
/// Pflanze in zwei Gebieten bekommt zwei Zeilen, weil Wirkung, Wert und Seltenheit regional
/// abweichen dürfen und der Sammelwurf immer nur eine Gebietstabelle zieht.
/// </summary>
public class HerbalismIngredient
{
    public int Id { get; set; }
    public int HerbalismRegionId { get; set; }
    public required string Name { get; set; }
    public IngredientKind Kind { get; set; }
    public IngredientRarity Rarity { get; set; }

    /// <summary>Wirkung bzw. Verwendung — das, was der GM am Tisch vorliest.</summary>
    public string? Effect { get; set; }

    public string? Notes { get; set; }

    /// <summary>Marktwert in Goldmünzen, optional.</summary>
    public int? ValueInGoldPieces { get; set; }

    public DateTime CreatedAt { get; set; }
}
