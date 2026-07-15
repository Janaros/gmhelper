using System.Windows;
using System.Windows.Controls;
using GMHelper.App.ViewModels;

namespace GMHelper.App.Views;

public partial class CombatTrackerView : UserControl
{
    private CombatTrackerViewModel? ViewModel => DataContext as CombatTrackerViewModel;

    public CombatTrackerView()
    {
        InitializeComponent();
    }

    private void RollInitiativeButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: CombatParticipantVm vm })
        {
            ViewModel?.RollInitiative(vm);
        }
    }

    private void IncrementHpButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: CombatParticipantVm vm })
        {
            ViewModel?.AdjustTrackedValue(vm, 1);
        }
    }

    private void DecrementHpButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: CombatParticipantVm vm })
        {
            ViewModel?.AdjustTrackedValue(vm, -1);
        }
    }

    private async void RemoveParticipantButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: CombatParticipantVm vm } && ViewModel is not null)
        {
            await ViewModel.RemoveParticipantAsync(vm);
        }
    }
}
