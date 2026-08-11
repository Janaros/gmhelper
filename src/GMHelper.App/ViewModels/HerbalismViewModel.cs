using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Core.Enums;
using Microsoft.Extensions.Logging;

namespace GMHelper.App.ViewModels;

/// <summary>
/// Kräuterkunde: Gebiet wählen, dessen Fundtabelle durchsuchen und pflegen. Das eigentliche
/// Auswürfeln liegt in <see cref="Harvest"/>. Global und nicht kampagnengebunden, wie die
/// Monster-Datenbank — die Flora der Schwertküste ändert sich nicht je Spielrunde.
/// </summary>
public partial class HerbalismViewModel : ObservableObject
{
    private readonly IHerbalismRegionService _regionService;
    private readonly IHerbalismIngredientService _ingredientService;
    private readonly IHerbalismSeeder _seeder;
    private readonly ILogger<HerbalismViewModel> _logger;

    /// <summary>Ungefilterte Fundtabelle des gewählten Gebiets — Grundlage für Anzeige-Filter
    /// und Sammelwurf.</summary>
    private IReadOnlyList<HerbalismIngredient> _allIngredients = [];

    public ObservableCollection<HerbalismRegion> Regions { get; } = new();
    public ObservableCollection<IngredientRowVm> Ingredients { get; } = new();
    public HerbalismHarvestViewModel Harvest { get; }

    public IReadOnlyList<KindFilterOption> KindFilterOptions { get; } =
    [
        new("Alle Zutaten", null),
        new("Nur Trankzutaten", IngredientKind.PotionIngredient),
        new("Nur Zauberzutaten", IngredientKind.SpellComponent),
        new("Nur Doppelverwender", IngredientKind.Both),
    ];

    public IReadOnlyList<KindOption> KindOptions { get; } =
    [
        new(HerbalismLabels.For(IngredientKind.PotionIngredient), IngredientKind.PotionIngredient),
        new(HerbalismLabels.For(IngredientKind.SpellComponent), IngredientKind.SpellComponent),
        new(HerbalismLabels.For(IngredientKind.Both), IngredientKind.Both),
    ];

    public IReadOnlyList<RarityOption> RarityOptions { get; } =
    [
        new(HerbalismLabels.For(IngredientRarity.Common), IngredientRarity.Common),
        new(HerbalismLabels.For(IngredientRarity.Uncommon), IngredientRarity.Uncommon),
        new(HerbalismLabels.For(IngredientRarity.Rare), IngredientRarity.Rare),
        new(HerbalismLabels.For(IngredientRarity.VeryRare), IngredientRarity.VeryRare),
    ];

    [ObservableProperty]
    private HerbalismRegion? _selectedRegion;

    [ObservableProperty]
    private KindFilterOption _selectedKindFilter;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _editRegionName = string.Empty;

    [ObservableProperty]
    private string _editRegionTerrain = string.Empty;

    [ObservableProperty]
    private string _editRegionDescription = string.Empty;

    [ObservableProperty]
    private string _editRegionDifficultyClassText = string.Empty;

    [ObservableProperty]
    private IngredientRowVm? _selectedIngredient;

    [ObservableProperty]
    private string _editIngredientName = string.Empty;

    [ObservableProperty]
    private KindOption _editIngredientKind;

    [ObservableProperty]
    private RarityOption _editIngredientRarity;

    [ObservableProperty]
    private string _editIngredientEffect = string.Empty;

    [ObservableProperty]
    private string _editIngredientNotes = string.Empty;

    [ObservableProperty]
    private string _editIngredientValueText = string.Empty;

    [ObservableProperty]
    private string? _statusMessage;

    public event EventHandler? BackRequested;

    public HerbalismViewModel(
        IHerbalismRegionService regionService,
        IHerbalismIngredientService ingredientService,
        IHerbalismSeeder seeder,
        HerbalismHarvestViewModel harvest,
        ILogger<HerbalismViewModel> logger)
    {
        _regionService = regionService;
        _ingredientService = ingredientService;
        _seeder = seeder;
        _logger = logger;

        Harvest = harvest;
        _selectedKindFilter = KindFilterOptions[0];
        _editIngredientKind = KindOptions[0];
        _editIngredientRarity = RarityOptions[0];
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _seeder.EnsureSeededAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed herbalism regions");
            StatusMessage = $"Startdaten konnten nicht angelegt werden: {ex.Message}";
        }

