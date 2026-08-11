using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GMHelper.App.ViewModels;

namespace GMHelper.App.Views;

public partial class CombatTrackerView : UserControl
{
    private CombatTrackerViewModel? ViewModel => DataContext as CombatTrackerViewModel;

    public CombatTrackerView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// SfDataGrid claims the first left click inside a row for its own cell selection, so a
    /// TextBox in a GridTemplateColumn would only receive keyboard focus on the second click.
    /// Focus it right away — placing the caret where the user actually clicked — and mark the
    /// click handled so the grid's selection handling cannot pull the focus back out. Once the
    /// box has focus, later clicks fall through untouched, so selecting/dragging text still works.
    /// </summary>
    private void CellTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not TextBox textBox || textBox.IsKeyboardFocusWithin)
        {
            return;
        }

        textBox.Focus();
        textBox.CaretIndex = textBox.GetCharacterIndexFromPoint(e.GetPosition(textBox), snapToText: true);
        e.Handled = true;
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
