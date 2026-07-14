using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GMHelper.App.ViewModels;
using GMHelper.Core.Entities;
using Microsoft.Win32;
using Syncfusion.Windows.PdfViewer;

namespace GMHelper.App.Views;

public partial class PdfLibraryView : UserControl
{
    private PdfDocument? _loadedPdf;

    private PdfLibraryViewModel? ViewModel => DataContext as PdfLibraryViewModel;

    public PdfLibraryView()
    {
        InitializeComponent();

        PdfList.SelectionChanged += PdfList_SelectionChanged;
        PdfViewer.DocumentLoaded += PdfViewer_DocumentLoaded;
        PdfViewer.CurrentPageChanged += PdfViewer_CurrentPageChanged;

        PdfViewer.InkAnnotationSettings.InkColor = Colors.Black;
        PdfViewer.InkAnnotationSettings.Thickness = 2;
        PdfViewer.InkAnnotationSettings.Opacity = 1f;
    }

    private async void AddPdfButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = "PDF-Dateien (*.pdf)|*.pdf",
            Title = "PDF zur Kampagne hinzufügen",
        };

        if (dialog.ShowDialog() == true)
        {
            await ViewModel.AddPdfAsync(dialog.FileName);
        }
    }

    private void PdfList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel?.SelectedPdf is not { } pdf)
        {
            return;
        }

        ResetInkMode();

        _loadedPdf = pdf;
        PdfViewer.Load(ViewModel.GetAbsoluteFilePath(pdf));
    }

    private void PdfViewer_DocumentLoaded(object? sender, EventArgs e)
    {
        if (_loadedPdf is null)
        {
            return;
        }

        var targetPage = Math.Min(Math.Max(_loadedPdf.LastViewedPage, 1), Math.Max(PdfViewer.PageCount, 1));
        PdfViewer.GotoPage(targetPage);
    }

    private async void PdfViewer_CurrentPageChanged(object? sender, EventArgs e)
    {
        if (ViewModel is null || _loadedPdf is null)
        {
            return;
        }

        await ViewModel.SaveLastViewedPageAsync(PdfViewer.CurrentPage);
    }

    private void ToggleInkButton_Click(object sender, RoutedEventArgs e)
    {
        PdfViewer.AnnotationMode = ToggleInkButton.IsChecked == true
            ? PdfDocumentView.PdfViewerAnnotationMode.Ink
            : PdfDocumentView.PdfViewerAnnotationMode.None;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel is null || _loadedPdf is null)
        {
            return;
        }

        var backupSucceeded = await ViewModel.PrepareSaveAsync();
        if (!backupSucceeded)
        {
            return;
        }

        try
        {
            PdfViewer.Save(ViewModel.GetAbsoluteFilePath(_loadedPdf));
            ViewModel.StatusMessage = "Gespeichert.";
        }
        catch (Exception ex)
        {
            ViewModel.StatusMessage = $"Fehler beim Speichern: {ex.Message}";
        }
    }

    private void ResetInkMode()
    {
        ToggleInkButton.IsChecked = false;
        PdfViewer.AnnotationMode = PdfDocumentView.PdfViewerAnnotationMode.None;
    }
}
