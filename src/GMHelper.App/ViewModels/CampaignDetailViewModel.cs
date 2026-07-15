using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using Microsoft.Extensions.Logging;

namespace GMHelper.App.ViewModels;

public partial class CampaignDetailViewModel : ObservableObject
{
    private readonly ICampaignExportService _campaignExportService;
    private readonly ILogger<CampaignDetailViewModel> _logger;

    public Campaign Campaign { get; }

    public PdfLibraryViewModel PdfLibrary { get; }
    public ImageLibraryViewModel ImageLibrary { get; }
    public RosterViewModel Roster { get; }
    public CombatTrackerViewModel CombatTracker { get; }
    public SessionNotesViewModel SessionNotes { get; }

    [ObservableProperty]
    private string? _statusMessage;

    public event EventHandler? BackRequested;

    public CampaignDetailViewModel(
        Campaign campaign,
        PdfLibraryViewModel pdfLibrary,
        ImageLibraryViewModel imageLibrary,
        RosterViewModel roster,
        CombatTrackerViewModel combatTracker,
        SessionNotesViewModel sessionNotes,
        ICampaignExportService campaignExportService,
        ILogger<CampaignDetailViewModel> logger)
    {
        Campaign = campaign;
        PdfLibrary = pdfLibrary;
        ImageLibrary = imageLibrary;
        Roster = roster;
        CombatTracker = combatTracker;
        SessionNotes = sessionNotes;
        _campaignExportService = campaignExportService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await PdfLibrary.InitializeAsync();
        await ImageLibrary.InitializeAsync();
        await Roster.InitializeAsync();
        await CombatTracker.InitializeAsync();
        await SessionNotes.InitializeAsync();
    }

    public async Task ExportAsync(string destinationZipFilePath)
    {
        try
        {
            await _campaignExportService.ExportCampaignAsync(Campaign.Id, destinationZipFilePath);
            StatusMessage = $"Backup gespeichert: {destinationZipFilePath}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export campaign {CampaignId}", Campaign.Id);
            StatusMessage = $"Fehler beim Export: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);
}
