using GMHelper.Core.Models;

namespace GMHelper.Core.Abstractions;

/// <summary>Würfelt einen Sammelversuch aus. Reine Logik, kein Datenbank- oder Dateizugriff.</summary>
public interface IHerbalismHarvestService
{
    HarvestOutcome Resolve(HarvestAttempt attempt);
}
