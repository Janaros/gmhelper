using CommunityToolkit.Mvvm.ComponentModel;
using GMHelper.Core.Entities;
using GMHelper.Core.Enums;

namespace GMHelper.App.ViewModels;

/// <summary>
/// Editable row for the combat tracker. Any change to DisplayName/InitiativeText/
/// TrackedValueText/ConditionsText is picked up by CombatTrackerViewModel (which subscribes to
/// PropertyChanged on every row) and persisted immediately — no separate save step per row.
/// </summary>
public partial class CombatParticipantVm : ObservableObject
{
    public int Id { get; }
    public CombatParticipantSourceType SourceType { get; }
    public bool IsMonster => SourceType == CombatParticipantSourceType.MonsterInstance;

    [ObservableProperty]
    private string _displayName;

    [ObservableProperty]
    private string _initiativeText;

    [ObservableProperty]
    private string _trackedValueText;

    [ObservableProperty]
    private int? _maxTrackedValue;

    [ObservableProperty]
    private string _conditionsText;

    [ObservableProperty]
    private bool _isCurrentTurn;

    public CombatParticipantVm(CombatParticipant model)
    {
        Id = model.Id;
        SourceType = model.SourceType;
        _displayName = model.DisplayName;
        _initiativeText = model.Initiative?.ToString() ?? string.Empty;
        _trackedValueText = model.CurrentTrackedValue?.ToString() ?? string.Empty;
        _maxTrackedValue = model.MaxTrackedValue;
        _conditionsText = model.ConditionsText ?? string.Empty;
    }
}
