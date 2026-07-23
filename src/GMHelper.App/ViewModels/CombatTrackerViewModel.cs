using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using Microsoft.Extensions.Logging;

namespace GMHelper.App.ViewModels;

public partial class CombatTrackerViewModel : ObservableObject
{
    private readonly Campaign _campaign;
    private readonly ICombatTrackerService _combatTrackerService;
    private readonly IMonsterService _monsterService;
    private readonly ILogger<CombatTrackerViewModel> _logger;

    private CombatEncounter? _encounter;

    public ObservableCollection<CombatParticipantVm> Participants { get; } = new();
    public ObservableCollection<Monster> AvailableMonsters { get; } = new();

    [ObservableProperty]
    private Monster? _selectedMonsterToAdd;

    [ObservableProperty]
    private bool _hasActiveEncounter;

    [ObservableProperty]
    private bool _hasStarted;

    [ObservableProperty]
    private int _currentRound;

    [ObservableProperty]
    private string? _statusMessage;

    /// <summary>Encounter exists but hasn't been started yet (still adding participants/initiative).</summary>
    public bool IsPreparing => HasActiveEncounter && !HasStarted;

    partial void OnHasActiveEncounterChanged(bool value) => OnPropertyChanged(nameof(IsPreparing));
    partial void OnHasStartedChanged(bool value) => OnPropertyChanged(nameof(IsPreparing));

    public CombatTrackerViewModel(
        Campaign campaign,
        ICombatTrackerService combatTrackerService,
        IMonsterService monsterService,
        ILogger<CombatTrackerViewModel> logger)
    {
        _campaign = campaign;
        _combatTrackerService = combatTrackerService;
        _monsterService = monsterService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        var monsters = await _monsterService.GetMonstersAsync();
        AvailableMonsters.Clear();
        foreach (var monster in monsters)
        {
            AvailableMonsters.Add(monster);
        }
        SelectedMonsterToAdd = AvailableMonsters.FirstOrDefault();

        await LoadEncounterAsync();
    }

    [RelayCommand]
    private async Task PrepareEncounterAsync()
    {
        try
        {
            _encounter = await _combatTrackerService.PrepareEncounterAsync(_campaign.Id);
            StatusMessage = null;
            await RefreshFromEncounterAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to prepare combat encounter for campaign {CampaignId}", _campaign.Id);
            StatusMessage = $"Fehler: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task AddMonsterAsync()
    {
        if (_encounter is null || SelectedMonsterToAdd is null)
        {
            return;
        }

        try
        {
            await _combatTrackerService.AddMonsterParticipantAsync(_encounter.Id, SelectedMonsterToAdd.Id);
            await ReloadParticipantsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add monster {MonsterId} to encounter {EncounterId}", SelectedMonsterToAdd.Id, _encounter.Id);
            StatusMessage = $"Fehler: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task StartEncounterAsync()
    {
        if (_encounter is null)
        {
            return;
        }

        try
        {
            await _combatTrackerService.StartEncounterAsync(_encounter.Id);
            await LoadEncounterAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start encounter {EncounterId}", _encounter.Id);
            StatusMessage = $"Fehler: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task NextTurnAsync()
    {
        if (_encounter is null)
        {
            return;
        }

        try
        {
            await _combatTrackerService.AdvanceTurnAsync(_encounter.Id);
            await LoadEncounterAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to advance turn for encounter {EncounterId}", _encounter.Id);
            StatusMessage = $"Fehler: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task EndEncounterAsync()
    {
        if (_encounter is null)
        {
            return;
        }

        try
        {
            await _combatTrackerService.EndEncounterAsync(_encounter.Id);
            _encounter = null;
            HasActiveEncounter = false;
            HasStarted = false;
            CurrentRound = 0;
            Participants.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to end encounter");
            StatusMessage = $"Fehler: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshOrderAsync() => await ReloadParticipantsAsync();

    public void RollInitiative(CombatParticipantVm vm) => vm.InitiativeText = Random.Shared.Next(1, 21).ToString();

    public void AdjustTrackedValue(CombatParticipantVm vm, int delta)
    {
        var current = int.TryParse(vm.TrackedValueText, out var value) ? value : 0;
        vm.TrackedValueText = (current + delta).ToString();
    }

    public async Task RemoveParticipantAsync(CombatParticipantVm vm)
    {
        try
        {
            await _combatTrackerService.RemoveParticipantAsync(vm.Id);
            await ReloadParticipantsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove participant {ParticipantId}", vm.Id);
            StatusMessage = $"Fehler: {ex.Message}";
        }
    }

    private async Task LoadEncounterAsync()
    {
        _encounter = await _combatTrackerService.GetActiveEncounterAsync(_campaign.Id);
        await RefreshFromEncounterAsync();
    }

    private async Task RefreshFromEncounterAsync()
    {
        HasActiveEncounter = _encounter is not null;
        HasStarted = _encounter is { CurrentRound: > 0 };
        CurrentRound = _encounter?.CurrentRound ?? 0;

        await ReloadParticipantsAsync();
    }

    private async Task ReloadParticipantsAsync()
    {
        foreach (var existing in Participants)
        {
            existing.PropertyChanged -= OnParticipantPropertyChanged;
        }
        Participants.Clear();

        if (_encounter is null)
        {
            return;
        }

        var participants = await _combatTrackerService.GetParticipantsAsync(_encounter.Id);
        foreach (var participant in participants)
        {
            var vm = new CombatParticipantVm(participant)
            {
                IsCurrentTurn = participant.Id == _encounter.CurrentTurnParticipantId,
            };
            vm.PropertyChanged += OnParticipantPropertyChanged;
            Participants.Add(vm);
        }
    }

    private void OnParticipantPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not CombatParticipantVm vm)
        {
            return;
        }

        if (e.PropertyName is nameof(CombatParticipantVm.DisplayName)
            or nameof(CombatParticipantVm.InitiativeText)
            or nameof(CombatParticipantVm.TrackedValueText)
            or nameof(CombatParticipantVm.ConditionsText))
        {
            _ = SaveParticipantAsync(vm);
        }
    }

    private async Task SaveParticipantAsync(CombatParticipantVm vm)
    {
        var initiative = int.TryParse(vm.InitiativeText, out var initiativeValue) ? initiativeValue : (int?)null;
        var trackedValue = int.TryParse(vm.TrackedValueText, out var trackedValueValue) ? trackedValueValue : (int?)null;

        try
        {
            await _combatTrackerService.UpdateParticipantAsync(
                vm.Id,
                vm.DisplayName,
                initiative,
                trackedValue,
                string.IsNullOrWhiteSpace(vm.ConditionsText) ? null : vm.ConditionsText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save participant {ParticipantId}", vm.Id);
            StatusMessage = $"Fehler beim Speichern: {ex.Message}";
        }
    }
}
