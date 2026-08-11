using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Core.Enums;
using GMHelper.Core.Models;

namespace GMHelper.App.ViewModels;

/// <summary>
/// Der Sammelwurf-Teil der Kräuterkunde-Ansicht. Eigenes ViewModel, weil das Auswürfeln eine
/// andere Verantwortung ist als das Pflegen der Gebietstabellen — es liest die Tabelle nur,
/// schreibt nie.
/// </summary>
public partial class HerbalismHarvestViewModel : ObservableObject
{
    private readonly IHerbalismHarvestService _harvestService;

    private HerbalismRegion? _region;
    private IReadOnlyList<HerbalismIngredient> _ingredients = [];
    private IngredientKind? _kindFilter;

    /// <summary>Weisheit(Überleben)-Modifikator, als Text, damit "+3" und "-1" tippbar sind.</summary>
    [ObservableProperty]
    private string _skillModifierText = "+0";

    /// <summary>Übung mit dem Kräuterkundeset — der Wurf erfolgt dann mit Vorteil.</summary>
    [ObservableProperty]
    private bool _useHerbalismKit;

    [ObservableProperty]
    private string? _rollSummary;

    [ObservableProperty]
    private string? _resultSummary;

    [ObservableProperty]
    private bool _hasResult;

    public ObservableCollection<HarvestFindVm> Finds { get; } = new();

    public HerbalismHarvestViewModel(IHerbalismHarvestService harvestService)
    {
        _harvestService = harvestService;
    }

    /// <summary>Übernimmt Gebiet, Fundtabelle und aktiven Filter aus der Elternansicht und
    /// verwirft ein Ergebnis, das sich auf einen anderen Stand bezogen hat.</summary>
    public void SetContext(HerbalismRegion? region, IReadOnlyList<HerbalismIngredient> ingredients, IngredientKind? kindFilter)
    {
        _region = region;
        _ingredients = ingredients;
        _kindFilter = kindFilter;

        Clear();
        HarvestCommand.NotifyCanExecuteChanged();
    }

    public bool CanHarvest => _region is not null;

    [RelayCommand(CanExecute = nameof(CanHarvest))]
    private void Harvest()
    {
        if (_region is null)
        {
            return;
        }

        var attempt = new HarvestAttempt(_region, _ingredients, ParseSkillModifier(SkillModifierText), UseHerbalismKit, _kindFilter);
        var outcome = _harvestService.Resolve(attempt);

        RollSummary = BuildRollSummary(outcome);

        Finds.Clear();
        foreach (var find in outcome.Finds)
        {
            Finds.Add(new HarvestFindVm(find));
        }

        ResultSummary = BuildResultSummary(outcome);
        HasResult = true;
    }

    [RelayCommand]
    private void Clear()
    {
        Finds.Clear();
        RollSummary = null;
        ResultSummary = null;
        HasResult = false;
    }

    private static string BuildRollSummary(HarvestOutcome outcome)
    {
        var modifier = outcome.SkillModifier >= 0
            ? $"+{outcome.SkillModifier}"
            : outcome.SkillModifier.ToString(CultureInfo.InvariantCulture);

        var dice = outcome.DiscardedDiceRoll is { } discarded
            ? $"W20 mit Vorteil: {outcome.DiceRoll} (verworfen: {discarded})"
            : $"W20: {outcome.DiceRoll}";

        return $"{dice} {modifier} = {outcome.Total} gegen SG {outcome.DifficultyClass}";
    }

    private static string BuildResultSummary(HarvestOutcome outcome)
    {
        if (!outcome.IsSuccess)
        {
            return $"Misslungen ({-outcome.Margin} unter dem SG) — nichts Brauchbares gefunden.";
        }

        if (outcome.Finds.Count == 0)
        {
            return "Gelungen, aber in diesem Gebiet gibt es zum gewählten Filter nichts zu finden.";
        }

        var total = outcome.Finds.Sum(find => find.Quantity);
        return $"Gelungen ({outcome.Margin} über dem SG) — {total} Zutat(en) gesammelt.";
    }

    /// <summary>Unlesbare Eingaben werden als 0 gewertet, statt den Wurf zu blockieren —
    /// am Spieltisch soll der Klick auf "Sammeln" nie an einem Tippfehler hängenbleiben.</summary>
    private static int ParseSkillModifier(string text)
    {
        var trimmed = text.Trim().TrimStart('+');
        return int.TryParse(trimmed, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value) ? value : 0;
    }
}
