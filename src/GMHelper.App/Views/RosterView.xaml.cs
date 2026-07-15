using System.Windows;
using System.Windows.Controls;
using GMHelper.App.ViewModels;
using GMHelper.Core.Entities;

namespace GMHelper.App.Views;

public partial class RosterView : UserControl
{
    private RosterViewModel? ViewModel => DataContext as RosterViewModel;

    public RosterView()
    {
        InitializeComponent();
    }

    private async void ActiveCheckBox_Click(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox { DataContext: Player player } checkBox && ViewModel is not null)
        {
            await ViewModel.SetPlayerActiveAsync(player, checkBox.IsChecked == true);
        }
    }
}
