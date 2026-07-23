using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using GMHelper.App.ViewModels;

namespace GMHelper.App.Views;

public partial class StatFieldEditor : UserControl
{
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(ObservableCollection<StatFieldEditorItem>),
        typeof(StatFieldEditor),
        new PropertyMetadata(null));

    public ObservableCollection<StatFieldEditorItem>? ItemsSource
    {
        get => (ObservableCollection<StatFieldEditorItem>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public StatFieldEditor()
    {
        InitializeComponent();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        ItemsSource?.Add(new StatFieldEditorItem());
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: StatFieldEditorItem item } && ItemsSource is not null && !item.IsLocked)
        {
            ItemsSource.Remove(item);
        }
    }

    private void MoveUpButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: StatFieldEditorItem item } || ItemsSource is null)
        {
            return;
        }

        var index = ItemsSource.IndexOf(item);
        if (index > 0)
        {
            ItemsSource.Move(index, index - 1);
        }
    }

    private void MoveDownButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: StatFieldEditorItem item } || ItemsSource is null)
        {
            return;
        }

        var index = ItemsSource.IndexOf(item);
        if (index >= 0 && index < ItemsSource.Count - 1)
        {
            ItemsSource.Move(index, index + 1);
        }
    }
}
