using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using Microsoft.Extensions.Logging;

namespace GMHelper.App.ViewModels;

public partial class PdfLibraryViewModel : ObservableObject
{
    private readonly Campaign _campaign;
    private readonly IPdfLibraryService _pdfLibraryService;
    private readonly ILogger<PdfLibraryViewModel> _logger;

    public ObservableCollection<PdfDocument> Pdfs { get; } = new();

    [ObservableProperty]
    private PdfDocument? _selectedPdf;

    [ObservableProperty]
    private string? _statusMessage;

    public PdfLibraryViewModel(Campaign campaign, IPdfLibraryService pdfLibraryService, ILogger<PdfLibraryViewModel> logger)
    {
        _campaign = campaign;
        _pdfLibraryService = pdfLibraryService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await ReloadAsync();
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
