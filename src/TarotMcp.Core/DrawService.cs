using TarotMcp.Core.Models;
using TarotMcp.Core.Randomness;

namespace TarotMcp.Core;

public sealed class UnknownSpreadException : Exception
{
    public UnknownSpreadException(string spreadId, IEnumerable<string> available)
        : base($"找不到牌陣「{spreadId}」。可用的牌陣：{string.Join("、", available)}")
    {
    }
}

public sealed class UnknownCardException : Exception
{
    public UnknownCardException(string cardId)
        : base($"找不到牌「{cardId}」。牌的 id 格式例如 major_00、wands_01。")
    {
    }
}

/// <summary>牌陣中的一個位置與落在這個位置的牌。</summary>
public sealed record DrawnCard(SpreadPosition Position, Card Card, Orientation Orientation)
{
    public IReadOnlyList<string> Keywords => Orientation == Orientation.Reversed ? Card.KeywordsReversed : Card.KeywordsUpright;

    public string Meaning => Orientation == Orientation.Reversed ? Card.MeaningReversed : Card.MeaningUpright;
}

public sealed record DrawResult(Spread Spread, bool AllowReversed, IReadOnlyList<DrawnCard> Cards);

/// <summary>
/// 洗牌與抽牌：Fisher–Yates 洗牌後依序取牌（不放回），再逐張決定正逆位。
/// 亂數的取用順序固定：先洗牌（i 從 n-1 到 1，每次取 NextInt(i+1)），再依位置順序決定方向（NextInt(2)，1 為逆位）。
/// </summary>
public sealed class DrawService
{
    private readonly DeckData _deck;
    private readonly IRandomSource _random;

    public DrawService(DeckData deck, IRandomSource random)
    {
        _deck = deck;
        _random = random;
    }

    public DrawResult Draw(string spreadId, bool allowReversed = true)
    {
        var spread = _deck.FindSpread(spreadId)
                     ?? throw new UnknownSpreadException(spreadId, _deck.Spreads.Select(s => s.Id));

        var shuffled = Shuffle(_deck.Cards);
        var positions = spread.Positions.OrderBy(p => p.Index).ToList();

        var drawn = new List<DrawnCard>(positions.Count);
        for (var i = 0; i < positions.Count; i++)
        {
            var orientation = allowReversed && _random.NextInt(2) == 1 ? Orientation.Reversed : Orientation.Upright;
            drawn.Add(new DrawnCard(positions[i], shuffled[i], orientation));
        }

        return new DrawResult(spread, allowReversed, drawn);
    }

    internal Card[] Shuffle(IReadOnlyList<Card> source)
    {
        var cards = source.ToArray();
        for (var i = cards.Length - 1; i > 0; i--)
        {
            var j = _random.NextInt(i + 1);
            (cards[i], cards[j]) = (cards[j], cards[i]);
        }

        return cards;
    }
}
