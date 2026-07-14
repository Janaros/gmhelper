using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using GMHelper.Core.Enums;
using GMHelper.Core.Models;

namespace GMHelper.App.ViewModels;

/// <summary>
/// Backs the transient import-preview dialog. Not DI-registered — created directly by
/// MonsterDatabaseView's code-behind for the duration of a single import action.
/// </summary>
public partial class ImportPreviewViewModel : ObservableObject
{
    public ObservableCollection<MonsterImportRecord> Records { get; }

    public IReadOnlyList<MonsterImportConflictStrategy> ConflictStrategies { get; } = Enum.GetValues<MonsterImportConflictStrategy>();

    [ObservableProperty]
    private MonsterImportConflictStrategy _conflictStrategy = MonsterImportConflictStrategy.Skip;

    public ImportPreviewViewModel(IReadOnlyList<MonsterImportRecord> records)
    {
        Records = new ObservableCollection<MonsterImportRecord>(records);
    }
}
