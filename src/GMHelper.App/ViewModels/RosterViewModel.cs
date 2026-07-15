using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Core.Enums;
using Microsoft.Extensions.Logging;

namespace GMHelper.App.ViewModels;

public partial class RosterViewModel : ObservableObject
{
    private readonly Campaign _campaign;
    private readonly IPlayerService _playerService;
    private readonly IStatFieldService _statFieldService;
    private readonly ILogger<RosterViewModel> _logger;

    public ObservableCollection<Player> Players { get; } = new();
    public ObservableCollection<StatFieldEditorItem> StatFields { get; } = new();

    [ObservableProperty]
    private Player? _selectedPlayer;

    [ObservableProperty]
    private string _editCharacterName = string.Empty;

    [ObservableProperty]
    private string _editPlayerName = string.Empty;

    [ObservableProperty]
    private string _editInitiativeText = string.Empty;

    [ObservableProperty]
    private string _editNotes = string.Empty;

    [ObservableProperty]
    private string? _statusMessage;

    public RosterViewModel(Campaign campaign, IPlayerService playerService, IStatFieldService statFieldService, ILogger<RosterViewModel> logger)
    {
        _campaign = campaign;
        _playerService = playerService;
        _statFieldService = statFieldService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await ReloadPlayersAsync();
    }

    [RelayCommand]
    private void NewCharacter() => SelectedPlayer = null;

    [RelayCommand]
    private async Task DeleteSelectedPlayerAsync()
    {
        if (SelectedPlayer is null)
        {
            return;
        }

        try
        {
            await _playerService.DeletePlayerAsync(SelectedPlayer.Id);
            SelectedPlayer = null;
            StatusMessage = null;
            await ReloadPlayersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete player {PlayerId}", SelectedPlayer?.Id);
            StatusMessage = $"Fehler beim Löschen: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveSelectedPlayerAsync()
    {
        if (string.IsNullOrWhiteSpace(EditCharacterName))
        {
            StatusMessage = "Charaktername darf nicht leer sein.";
            return;
        }

        var initiative = int.TryParse(EditInitiativeText, out var parsedInitiative) ? parsedInitiative : (int?)null;

        try
        {
            var selectedId = SelectedPlayer?.Id
                ?? (await _playerService.CreatePlayerAsync(_campaign.Id, EditCharacterName.Trim())).Id;

            await _playerService.UpdatePlayerAsync(
                selectedId,
                EditCharacterName.Trim(),
                string.IsNullOrWhiteSpace(EditPlayerName) ? null : EditPlayerName.Trim(),
                initiative,
                string.IsNullOrWhiteSpace(EditNotes) ? null : EditNotes.Trim());

            var fields = StatFields
                .Where(f => !string.IsNullOrWhiteSpace(f.Name))
                .Select(f => (f.Name, f.Value))
                .ToList();
            await _statFieldService.ReplaceStatFieldsAsync(StatFieldOwnerType.Player, selectedId, fields);

            StatusMessage = "Gespeichert.";
            await ReloadPlayersAsync();
            SelectedPlayer = Players.FirstOrDefault(p => p.Id == selectedId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save player {PlayerId} in campaign {CampaignId}", SelectedPlayer?.Id, _campaign.Id);
            StatusMessage = $"Fehler beim Speichern: {ex.Message}";
        }
    }

    public async Task SetPlayerActiveAsync(Player player, bool isActive)
    {
        try
        {
            await _playerService.SetActiveAsync(player.Id, isActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set player {PlayerId} active={IsActive}", player.Id, isActive);
            StatusMessage = $"Fehler: {ex.Message}";
        }
    }

    partial void OnSelectedPlayerChanged(Player? value)
    {
        _ = LoadSelectedPlayerAsync(value);
    }

    private async Task LoadSelectedPlayerAsync(Player? player)
    {
        StatFields.Clear();

        if (player is null)
        {
            EditCharacterName = string.Empty;
            EditPlayerName = string.Empty;
            EditInitiativeText = string.Empty;
            EditNotes = string.Empty;
            return;
        }

        EditCharacterName = player.CharacterName;
        EditPlayerName = player.PlayerName ?? string.Empty;
        EditInitiativeText = player.Initiative?.ToString() ?? string.Empty;
        EditNotes = player.Notes ?? string.Empty;

        var fields = await _statFieldService.GetStatFieldsAsync(StatFieldOwnerType.Player, player.Id);
        foreach (var field in fields)
        {
            StatFields.Add(new StatFieldEditorItem { Name = field.Name, Value = field.Value });
        }
    }

    private async Task ReloadPlayersAsync()
    {
        var players = await _playerService.GetPlayersForCampaignAsync(_campaign.Id);

        Players.Clear();
        foreach (var player in players)
        {
            Players.Add(player);
        }
    }
}
