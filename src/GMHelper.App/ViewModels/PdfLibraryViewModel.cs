using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using Microsoft.Extensions.Logging;

namespace GMHelper.App.ViewModels;

public partial class PdfLibraryViewModel : ObservableObject
{
    private readonly Campaign _campaign;
    private readonly IPdfLibraryService _pdfLibraryService;
    private readonly IPdfJumpMarkerService _pdfJumpMarkerService;
    private readonly IPdfTocGeneratorService _pdfTocGeneratorService;
    private readonly ILogger<PdfLibraryViewModel> _logger;

    public ObservableCollection<PdfDocument> Pdfs { get; } = new();
    public ObservableCollection<PdfJumpMarker> JumpMarkers { get; } = new();

    [ObservableProperty]
    private PdfDocument? _selectedPdf;

    [ObservableProperty]
    private string? _statusMessage;

    public PdfLibraryViewModel(
        Campaign campaign,
        IPdfLibraryService pdfLibraryService,
        IPdfJumpMarkerService pdfJumpMarkerService,
        IPdfTocGeneratorService pdfTocGeneratorService,
        ILogger<PdfLibraryViewModel> logger)
    {
        _campaign = campaign;
        _pdfLibraryService = pdfLibraryService;
        _pdfJumpMarkerService = pdfJumpMarkerService;
        _pdfTocGeneratorService = pdfTocGeneratorService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await ReloadAsync();
    }

    partial void OnSelectedPdfChanged(PdfDocument? value)
    {
        _ = ReloadJumpMarkersAsync(value);
    }

    private async Task ReloadJumpMarkersAsync(PdfDocument? pdf)
    {
        JumpMarkers.Clear();

        if (pdf is null)
        {
            return;
        }

        var markers = await _pdfJumpMarkerService.GetJumpMarkersAsync(pdf.Id);
        foreach (var marker in markers)
        {
            JumpMarkers.Add(marker);
        }
    }

    public async Task AddJumpMarkerAsync(string title, int pageNumber)
    {
        if (SelectedPdf is null || string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        try
        {
            await _pdfJumpMarkerService.AddJumpMarkerAsync(SelectedPdf.Id, title.Trim(), pageNumber);
            await ReloadJumpMarkersAsync(SelectedPdf);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add jump marker for PDF {PdfDocumentId}", SelectedPdf.Id);
            StatusMessage = $"Fehler beim Anlegen der Sprungmarke: {ex.Message}";
        }
    }

    public async Task DeleteJumpMarkerAsync(PdfJumpMarker marker)
    {
        try
        {
            await _pdfJumpMarkerService.DeleteJumpMarkerAsync(marker.Id);
            await ReloadJumpMarkersAsync(SelectedPdf);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete jump marker {JumpMarkerId}", marker.Id);
            StatusMessage = $"Fehler beim Löschen der Sprungmarke: {ex.Message}";
        }
    }

    /// <summary>Scans the currently selected PDF for numbered headings and writes them as a real
    /// PDF outline (see <see cref="IPdfTocGeneratorService"/>), so the viewer's native bookmark
    /// panel shows something useful. Backs up first via <see cref="PrepareSaveAsync"/> since this
    /// mutates the file in place, same as the manual "Speichern" flow.</summary>
    public async Task GenerateTocAsync()
    {
        if (SelectedPdf is null)
        {
            return;
        }

        if (!await PrepareSaveAsync())
        {
            return;
        }

        try
        {
            var count = await _pdfTocGeneratorService.GenerateOutlineAsync(_pdfLibraryService.GetAbsoluteFilePath(SelectedPdf));
            StatusMessage = count == 0
                ? "Keine nummerierten Überschriften gefunden."
                : $"{count} Lesezeichen erzeugt.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF outline for {PdfDocumentId}", SelectedPdf.Id);
            StatusMessage = $"Fehler beim Erzeugen des Inhaltsverzeichnisses: {ex.Message}";
        }
    }

    public async Task AddPdfAsync(string sourceFilePath)
    {
        try
        {
            var added = await _pdfLibraryService.AddPdfToCampaignAsync(_campaign.Id, sourceFilePath);
            StatusMessage = null;
            await ReloadAsync();
            SelectedPdf = Pdfs.FirstOrDefault(p => p.Id == added.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add PDF {SourceFilePath} to campaign {CampaignId}", sourceFilePath, _campaign.Id);
            StatusMessage = $"Fehler beim Hinzufügen: {ex.Message}";
        }
    }

    public async Task SaveLastViewedPageAsync(int pageNumber)
    {
        if (SelectedPdf is null || pageNumber <= 0)
        {
            return;
        }

        try
        {
            await _pdfLibraryService.UpdateLastViewedPageAsync(SelectedPdf.Id, pageNumber);
            SelectedPdf.LastViewedPage = pageNumber;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist last viewed page for PDF {PdfDocumentId}", SelectedPdf.Id);
        }
    }

    public string GetAbsoluteFilePath(PdfDocument pdfDocument) => _pdfLibraryService.GetAbsoluteFilePath(pdfDocument);

    [RelayCommand]
    private async Task DeleteSelectedPdfAsync()
    {
        if (SelectedPdf is null)
        {
            return;
        }

        try
        {
            await _pdfLibraryService.DeletePdfAsync(SelectedPdf.Id);
            SelectedPdf = null;
            StatusMessage = null;
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete PDF {PdfDocumentId}", SelectedPdf?.Id);
            StatusMessage = $"Fehler beim Entfernen: {ex.Message}";
        }
    }

    /// <summary>
    /// Backs up the current file before the viewer control overwrites it in place.
    /// Returns false (and sets <see cref="StatusMessage"/>) if the backup failed, so the
    /// caller can skip the actual save rather than risk losing the only copy.
    /// </summary>
    public async Task<bool> PrepareSaveAsync()
    {
        if (SelectedPdf is null)
        {
            return false;
        }

        try
        {
            await _pdfLibraryService.CreateBackupAsync(SelectedPdf);
            StatusMessage = null;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create backup before saving PDF {PdfDocumentId}", SelectedPdf.Id);
            StatusMessage = $"Fehler beim Backup, Speichern abgebrochen: {ex.Message}";
            return false;
        }
    }

    private async Task ReloadAsync()
    {
        var pdfs = await _pdfLibraryService.GetPdfsForCampaignAsync(_campaign.Id);

        Pdfs.Clear();
        foreach (var pdf in pdfs)
        {
            Pdfs.Add(pdf);
        }
    }
}
