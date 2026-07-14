using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using GMHelper.App.ViewModels;

namespace GMHelper.App.Views;

public partial class SecondaryDisplayWindow : Window
{
    public SecondaryDisplayWindow(SecondaryDisplayViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;

        SourceInitialized += SecondaryDisplayWindow_SourceInitialized;
        Closing += SecondaryDisplayWindow_Closing;
    }

    private void SecondaryDisplayWindow_SourceInitialized(object? sender, EventArgs e)
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
}
