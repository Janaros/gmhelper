namespace GMHelper.Core.Entities;

/// <summary>
/// Ein Sammelgebiet (z.B. "Neverwinterwald"). <see cref="DifficultyClass"/> ist der SG des
/// Weisheit(Überleben)-Wurfs beim Sammeln und bildet damit ab, wie ergiebig das Gelände ist
/// (angelehnt an die Nahrungssuche-Regel: ergiebig SG 10, normal SG 15, karg SG 20).
/// </summary>
public class HerbalismRegion
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Terrain { get; set; }
    public string? Description { get; set; }
    public int DifficultyClass { get; set; }

    /// <summary>"Seed" für die mitgelieferten Schwertküsten-Gebiete, "Manual" für selbst
    /// angelegte — damit bleibt unterscheidbar, was aus den Startdaten stammt.</summary>
    public required string Source { get; set; }

    public DateTime CreatedAt { get; set; }
}
