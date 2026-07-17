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
        PdfViewer.InkAnnotationSettings.Thickness = 3;
        PdfViewer.InkAnnotationSettings.Opacity = 1f;

        PdfViewer.FreeTextAnnotationSettings.FontColor = Colors.Black;
        PdfViewer.FreeTextAnnotationSettings.FontSize = 14;
        PdfViewer.FreeTextAnnotationSettings.Background = Colors.Transparent;
        PdfViewer.FreeTextAnnotationSettings.BorderColor = Colors.Transparent;

        PdfViewer.RedactionSettings.UseOverlayText = true;
        PdfViewer.RedactionSettings.FontColor = Colors.Black;
        PdfViewer.RedactionSettings.FontSize = 14;
        PdfViewer.RedactionSettings.FillColor = Colors.White;
    }

    private enum EditMode
    {
        None,
        Ink,
        FreeText,
        Redact,
    }

    private void SetEditMode(EditMode mode)
    {
        ToggleInkButton.IsChecked = mode == EditMode.Ink;
        ToggleFreeTextButton.IsChecked = mode == EditMode.FreeText;
        ToggleRedactButton.IsChecked = mode == EditMode.Redact;

        PdfViewer.AnnotationMode = mode switch
        {
            EditMode.Ink => PdfDocumentView.PdfViewerAnnotationMode.Ink,
            EditMode.FreeText => PdfDocumentView.PdfViewerAnnotationMode.FreeText,
            _ => PdfDocumentView.PdfViewerAnnotationMode.None,
        };
        PdfViewer.PageRedactor.EnableRedactionMode = mode == EditMode.Redact;
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
            _loadedPdf = null;
            ResetEditMode();
            PdfViewer.Unload();
            return;
        }

        ResetEditMode();

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

    private void ToggleInkButton_Click(object sender, RoutedEventArgs e) =>
        SetEditMode(ToggleInkButton.IsChecked == true ? EditMode.Ink : EditMode.None);

    private void ToggleFreeTextButton_Click(object sender, RoutedEventArgs e) =>
        SetEditMode(ToggleFreeTextButton.IsChecked == true ? EditMode.FreeText : EditMode.None);

    private void ToggleRedactButton_Click(object sender, RoutedEventArgs e) =>
        SetEditMode(ToggleRedactButton.IsChecked == true ? EditMode.Redact : EditMode.None);

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

    private void InkColor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Background: SolidColorBrush brush })
        {
            PdfViewer.InkAnnotationSettings.InkColor = brush.Color;
        }
    }

    private void InkThickness_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: string tag } && float.TryParse(tag, out var thickness))
        {
            PdfViewer.InkAnnotationSettings.Thickness = thickness;
        }
    }

    private void TransparentCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        PdfViewer.InkAnnotationSettings.Opacity = TransparentCheckBox.IsChecked == true ? 0.35f : 1f;
    }

    private void FreeTextColor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Background: SolidColorBrush brush })
        {
            PdfViewer.FreeTextAnnotationSettings.FontColor = brush.Color;
        }
    }

    private void FreeTextSize_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Tag: string tag } && int.TryParse(tag, out var size))
        {
            PdfViewer.FreeTextAnnotationSettings.FontSize = size;
        }
    }

    private void RedactColor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton { Background: SolidColorBrush brush })
        {
            PdfViewer.RedactionSettings.FontColor = brush.Color;
        }
    }

    private void RedactOverlayTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        PdfViewer.RedactionSettings.OverlayText = RedactOverlayTextBox.Text;
    }

    private void ApplyRedactionButton_Click(object sender, RoutedEventArgs e)
    {
        PdfViewer.PageRedactor.ApplyRedaction();
    }

    private void ResetEditMode()
    {
        SetEditMode(EditMode.None);
    }
}
