using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using Microsoft.Extensions.Logging;

namespace GMHelper.App.ViewModels;

public partial class SessionNotesViewModel : ObservableObject
{
    private readonly Campaign _campaign;
    private readonly ISessionNotesService _sessionNotesService;
    private readonly ILogger<SessionNotesViewModel> _logger;

    public ObservableCollection<SessionNote> Notes { get; } = new();

    [ObservableProperty]
    private SessionNote? _selectedNote;

    [ObservableProperty]
    private string _editTitle = string.Empty;

    [ObservableProperty]
    private DateTime? _editSessionDate = DateTime.Today;

    [ObservableProperty]
    private string _editMarkdownContent = string.Empty;

    [ObservableProperty]
    private string? _statusMessage;

    public SessionNotesViewModel(Campaign campaign, ISessionNotesService sessionNotesService, ILogger<SessionNotesViewModel> logger)
    {
        _campaign = campaign;
        _sessionNotesService = sessionNotesService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await ReloadNotesAsync();
    }

    [RelayCommand]
    private void NewNote()
    {
        SelectedNote = null;
    }

    [RelayCommand]
    private async Task SaveSelectedNoteAsync()
    {
        if (string.IsNullOrWhiteSpace(EditTitle))
        {
            StatusMessage = "Titel darf nicht leer sein.";
            return;
        }

        var sessionDate = EditSessionDate ?? DateTime.Today;

        try
        {
            var selectedId = SelectedNote?.Id;
            if (selectedId is null)
            {
                var created = await _sessionNotesService.CreateNoteAsync(_campaign.Id, EditTitle.Trim(), sessionDate, EditMarkdownContent);
                selectedId = created.Id;
            }
            else
            {
                await _sessionNotesService.UpdateNoteAsync(selectedId.Value, EditTitle.Trim(), sessionDate, EditMarkdownContent);
            }

            StatusMessage = "Gespeichert.";
            await ReloadNotesAsync();
            SelectedNote = Notes.FirstOrDefault(n => n.Id == selectedId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save session note for campaign {CampaignId}", _campaign.Id);
            StatusMessage = $"Fehler beim Speichern: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteSelectedNoteAsync()
    {
        if (SelectedNote is null)
        {
            return;
        }

        try
        {
            await _sessionNotesService.DeleteNoteAsync(SelectedNote.Id);
            SelectedNote = null;
            StatusMessage = null;
            await ReloadNotesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete session note {NoteId}", SelectedNote?.Id);
            StatusMessage = $"Fehler beim Löschen: {ex.Message}";
        }
    }

    partial void OnSelectedNoteChanged(SessionNote? value)
    {
        if (value is null)
        {
            EditTitle = string.Empty;
            EditSessionDate = DateTime.Today;
            EditMarkdownContent = string.Empty;
            return;
        }

        EditTitle = value.Title;
        EditSessionDate = value.SessionDate;
        EditMarkdownContent = value.MarkdownContent;
    }

    private async Task ReloadNotesAsync()
    {
        var notes = await _sessionNotesService.GetNotesForCampaignAsync(_campaign.Id);

        Notes.Clear();
        foreach (var note in notes)
        {
            Notes.Add(note);
        }
    }
}
