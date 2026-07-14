using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMHelper.Core.Entities;

namespace GMHelper.App.ViewModels;

public partial class CampaignDetailViewModel : ObservableObject
{
    public Campaign Campaign { get; }

    public ObservableCollection<CampaignDetailTab> Tabs { get; }

    public event EventHandler? BackRequested;

    public CampaignDetailViewModel(Campaign campaign)
    {
        Campaign = campaign;
        Tabs = new ObservableCollection<CampaignDetailTab>
        {
            new("PDFs", "Kommt in Phase 2 (PDF-Bibliothek)."),
            new("Bilder", "Kommt in Phase 4 (Bild-Bibliothek + Zweitbildschirm)."),
            new("Spieler", "Kommt in Phase 5 (Roster mit flexiblen Stats)."),
            new("Kampf", "Kommt in Phase 8 (Kampf-Tracker)."),
            new("Notizen", "Kommt in Phase 9 (Markdown-Session-Notizen)."),
        };
    }

    [RelayCommand]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);
}
