using System.Windows;
using GMHelper.App.ViewModels;

namespace GMHelper.App.Views;

public partial class ImportPreviewWindow : Window
{
    public ImportPreviewWindow(ImportPreviewViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
