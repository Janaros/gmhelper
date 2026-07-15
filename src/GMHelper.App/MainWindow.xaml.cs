using System.Windows;
using System.Windows.Input;
using GMHelper.App.ViewModels;

namespace GMHelper.App;

public partial class MainWindow : Window
{
    private readonly ShellViewModel _viewModel;
    private readonly SecondaryDisplayViewModel _secondaryDisplayViewModel;

    public MainWindow(ShellViewModel viewModel, SecondaryDisplayViewModel secondaryDisplayViewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _secondaryDisplayViewModel = secondaryDisplayViewModel;
        DataContext = _viewModel;

        Loaded += async (_, _) => await _viewModel.InitializeAsync();
        PreviewKeyDown += MainWindow_PreviewKeyDown;
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.F12:
                _secondaryDisplayViewModel.ToggleBlackout();
                e.Handled = true;
                break;

            case Key.F9:
                if (_viewModel.CurrentViewModel is CampaignDetailViewModel detail)
                {
                    detail.CombatTracker.NextTurnCommand.Execute(null);
                }
                e.Handled = true;
                break;
        }
    }
}
