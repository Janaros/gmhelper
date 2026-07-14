using CommunityToolkit.Mvvm.ComponentModel;

namespace GMHelper.App.ViewModels;

/// <summary>
/// Editable row shown in the reusable StatFieldEditor control — a lightweight UI-only DTO,
/// distinct from the persisted GMHelper.Core.Entities.StatField.
/// </summary>
public partial class StatFieldEditorItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _value = string.Empty;
}
