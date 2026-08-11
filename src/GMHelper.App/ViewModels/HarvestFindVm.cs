using GMHelper.Core.Models;

namespace GMHelper.App.ViewModels;

/// <summary>Eine Zeile der Fundliste nach einem Sammelwurf.</summary>
public class HarvestFindVm
{
    public HarvestFindVm(HarvestFind find)
    {
        Quantity = find.Quantity;
        Name = find.Ingredient.Name;
        KindLabel = HerbalismLabels.For(find.Ingredient.Kind);
        RarityLabel = HerbalismLabels.For(find.Ingredient.Rarity);
        Effect = find.Ingredient.Effect ?? string.Empty;
    }

    public int Quantity { get; }
    public string Name { get; }
    public string KindLabel { get; }
    public string RarityLabel { get; }
    public string Effect { get; }

    public string DisplayName => Quantity > 1 ? $"{Quantity}× {Name}" : Name;
}
