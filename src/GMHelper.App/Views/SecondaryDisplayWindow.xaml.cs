using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using GMHelper.App.ViewModels;
using GMHelper.Core.Abstractions;

namespace GMHelper.App.Views;

public partial class SecondaryDisplayWindow : Window
{
    private readonly IAppPaths _appPaths;

    public SecondaryDisplayWindow(SecondaryDisplayViewModel viewModel, IAppPaths appPaths)
    {
        InitializeComponent();

        _appPaths = appPaths;
        DataContext = viewModel;

        SourceInitialized += SecondaryDisplayWindow_SourceInitialized;
        Closing += SecondaryDisplayWindow_Closing;
    }

    /// <summary>Called from App.xaml.cs on shutdown, since Closing is always canceled during
    /// normal operation (the window is hidden, not destroyed) and never fires for a real save point.</summary>
    public void SaveCurrentPlacement()
    {
        var placement = new WindowPlacement(RestoreBounds.Left, RestoreBounds.Top, RestoreBounds.Width, RestoreBounds.Height, WindowState == WindowState.Maximized);

        try
        {
            Directory.CreateDirectory(_appPaths.DataRoot);
            File.WriteAllText(PlacementFilePath, JsonSerializer.Serialize(placement));
        }
        catch (Exception)
        {
            // Best-effort — losing the remembered window placement is not worth surfacing an error for.
        }
    }

    private string PlacementFilePath => Path.Combine(_appPaths.DataRoot, "secondary-display-window.json");

    private void SecondaryDisplayWindow_SourceInitialized(object? sender, EventArgs e)
    {
        if (TryApplySavedPlacement())
        {
            return;
        }

        PositionOnSecondaryMonitorIfAvailable();
    }

    private bool TryApplySavedPlacement()
    {
        if (!File.Exists(PlacementFilePath))
        {
            return false;
        }

        try
        {
            var json = File.ReadAllText(PlacementFilePath);
            var placement = JsonSerializer.Deserialize<WindowPlacement>(json);
            if (placement is null || placement.Width <= 0 || placement.Height <= 0)
            {
                return false;
            }

            WindowState = WindowState.Normal;
            Left = placement.Left;
            Top = placement.Top;
            Width = placement.Width;
            Height = placement.Height;
            WindowState = placement.IsMaximized ? WindowState.Maximized : WindowState.Normal;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void PositionOnSecondaryMonitorIfAvailable()
    {
        var screens = System.Windows.Forms.Screen.AllScreens;
        var targetScreen = screens.FirstOrDefault(s => !s.Primary);
        if (targetScreen is null)
        {
            return;
        }

        var source = PresentationSource.FromVisual(this);
        if (source?.CompositionTarget is null)
        {
            return;
        }

        var transform = source.CompositionTarget.TransformFromDevice;
        var topLeft = transform.Transform(new System.Windows.Point(targetScreen.Bounds.Left, targetScreen.Bounds.Top));

        WindowState = WindowState.Normal;
        Left = topLeft.X;
        Top = topLeft.Y;
        WindowState = WindowState.Maximized;
    }

    private void SecondaryDisplayWindow_Closing(object? sender, CancelEventArgs e)
    {
        // Keep the window (and its position/state) alive for the rest of the session —
        // the GM toggles visibility via IWindowManager instead of recreating it each time.
        e.Cancel = true;
        Hide();
    }

    private record WindowPlacement(double Left, double Top, double Width, double Height, bool IsMaximized);
}
