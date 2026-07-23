using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Core.Enums;
using GMHelper.Core.Models;
using Microsoft.Extensions.Logging;

namespace GMHelper.App.ViewModels;

/// <summary>
/// Global monster database — not scoped to a campaign, reachable as a peer of the campaign
/// list. Reuses the same StatFieldEditor control/item type as the player roster.
/// </summary>
public partial class MonsterDatabaseViewModel : ObservableObject
{
    /// <summary>Stat fields every monster/NPC entry always gets, so the GM never has to
    /// remember to add HP/RK ("Rüstungsklasse")/TK ("Tokennummer") by hand. Name is locked
    /// (read-only, not removable) to keep them standardized; only the value is free-form.</summary>
    private static readonly (string Name, int ValueMaxLength)[] StandardStatFields =
    [
        ("HP", 0),
        ("RK", 0),
        ("TK", 2),
    ];

    private readonly IMonsterService _monsterService;
    private readonly IStatFieldService _statFieldService;
    private readonly IImageLibraryService _imageLibraryService;
    private readonly IMonsterImportService _monsterImportService;
    private readonly IMonsterExportService _monsterExportService;
    private readonly ILogger<MonsterDatabaseViewModel> _logger;

    public ObservableCollection<Monster> Monsters { get; } = new();
    public ObservableCollection<StatFieldEditorItem> StatFields { get; } = new();

    [ObservableProperty]
    private Monster? _selectedMonster;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private string _editNotes = string.Empty;

    [ObservableProperty]
    private string? _statusMessage;

    public event EventHandler? BackRequested;

    public MonsterDatabaseViewModel(
        IMonsterService monsterService,
        IStatFieldService statFieldService,
        IImageLibraryService imageLibraryService,
        IMonsterImportService monsterImportService,
        IMonsterExportService monsterExportService,
        ILogger<MonsterDatabaseViewModel> logger)
    {
        _monsterService = monsterService;
        _statFieldService = statFieldService;
        _imageLibraryService = imageLibraryService;
        _monsterImportService = monsterImportService;
        _monsterExportService = monsterExportService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await ReloadMonstersAsync();
    }

    [RelayCommand]
    private void Back() => BackRequested?.Invoke(this, EventArgs.Empty);

    [RelayCommand]
    private void NewMonster()
    {
        SelectedMonster = null;
        _ = LoadSelectedMonsterAsync(null); // explicit: SelectedMonster's changed-hook is a no-op if it was already null
    }

