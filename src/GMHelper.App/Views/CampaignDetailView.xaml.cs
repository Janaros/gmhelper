using System.Windows;
using System.Windows.Controls;
using GMHelper.App.ViewModels;
using Microsoft.Win32;

namespace GMHelper.App.Views;

public partial class CampaignDetailView : UserControl
{
    private CampaignDetailViewModel? ViewModel => DataContext as CampaignDetailViewModel;

    public CampaignDetailView()
    {
        InitializeComponent();
    }

    private async void ExportBackupButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "Zip-Archiv (*.zip)|*.zip",
            Title = "Kampagnen-Backup exportieren",
            FileName = $"{ViewModel.Campaign.Name}-Backup.zip",
        };

        if (dialog.ShowDialog() == true)
        {
            await ViewModel.ExportAsync(dialog.FileName);
        }
    }
}
