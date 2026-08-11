namespace GMHelper.Core.Abstractions;

/// <summary>
/// Legt die mitgelieferten Schwertküsten-Gebiete an. Bewusst nicht als EF-<c>HasData</c>
/// modelliert: EF würde die Zeilen als von der Migration verwaltet betrachten und spätere
/// Änderungen des GM bei jeder Migration wieder überschreiben. Stattdessen wird nur in eine
/// noch leere Tabelle geschrieben, danach gehören die Daten dem Nutzer.
/// </summary>
public interface IHerbalismSeeder
{
    Task EnsureSeededAsync(CancellationToken cancellationToken = default);
}
