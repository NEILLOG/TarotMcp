using System.Text.Json;
using System.Text.Json.Nodes;
using TarotMcp.Core;
using Xunit;

namespace TarotMcp.Core.Tests;

public class DeckDataTests
{
    [Fact]
    public void EmbeddedData_LoadsAndValidates()
    {
        var deck = DeckData.LoadEmbedded();

        Assert.Equal(78, deck.Cards.Count);
        Assert.Equal(78, deck.Cards.Select(c => c.Id).Distinct().Count());
        Assert.Equal(22, deck.Cards.Count(c => c.Arcana == "major"));
        foreach (var suit in new[] { "wands", "cups", "swords", "pentacles" })
        {
            Assert.Equal(14, deck.Cards.Count(c => c.Suit == suit));
        }
    }

    [Fact]
    public void EveryCard_HasBothDirections_AndCorrectElement()
    {
        var deck = DeckData.LoadEmbedded();

        foreach (var card in deck.Cards)
        {
            Assert.False(string.IsNullOrWhiteSpace(card.MeaningUpright), card.Id);
            Assert.False(string.IsNullOrWhiteSpace(card.MeaningReversed), card.Id);
            Assert.NotEmpty(card.KeywordsUpright);
            Assert.NotEmpty(card.KeywordsReversed);
            Assert.Equal(card.Arcana == "major", card.Element is null);
            Assert.Equal($"cards/{card.Id}.webp", card.Image);
            Assert.False(string.IsNullOrWhiteSpace(card.ImagePrompt), card.Id);
        }
    }

    [Fact]
    public void FindCard_ReturnsKnownCard_AndNullForUnknown()
    {
        var deck = DeckData.LoadEmbedded();

        Assert.Equal("愚者", deck.FindCard("major_00")?.NameZh);
        Assert.Null(deck.FindCard("nope"));
    }

    [Fact]
    public void Spreads_HaveConsistentPositions()
    {
        var deck = DeckData.LoadEmbedded();

        Assert.NotNull(deck.FindSpread("single_card"));
        Assert.NotNull(deck.FindSpread("three_card_timeline"));
        var celtic = deck.FindSpread("celtic_cross");
        Assert.NotNull(celtic);
        Assert.Equal(10, celtic!.Positions.Count);
        Assert.Equal(90, celtic.Positions.Single(p => p.Id == "crosses").Layout.Rotation);

        foreach (var spread in deck.Spreads)
        {
            Assert.Equal(spread.CardCount, spread.Positions.Count);
        }
    }

    [Fact]
    public void Parse_RejectsDuplicateIds()
    {
        var cards = LoadCardsNode();
        var array = cards["cards"]!.AsArray();
        array[1]!["id"] = array[0]!["id"]!.GetValue<string>();

        var ex = Assert.Throws<DeckValidationException>(() => DeckData.Parse(cards.ToJsonString(), ReadSpreads()));

        Assert.Contains(ex.Errors, e => e.Contains("id 重複"));
    }

    [Fact]
    public void Parse_RejectsWrongCardCount()
    {
        var cards = LoadCardsNode();
        cards["cards"]!.AsArray().RemoveAt(0);

        var ex = Assert.Throws<DeckValidationException>(() => DeckData.Parse(cards.ToJsonString(), ReadSpreads()));

        Assert.Contains(ex.Errors, e => e.Contains("應為 78"));
    }

    [Fact]
    public void Parse_RejectsMajorCardWithElement()
    {
        var cards = LoadCardsNode();
        cards["cards"]!.AsArray()[0]!["element"] = "火";

        var ex = Assert.Throws<DeckValidationException>(() => DeckData.Parse(cards.ToJsonString(), ReadSpreads()));

        Assert.Contains(ex.Errors, e => e.Contains("element 必須為 null"));
    }

    [Fact]
    public void Parse_RejectsSpreadWithMismatchedCount()
    {
        var spreads = JsonNode.Parse(ReadSpreads())!;
        spreads["spreads"]!.AsArray()[0]!["card_count"] = 5;

        var ex = Assert.Throws<DeckValidationException>(() => DeckData.Parse(ReadCards(), spreads.ToJsonString()));

        Assert.Contains(ex.Errors, e => e.Contains("card_count"));
    }

    private static JsonNode LoadCardsNode() => JsonNode.Parse(ReadCards())!;

    private static string ReadCards() => ReadResource("TarotMcp.Data.cards.json");

    private static string ReadSpreads() => ReadResource("TarotMcp.Data.spreads.json");

    private static string ReadResource(string name)
    {
        using var stream = typeof(DeckData).Assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
