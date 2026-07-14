namespace GMHelper.App.Windows;

/// <summary>
/// Owns lifecycle of the secondary (player-facing) display window: created once at startup,
/// shown/hidden on demand rather than constructed per use, so its position/state persists.
/// </summary>
public interface IWindowManager
{
    void ShowSecondaryDisplay();
}
