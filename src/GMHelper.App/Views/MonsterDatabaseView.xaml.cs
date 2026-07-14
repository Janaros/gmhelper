using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GMHelper.App.ViewModels;
using Microsoft.Win32;

namespace GMHelper.App.Views;

public partial class MonsterDatabaseView : UserControl
{
    private MonsterDatabaseViewModel? ViewModel => DataContext as MonsterDatabaseViewModel;

    public MonsterDatabaseView()
    {
        InitializeComponent();

        MonsterList.SelectionChanged += async (_, _) => await RefreshPortraitAsync();
    }

    private async void AssignImageButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = "Bilder (*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp",
            Title = "Bild für Monster zuweisen",
        };

        if (dialog.ShowDialog() == true)
        {
            await ViewModel.AssignImageAsync(dialog.FileName);
            await RefreshPortraitAsync();
        }
    }

    private async void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = "Monster-Import (*.json;*.csv)|*.json;*.csv",
            Title = "Monster importieren",
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var records = await ViewModel.ParseImportFileAsync(dialog.FileName);
        if (records.Count == 0)
        {
            MessageBox.Show("Die Datei enthält keine importierbaren Monster.", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var previewViewModel = new ImportPreviewViewModel(records);
        var previewWindow = new ImportPreviewWindow(previewViewModel)
        {
            Owner = Window.GetWindow(this),
        };

        if (previewWindow.ShowDialog() == true)
        {
            var baseDirectory = Path.GetDirectoryName(dialog.FileName) ?? string.Empty;
            await ViewModel.CommitImportAsync(records, baseDirectory, previewViewModel.ConflictStrategy);
        }
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "JSON-Datei (*.json)|*.json|CSV-Datei (*.csv)|*.csv",
            Title = "Monster exportieren",
            FileName = "Monster.json",
        };

        if (dialog.ShowDialog() == true)
        {
            await ViewModel.ExportAsync(dialog.FileName);
        }
    }

    private async Task RefreshPortraitAsync()
    {
        if (ViewModel is null)
        {
            PortraitImage.Source = null;
            return;
        }

        var path = await ViewModel.GetPortraitAbsolutePathAsync();
        if (path is null)
        {
            PortraitImage.Source = null;
            return;
        }

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.UriSource = new Uri(path, UriKind.Absolute);
        bitmap.EndInit();
        bitmap.Freeze();

        PortraitImage.Source = bitmap;
    }
}
