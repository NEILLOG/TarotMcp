using System.Text.Json;
using TarotMcp.Core;
using TarotMcp.Core.Models;
using TarotMcp.Core.Randomness;
using Xunit;

namespace TarotMcp.Core.Tests;

public class DrawServiceTests
{
    private static readonly DeckData Deck = DeckData.LoadEmbedded();

    /// <summary>永遠回傳最大值：洗牌時 j == i（不交換），方向亂數回傳 1（逆位）。</summary>
    private sealed class MaxRandom : IRandomSource
    {
        public int NextInt(int toExclusive) => toExclusive - 1;
    }

    /// <summary>永遠回傳 0：方向全部正位。</summary>
    private sealed class ZeroRandom : IRandomSource
    {
        public int NextInt(int toExclusive) => 0;
    }

    [Fact]
    public void Draw_NoSwapShuffle_TakesCardsInDeckOrder_AndFillsPositionsInOrder()
    {
        var service = new DrawService(Deck, new MaxRandom());

        var result = service.Draw("three_card_timeline");

        Assert.Equal(new[] { 1, 2, 3 }, result.Cards.Select(c => c.Position.Index));
        Assert.Equal(Deck.Cards.Take(3).Select(c => c.Id), result.Cards.Select(c => c.Card.Id));
        Assert.All(result.Cards, c => Assert.Equal(Orientation.Reversed, c.Orientation));
    }

    [Fact]
    public void Draw_AllowReversedFalse_AlwaysUpright()
    {
        var service = new DrawService(Deck, new MaxRandom());

        var result = service.Draw("celtic_cross", allowReversed: false);

        Assert.All(result.Cards, c => Assert.Equal(Orientation.Upright, c.Orientation));
    }

    [Fact]
    public void Draw_ZeroRandom_IsAllUpright()
    {
        var service = new DrawService(Deck, new ZeroRandom());

        var result = service.Draw("celtic_cross");

        Assert.All(result.Cards, c => Assert.Equal(Orientation.Upright, c.Orientation));
    }

    [Fact]
    public void Draw_IsWithoutReplacement_ForEverySpread()
    {
        var service = new DrawService(Deck, new SecureRandomSource());

        foreach (var spread in Deck.Spreads)
        {
            for (var i = 0; i < 500; i++)
            {
                var result = service.Draw(spread.Id);

                Assert.Equal(spread.CardCount, result.Cards.Count);
                Assert.Equal(result.Cards.Count, result.Cards.Select(c => c.Card.Id).Distinct().Count());
            }
        }
    }

    [Fact]
    public void Draw_ReversedRatio_IsRoughlyHalf()
    {
        var service = new DrawService(Deck, new SecureRandomSource());
        var reversed = 0;
        var total = 0;

        for (var i = 0; i < 3000; i++)
        {
            foreach (var card in service.Draw("celtic_cross").Cards)
            {
                total++;
                if (card.Orientation == Orientation.Reversed) reversed++;
            }
        }

        var ratio = (double)reversed / total;
        Assert.InRange(ratio, 0.47, 0.53);   // 30000 次抽樣，標準差約 0.3%，這個範圍夠寬鬆
    }

    [Fact]
    public void Draw_FirstCardIsRoughlyUniform()
    {
        var service = new DrawService(Deck, new SeededRandomSource(12345));
        var counts = Deck.Cards.ToDictionary(c => c.Id, _ => 0);
        const int rounds = 78_000;

        for (var i = 0; i < rounds; i++)
        {
            counts[service.Draw("single_card").Cards[0].Card.Id]++;
        }

        // 期望每張約 1000 次，標準差約 31；±300 是很寬鬆的範圍，只用來抓到明顯的偏差。
        Assert.All(counts, kv => Assert.InRange(kv.Value, 700, 1300));
    }

    [Fact]
    public void Draw_SameSeed_GivesSameResult()
    {
        var a = new DrawService(Deck, new SeededRandomSource(42)).Draw("celtic_cross");
        var b = new DrawService(Deck, new SeededRandomSource(42)).Draw("celtic_cross");

        Assert.Equal(a.Cards.Select(c => (c.Card.Id, c.Orientation)), b.Cards.Select(c => (c.Card.Id, c.Orientation)));
    }

    [Fact]
    public void Draw_UnknownSpread_ThrowsWithAvailableIds()
    {
        var service = new DrawService(Deck, new SecureRandomSource());

        var ex = Assert.Throws<UnknownSpreadException>(() => service.Draw("nope"));

        Assert.Contains("celtic_cross", ex.Message);
    }

    [Fact]
    public void View_ContainsOnlyCurrentDirectionMeaning()
    {
        var service = new DrawService(Deck, new MaxRandom());   // 全部逆位
        var result = service.Draw("single_card");
        var drawn = result.Cards[0];

        var json = TarotJson.Serialize(result.ToView());

        Assert.Contains(drawn.Card.MeaningReversed, json);
        Assert.DoesNotContain(drawn.Card.MeaningUpright, json);
        Assert.Contains("\"orientation\":\"reversed\"", json);
        Assert.Contains("\"orientation_zh\":\"逆位\"", json);
    }

    [Fact]
    public void View_SerializesLayoutAndSnakeCaseFields()
    {
        var service = new DrawService(Deck, new ZeroRandom());

        using var doc = JsonDocument.Parse(TarotJson.Serialize(service.Draw("celtic_cross").ToView()));
        var positions = doc.RootElement.GetProperty("positions");

        Assert.Equal(10, positions.GetArrayLength());
        var crosses = positions.EnumerateArray().Single(p => p.GetProperty("position_id").GetString() == "crosses");
        Assert.Equal(90, crosses.GetProperty("layout").GetProperty("rotation").GetDouble());
        Assert.True(crosses.GetProperty("card").TryGetProperty("name_zh", out _));
    }
}
