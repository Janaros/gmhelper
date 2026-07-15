using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMHelper.Core.Entities;

namespace GMHelper.App.ViewModels;

public partial class CampaignDetailViewModel : ObservableObject
{
    public Campaign Campaign { get; }

    public PdfLibraryViewModel PdfLibrary { get; }
    public ImageLibraryViewModel ImageLibrary { get; }
    public RosterViewModel Roster { get; }
    public CombatTrackerViewModel CombatTracker { get; }
    public SessionNotesViewModel SessionNotes { get; }

    public event EventHandler? BackRequested;

    public CampaignDetailViewModel(
        Campaign campaign,
        PdfLibraryViewModel pdfLibrary,
        ImageLibraryViewModel imageLibrary,
        RosterViewModel roster,
        CombatTrackerViewModel combatTracker,
        SessionNotesViewModel sessionNotes)
    {
        Campaign = campaign;
        PdfLibrary = pdfLibrary;
        ImageLibrary = imageLibrary;
        Roster = roster;
        CombatTracker = combatTracker;
        SessionNotes = sessionNotes;
    }

    public async Task InitializeAsync()
    {
        await PdfLibrary.InitializeAsync();
        await ImageLibrary.InitializeAsync();
        await Roster.InitializeAsync();
        await CombatTracker.InitializeAsync();
        await SessionNotes.InitializeAsync();
    }

    [RelayCommand]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);
}
