using GMHelper.Core.Enums;

namespace GMHelper.Core.Tests;

public class IngredientKindExtensionsTests
{
    [Theory]
    [InlineData(IngredientKind.PotionIngredient)]
    [InlineData(IngredientKind.SpellComponent)]
    [InlineData(IngredientKind.Both)]
    public void MatchesFilter_WithoutFilter_LetsEverythingThrough(IngredientKind kind)
    {
        Assert.True(kind.MatchesFilter(null));
    }

    [Theory]
    [InlineData(IngredientKind.PotionIngredient, IngredientKind.PotionIngredient, true)]
    [InlineData(IngredientKind.Both, IngredientKind.PotionIngredient, true)]
    [InlineData(IngredientKind.SpellComponent, IngredientKind.PotionIngredient, false)]
    [InlineData(IngredientKind.SpellComponent, IngredientKind.SpellComponent, true)]
    [InlineData(IngredientKind.Both, IngredientKind.SpellComponent, true)]
    [InlineData(IngredientKind.PotionIngredient, IngredientKind.SpellComponent, false)]
    public void MatchesFilter_OnSingleKind_IncludesDualPurposeIngredients(IngredientKind kind, IngredientKind filter, bool expected)
    {
        Assert.Equal(expected, kind.MatchesFilter(filter));
    }

    [Theory]
    [InlineData(IngredientKind.Both, true)]
    [InlineData(IngredientKind.PotionIngredient, false)]
    [InlineData(IngredientKind.SpellComponent, false)]
    public void MatchesFilter_OnBoth_KeepsOnlyDualPurposeIngredients(IngredientKind kind, bool expected)
    {
        Assert.Equal(expected, kind.MatchesFilter(IngredientKind.Both));
    }
}
