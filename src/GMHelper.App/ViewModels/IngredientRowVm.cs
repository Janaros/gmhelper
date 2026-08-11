using GMHelper.Core.Entities;

namespace GMHelper.App.ViewModels;

/// <summary>
/// Anzeigezeile der Fundtabelle. Rein lesend: Änderungen laufen über das Bearbeitungsformular
/// und den Service, danach wird die Tabelle neu aufgebaut.
/// </summary>
public class IngredientRowVm
{
    public IngredientRowVm(HerbalismIngredient model)
    {
        Model = model;
    }

    public HerbalismIngredient Model { get; }

    public int Id => Model.Id;
    public string Name => Model.Name;
    public string KindLabel => HerbalismLabels.For(Model.Kind);
    public string RarityLabel => HerbalismLabels.For(Model.Rarity);
    public string Effect => Model.Effect ?? string.Empty;
    public string ValueLabel => Model.ValueInGoldPieces is { } value ? $"{value} GM" : "–";
}
