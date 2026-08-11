using GMHelper.Core.Abstractions;
using GMHelper.Core.Entities;
using GMHelper.Core.Enums;
using GMHelper.Core.Models;
using GMHelper.Services;

namespace GMHelper.Services.Tests;

public class HerbalismHarvestServiceTests
{
    /// <summary>Gibt eine feste Wurffolge zurück, damit jeder Test genau eine Situation prüft.</summary>
    private sealed class StubDiceRoller : IDiceRoller
    {
        private readonly Queue<int> _results;

        public StubDiceRoller(params int[] results) => _results = new Queue<int>(results);

        public int Roll(int sides) => _results.Dequeue();
    }

    private static HerbalismRegion Region(int difficultyClass) => new()
    {
        Id = 1,
        Name = "Testgebiet",
        DifficultyClass = difficultyClass,
        Source = "Test",
    };

    private static HerbalismIngredient Ingredient(int id, string name, IngredientKind kind, IngredientRarity rarity) => new()
    {
        Id = id,
        HerbalismRegionId = 1,
        Name = name,
        Kind = kind,
        Rarity = rarity,
    };

    [Fact]
    public void Resolve_WhenCheckMissesTheDc_ReportsFailureWithoutFinds()
    {
        var sut = new HerbalismHarvestService(new StubDiceRoller(5));
        var attempt = new HarvestAttempt(
            Region(15),
            [Ingredient(1, "Moorbart", IngredientKind.PotionIngredient, IngredientRarity.Common)],
            SkillModifier: 0,
            UseHerbalismKit: false,
            KindFilter: null);

        var outcome = sut.Resolve(attempt);

        Assert.False(outcome.IsSuccess);
        Assert.Equal(5, outcome.Total);
        Assert.Equal(-10, outcome.Margin);
        Assert.Empty(outcome.Finds);
    }

    [Fact]
    public void Resolve_OnBareSuccess_YieldsOneCommonIngredient()
    {
        // Wurf 15 gegen SG 15 => Überschuss 0: eine Zutat, und nur gewöhnliche sind freigeschaltet.
        var sut = new HerbalismHarvestService(new StubDiceRoller(15, 1));
        var attempt = new HarvestAttempt(
            Region(15),
            [
                Ingredient(1, "Moorbart", IngredientKind.PotionIngredient, IngredientRarity.Common),
                Ingredient(2, "Geisterblüte", IngredientKind.PotionIngredient, IngredientRarity.VeryRare),
            ],
            SkillModifier: 0,
            UseHerbalismKit: false,
            KindFilter: null);

        var outcome = sut.Resolve(attempt);

        Assert.True(outcome.IsSuccess);
        var find = Assert.Single(outcome.Finds);
        Assert.Equal("Moorbart", find.Ingredient.Name);
        Assert.Equal(1, find.Quantity);
    }

    [Fact]
    public void Resolve_WhenOnlyLockedRaritiesRemain_SucceedsWithoutFinds()
    {
        var sut = new HerbalismHarvestService(new StubDiceRoller(15));
        var attempt = new HarvestAttempt(
            Region(15),
            [Ingredient(1, "Geisterblüte", IngredientKind.PotionIngredient, IngredientRarity.VeryRare)],
            SkillModifier: 0,
            UseHerbalismKit: false,
            KindFilter: null);

        var outcome = sut.Resolve(attempt);

        Assert.True(outcome.IsSuccess);
        Assert.Empty(outcome.Finds);
    }

    [Fact]
    public void Resolve_OnHighMargin_UnlocksRareIngredientsAndStacksRepeatDraws()
    {
        // Wurf 20 +5 = 25 gegen SG 10 => Überschuss 15: vier Zutaten, alle Seltenheiten offen.
        // Gewichte Common 8 / Uncommon 4 / Rare 2 / VeryRare 1 = 15; die 15 trifft die letzte.
        var sut = new HerbalismHarvestService(new StubDiceRoller(20, 15, 15, 15, 15));
        var attempt = new HarvestAttempt(
            Region(10),
            [
                Ingredient(1, "Feldkamille", IngredientKind.PotionIngredient, IngredientRarity.Common),
                Ingredient(2, "Sichelklee", IngredientKind.PotionIngredient, IngredientRarity.Uncommon),
                Ingredient(3, "Wyvernfarn", IngredientKind.PotionIngredient, IngredientRarity.Rare),
                Ingredient(4, "Wolkenlotos", IngredientKind.PotionIngredient, IngredientRarity.VeryRare),
            ],
            SkillModifier: 5,
            UseHerbalismKit: false,
            KindFilter: null);

        var outcome = sut.Resolve(attempt);

        Assert.Equal(25, outcome.Total);
        Assert.Equal(15, outcome.Margin);
        var find = Assert.Single(outcome.Finds);
        Assert.Equal("Wolkenlotos", find.Ingredient.Name);
        Assert.Equal(4, find.Quantity);
    }

    [Fact]
    public void Resolve_WithHerbalismKit_KeepsTheHigherOfTwoRollsAndReportsTheDiscardedOne()
    {
        var sut = new HerbalismHarvestService(new StubDiceRoller(3, 18, 1));
        var attempt = new HarvestAttempt(
            Region(15),
            [Ingredient(1, "Moorbart", IngredientKind.PotionIngredient, IngredientRarity.Common)],
            SkillModifier: 0,
            UseHerbalismKit: true,
            KindFilter: null);

        var outcome = sut.Resolve(attempt);

        Assert.Equal(18, outcome.DiceRoll);
        Assert.Equal(3, outcome.DiscardedDiceRoll);
        Assert.True(outcome.IsSuccess);
    }

    [Fact]
    public void Resolve_WithKindFilter_IgnoresIngredientsOfTheOtherKind()
    {
        // Kandidaten nach Filter "Zauberzutat": Nachtwurzel und die Doppelverwenderin
        // Silberflechte, je Gewicht 8 => Gesamtgewicht 16, die 1 trifft die erste.
        var sut = new HerbalismHarvestService(new StubDiceRoller(10, 1));
        var attempt = new HarvestAttempt(
            Region(10),
            [
                Ingredient(1, "Glutkappe", IngredientKind.PotionIngredient, IngredientRarity.Common),
                Ingredient(2, "Nachtwurzel", IngredientKind.SpellComponent, IngredientRarity.Common),
                Ingredient(3, "Silberflechte", IngredientKind.Both, IngredientRarity.Common),
            ],
            SkillModifier: 0,
            UseHerbalismKit: false,
            KindFilter: IngredientKind.SpellComponent);

        var outcome = sut.Resolve(attempt);

        var find = Assert.Single(outcome.Finds);
        Assert.Equal("Nachtwurzel", find.Ingredient.Name);
    }

    [Fact]
    public void Resolve_CapsFindsAtFour_EvenOnAnExtremeMargin()
    {
        // Überschuss 30 würde rechnerisch 7 Zutaten ergeben; der Deckel greift bei 4.
        var sut = new HerbalismHarvestService(new StubDiceRoller(20, 1, 1, 1, 1));
        var attempt = new HarvestAttempt(
            Region(10),
            [
                Ingredient(1, "Feldkamille", IngredientKind.PotionIngredient, IngredientRarity.Common),
                Ingredient(2, "Rotlärchenrinde", IngredientKind.PotionIngredient, IngredientRarity.Common),
            ],
            SkillModifier: 20,
            UseHerbalismKit: false,
            KindFilter: null);

        var outcome = sut.Resolve(attempt);

        Assert.Equal(4, outcome.Finds.Sum(find => find.Quantity));
    }
}
