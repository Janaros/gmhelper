using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GMHelper.App.ViewModels;
using Microsoft.Win32;

namespace GMHelper.App.Views;

public partial class ImageLibraryView : UserControl
{
    private ImageLibraryViewModel? ViewModel => DataContext as ImageLibraryViewModel;

    public ImageLibraryView()
    {
        InitializeComponent();

        ImageList.SelectionChanged += ImageList_SelectionChanged;
    }

    private async void AddImageButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = "Bilder (*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp",
            Title = "Bild zur Kampagne hinzufügen",
        };

        if (dialog.ShowDialog() == true)
        {
            await ViewModel.AddImageAsync(dialog.FileName);
        }
    }

    private void ImageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel?.SelectedImage is not { } image)
        {
            PreviewImage.Source = null;
            return;
        }

        var path = ViewModel.GetAbsoluteFilePath(image);

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.UriSource = new Uri(path, UriKind.Absolute);
        bitmap.EndInit();
        bitmap.Freeze();

        PreviewImage.Source = bitmap;
    }
}