        var previouslySelectedId = SelectedRegion?.Id;
        await ReloadRegionsAsync();

        SelectedRegion = Regions.FirstOrDefault(r => r.Id == previouslySelectedId) ?? Regions.FirstOrDefault();
    }

    [RelayCommand]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private async Task NewRegionAsync()
    {
        try
        {
            var region = await _regionService.CreateRegionAsync("Neues Gebiet");
            await ReloadRegionsAsync();
            SelectedRegion = Regions.FirstOrDefault(r => r.Id == region.Id);
            StatusMessage = "Gebiet angelegt — Name und SG jetzt anpassen.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create herbalism region");
            StatusMessage = $"Fehler beim Anlegen: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveRegionAsync()
    {
        if (SelectedRegion is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(EditRegionName))
        {
            StatusMessage = "Gebietsname darf nicht leer sein.";
            return;
        }

        if (!TryParseDifficultyClass(EditRegionDifficultyClassText, out var difficultyClass))
        {
            StatusMessage = "SG muss eine Zahl zwischen 1 und 30 sein.";
            return;
        }

        var regionId = SelectedRegion.Id;

        try
        {
            await _regionService.UpdateRegionAsync(
                regionId,
                EditRegionName.Trim(),
                NullIfBlank(EditRegionTerrain),
                NullIfBlank(EditRegionDescription),
                difficultyClass);

            await ReloadRegionsAsync();
            SelectedRegion = Regions.FirstOrDefault(r => r.Id == regionId);
            StatusMessage = "Gebiet gespeichert.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save herbalism region {RegionId}", regionId);
            StatusMessage = $"Fehler beim Speichern: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteRegionAsync()
    {
        if (SelectedRegion is null)
        {
            return;
        }

        var regionId = SelectedRegion.Id;

        try
        {
            await _regionService.DeleteRegionAsync(regionId);
            await ReloadRegionsAsync();
            SelectedRegion = Regions.FirstOrDefault();
            StatusMessage = "Gebiet samt Fundtabelle gelöscht.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete herbalism region {RegionId}", regionId);
            StatusMessage = $"Fehler beim Löschen: {ex.Message}";
        }
    }

    [RelayCommand]
    private void NewIngredient()
    {
        if (SelectedRegion is null)
        {
            StatusMessage = "Bitte zuerst ein Gebiet wählen.";
            return;
        }

        SelectedIngredient = null;
        ResetIngredientForm();
    }

    [RelayCommand]
    private async Task SaveIngredientAsync()
    {
        if (SelectedRegion is null)
        {
            StatusMessage = "Bitte zuerst ein Gebiet wählen.";
            return;
        }

        if (string.IsNullOrWhiteSpace(EditIngredientName))
        {
            StatusMessage = "Zutatenname darf nicht leer sein.";
            return;
        }

        if (!TryParseValue(EditIngredientValueText, out var valueInGoldPieces))
        {
            StatusMessage = "Wert muss leer oder eine nicht-negative Zahl sein.";
            return;
        }

        try
        {
            var ingredientId = SelectedIngredient?.Id
                ?? (await _ingredientService.CreateIngredientAsync(SelectedRegion.Id, EditIngredientName.Trim())).Id;

            await _ingredientService.UpdateIngredientAsync(
                ingredientId,
                EditIngredientName.Trim(),
                EditIngredientKind.Value,
                EditIngredientRarity.Value,
                NullIfBlank(EditIngredientEffect),
                NullIfBlank(EditIngredientNotes),
                valueInGoldPieces);

            await ReloadIngredientsAsync();
            SelectedIngredient = Ingredients.FirstOrDefault(i => i.Id == ingredientId);
            StatusMessage = "Zutat gespeichert.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save herbalism ingredient {IngredientId}", SelectedIngredient?.Id);
            StatusMessage = $"Fehler beim Speichern: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteIngredientAsync()
    {
        if (SelectedIngredient is null)
        {
            return;
        }

        var ingredientId = SelectedIngredient.Id;

        try
        {
            await _ingredientService.DeleteIngredientAsync(ingredientId);
            SelectedIngredient = null;
            await ReloadIngredientsAsync();
            StatusMessage = "Zutat gelöscht.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete herbalism ingredient {IngredientId}", ingredientId);
            StatusMessage = $"Fehler beim Löschen: {ex.Message}";
        }
    }

    partial void OnSelectedRegionChanged(HerbalismRegion? value)
    {
        EditRegionName = value?.Name ?? string.Empty;
        EditRegionTerrain = value?.Terrain ?? string.Empty;
        EditRegionDescription = value?.Description ?? string.Empty;
        EditRegionDifficultyClassText = value?.DifficultyClass.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

        SelectedIngredient = null;
        ResetIngredientForm();

        _ = ReloadIngredientsAsync();
    }

    partial void OnSelectedKindFilterChanged(KindFilterOption value) => ApplyFilter();

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedIngredientChanged(IngredientRowVm? value)
    {
        if (value is null)
        {
            return;
        }

        EditIngredientName = value.Model.Name;
        EditIngredientKind = KindOptions.First(o => o.Value == value.Model.Kind);
        EditIngredientRarity = RarityOptions.First(o => o.Value == value.Model.Rarity);
        EditIngredientEffect = value.Model.Effect ?? string.Empty;
        EditIngredientNotes = value.Model.Notes ?? string.Empty;
        EditIngredientValueText = value.Model.ValueInGoldPieces?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private async Task ReloadRegionsAsync()
    {
        try
        {
            var regions = await _regionService.GetRegionsAsync();

            Regions.Clear();
            foreach (var region in regions)
            {
                Regions.Add(region);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load herbalism regions");
            StatusMessage = $"Fehler beim Laden der Gebiete: {ex.Message}";
        }
    }

    private async Task ReloadIngredientsAsync()
    {
        if (SelectedRegion is null)
        {
            _allIngredients = [];
            ApplyFilter();
            return;
        }

        try
        {
            _allIngredients = await _ingredientService.GetIngredientsAsync(SelectedRegion.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load ingredients of region {RegionId}", SelectedRegion.Id);
            StatusMessage = $"Fehler beim Laden der Zutaten: {ex.Message}";
            _allIngredients = [];
        }

        ApplyFilter();
    }

    /// <summary>
    /// Baut die angezeigte Tabelle neu auf. Der Sammelwurf bekommt bewusst die ungefilterte
    /// Liste plus den Art-Filter: der Suchtext ist eine reine Nachschlagehilfe und darf nicht
    /// beeinflussen, was im Gebiet wächst.
    /// </summary>
    private void ApplyFilter()
    {
        var kindFilter = SelectedKindFilter?.Value;
        var search = SearchText?.Trim() ?? string.Empty;

        Ingredients.Clear();
        foreach (var ingredient in _allIngredients.Where(i => i.Kind.MatchesFilter(kindFilter) && MatchesSearch(i, search)))
        {
            Ingredients.Add(new IngredientRowVm(ingredient));
        }

        Harvest.SetContext(SelectedRegion, _allIngredients, kindFilter);
    }

    private static bool MatchesSearch(HerbalismIngredient ingredient, string search)
    {
        if (search.Length == 0)
        {
            return true;
        }

        return ingredient.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase)
            || (ingredient.Effect?.Contains(search, StringComparison.CurrentCultureIgnoreCase) ?? false)
            || (ingredient.Notes?.Contains(search, StringComparison.CurrentCultureIgnoreCase) ?? false);
    }

    private void ResetIngredientForm()
    {
        EditIngredientName = string.Empty;
        EditIngredientKind = KindOptions[0];
        EditIngredientRarity = RarityOptions[0];
        EditIngredientEffect = string.Empty;
        EditIngredientNotes = string.Empty;
        EditIngredientValueText = string.Empty;
    }

    private static bool TryParseDifficultyClass(string text, out int difficultyClass)
    {
        return int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out difficultyClass)
            && difficultyClass is >= 1 and <= 30;
    }

    private static bool TryParseValue(string text, out int? valueInGoldPieces)
    {
        valueInGoldPieces = null;

        var trimmed = text.Trim();
        if (trimmed.Length == 0)
        {
            return true;
        }

        if (!int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) || parsed < 0)
        {
            return false;
        }

        valueInGoldPieces = parsed;
        return true;
    }

    private static string? NullIfBlank(string text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