    [RelayCommand]
    private async Task SaveSelectedMonsterAsync()
    {
        if (string.IsNullOrWhiteSpace(EditName))
        {
            StatusMessage = "Name darf nicht leer sein.";
            return;
        }

        try
        {
            var currentImageAssetId = SelectedMonster?.ImageAssetId;
            var selectedId = SelectedMonster?.Id
                ?? (await _monsterService.CreateMonsterAsync(EditName.Trim())).Id;

            await _monsterService.UpdateMonsterAsync(
                selectedId,
                EditName.Trim(),
                string.IsNullOrWhiteSpace(EditNotes) ? null : EditNotes.Trim(),
                currentImageAssetId);

            var fields = StatFields
                .Where(f => !string.IsNullOrWhiteSpace(f.Name))
                .Select(f => (f.Name, f.Value))
                .ToList();
            await _statFieldService.ReplaceStatFieldsAsync(StatFieldOwnerType.Monster, selectedId, fields);

            StatusMessage = "Gespeichert.";
            await ReloadMonstersAsync();
            SelectedMonster = Monsters.FirstOrDefault(m => m.Id == selectedId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save monster {MonsterId}", SelectedMonster?.Id);
            StatusMessage = $"Fehler beim Speichern: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteSelectedMonsterAsync()
    {
        if (SelectedMonster is null)
        {
            return;
        }

        try
        {
            await _monsterService.DeleteMonsterAsync(SelectedMonster.Id);
            SelectedMonster = null;
            StatusMessage = null;
            await ReloadMonstersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete monster {MonsterId}", SelectedMonster?.Id);
            StatusMessage = $"Fehler beim Löschen: {ex.Message}";
        }
    }

    public async Task AssignImageAsync(string sourceFilePath)
    {
        if (SelectedMonster is null)
        {
            StatusMessage = "Bitte zuerst speichern, bevor ein Bild zugewiesen wird.";
            return;
        }

        var selectedId = SelectedMonster.Id;

        try
        {
            var image = await _imageLibraryService.AddImageAsync(ImageOwnerType.Monster, selectedId, sourceFilePath, ImageCategory.Monster);
            await _monsterService.UpdateMonsterAsync(selectedId, EditName.Trim(), string.IsNullOrWhiteSpace(EditNotes) ? null : EditNotes.Trim(), image.Id);

            StatusMessage = "Bild zugewiesen.";
            await ReloadMonstersAsync();
            SelectedMonster = Monsters.FirstOrDefault(m => m.Id == selectedId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign image to monster {MonsterId}", selectedId);
            StatusMessage = $"Fehler beim Bild zuweisen: {ex.Message}";
        }
    }

    public async Task<IReadOnlyList<MonsterImportRecord>> ParseImportFileAsync(string filePath)
    {
        return Path.GetExtension(filePath).Equals(".csv", StringComparison.OrdinalIgnoreCase)
            ? await _monsterImportService.ParseCsvAsync(filePath)
            : await _monsterImportService.ParseJsonAsync(filePath);
    }

    public async Task CommitImportAsync(IReadOnlyList<MonsterImportRecord> records, string baseDirectory, MonsterImportConflictStrategy conflictStrategy)
    {
        try
        {
            var result = await _monsterImportService.CommitImportAsync(records, baseDirectory, conflictStrategy);
            StatusMessage = $"Import abgeschlossen: {result.CreatedCount} neu, {result.UpdatedCount} aktualisiert, {result.SkippedCount} übersprungen.";
            await ReloadMonstersAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to commit monster import");
            StatusMessage = $"Fehler beim Import: {ex.Message}";
        }
    }

    public async Task ExportAsync(string destinationFilePath)
    {
        try
        {
            if (Path.GetExtension(destinationFilePath).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                await _monsterExportService.ExportToCsvAsync(destinationFilePath);
            }
            else
            {
                await _monsterExportService.ExportToJsonAsync(destinationFilePath);
            }

            StatusMessage = $"Exportiert nach {destinationFilePath}.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export monsters to {DestinationFilePath}", destinationFilePath);
            StatusMessage = $"Fehler beim Export: {ex.Message}";
        }
    }

    public async Task<string?> GetPortraitAbsolutePathAsync()
    {
        if (SelectedMonster?.ImageAssetId is not { } imageAssetId)
        {
            return null;
        }

        var images = await _imageLibraryService.GetImagesAsync(ImageOwnerType.Monster, SelectedMonster.Id);
        var image = images.FirstOrDefault(i => i.Id == imageAssetId);
        return image is null ? null : _imageLibraryService.GetAbsoluteFilePath(image);
    }

    partial void OnSelectedMonsterChanged(Monster? value)
    {
        _ = LoadSelectedMonsterAsync(value);
    }

    private async Task LoadSelectedMonsterAsync(Monster? monster)
    {
        StatFields.Clear();

        if (monster is null)
        {
            EditName = string.Empty;
            EditNotes = string.Empty;
            foreach (var (name, valueMaxLength) in StandardStatFields)
            {
                StatFields.Add(new StatFieldEditorItem { Name = name, IsLocked = true, ValueMaxLength = valueMaxLength });
            }
            return;
        }

        EditName = monster.Name;
        EditNotes = monster.Notes ?? string.Empty;

        var fields = await _statFieldService.GetStatFieldsAsync(StatFieldOwnerType.Monster, monster.Id);
        foreach (var field in fields)
        {
            var standardField = Array.Find(StandardStatFields, f => string.Equals(f.Name, field.Name, StringComparison.OrdinalIgnoreCase));
            StatFields.Add(new StatFieldEditorItem
            {
                Name = field.Name,
                Value = field.Value,
                IsLocked = standardField.Name is not null,
                ValueMaxLength = standardField.ValueMaxLength,
            });
        }
    }

    private async Task ReloadMonstersAsync()
    {
        var monsters = await _monsterService.GetMonstersAsync();

        Monsters.Clear();
        foreach (var monster in monsters)
        {
            Monsters.Add(monster);
        }
    }
}
