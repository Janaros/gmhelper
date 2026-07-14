using GMHelper.App.Views;

namespace GMHelper.App.Windows;

public class WindowManager : IWindowManager
{
    private readonly SecondaryDisplayWindow _secondaryDisplayWindow;

    public WindowManager(SecondaryDisplayWindow secondaryDisplayWindow)
    {
        _secondaryDisplayWindow = secondaryDisplayWindow;
    }

    public void ShowSecondaryDisplay()
    {
        if (!_secondaryDisplayWindow.IsVisible)
        {
            _secondaryDisplayWindow.Show();
        }

        _secondaryDisplayWindow.Activate();
    }
}
