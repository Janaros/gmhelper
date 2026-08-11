using GMHelper.Core.Abstractions;

namespace GMHelper.Services;

/// <summary>Produktiv-Würfel auf Basis von <see cref="Random.Shared"/> (thread-sicher).</summary>
public class SystemDiceRoller : IDiceRoller
{
    public int Roll(int sides)
    {
        if (sides < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(sides), sides, "Ein Würfel braucht mindestens eine Seite.");
        }

        return Random.Shared.Next(1, sides + 1);
    }
}
