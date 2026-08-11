namespace GMHelper.Core.Abstractions;

/// <summary>
/// Einzige Zufallsquelle der Sammel-Logik. Absichtlich auf einen Wurf reduziert, damit Tests
/// eine feste Wurffolge vorgeben können und die Wurflogik selbst deterministisch bleibt.
/// </summary>
public interface IDiceRoller
{
    /// <summary>Wirft einen Würfel mit <paramref name="sides"/> Seiten; Ergebnis 1..sides.</summary>
    int Roll(int sides);
}
