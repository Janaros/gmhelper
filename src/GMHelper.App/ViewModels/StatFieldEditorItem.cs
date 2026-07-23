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

    /// <summary>True for standardized fields (e.g. Monster's HP/RK/TK) that must always exist —
    /// name becomes read-only and the remove button is hidden, but the value stays editable.</summary>
    [ObservableProperty]
    private bool _isLocked;

    /// <summary>0 = unbounded (WPF TextBox.MaxLength convention). Used e.g. to cap TK at 2 chars.</summary>
    [ObservableProperty]
    private int _valueMaxLength;
}
