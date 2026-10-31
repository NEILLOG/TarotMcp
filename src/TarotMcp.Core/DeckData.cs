using System.Reflection;
using System.Text.Json;
using TarotMcp.Core.Models;

namespace TarotMcp.Core;

/// <summary>資料檔內容不合規則時丟出，訊息會列出所有問題。</summary>
public sealed class DeckValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public DeckValidationException(IReadOnlyList<string> errors)
        : base("牌組資料驗證失敗：\n- " + string.Join("\n- ", errors))
    {
        Errors = errors;
    }
}

/// <summary>78 張牌與全部牌陣，載入後唯讀。</summary>
public sealed class DeckData
{
    public const int ExpectedCardCount = 78;

    private static readonly string[] Suits = ["wands", "cups", "swords", "pentacles"];
    private static readonly string[] Elements = ["火", "水", "風", "土"];

    private readonly Dictionary<string, Card> _cardsById;
    private readonly Dictionary<string, Spread> _spreadsById;

    public IReadOnlyList<Card> Cards { get; }

    public IReadOnlyList<Spread> Spreads { get; }

    private DeckData(IReadOnlyList<Card> cards, IReadOnlyList<Spread> spreads)
    {
        Cards = cards;
        Spreads = spreads;
        _cardsById = cards.ToDictionary(c => c.Id, StringComparer.Ordinal);
        _spreadsById = spreads.ToDictionary(s => s.Id, StringComparer.Ordinal);
    }

    public Card? FindCard(string id) => _cardsById.GetValueOrDefault(id);

    public Spread? FindSpread(string id) => _spreadsById.GetValueOrDefault(id);

    /// <summary>從組件內嵌的 cards.json 與 spreads.json 載入並驗證。</summary>
    public static DeckData LoadEmbedded()
    {
        var assembly = typeof(DeckData).Assembly;
        return Parse(ReadResource(assembly, "TarotMcp.Data.cards.json"), ReadResource(assembly, "TarotMcp.Data.spreads.json"));
    }

    /// <summary>解析並驗證。驗證失敗丟出 <see cref="DeckValidationException"/>。</summary>
    public static DeckData Parse(string cardsJson, string spreadsJson)
    {
        var cards = JsonSerializer.Deserialize<CardsFile>(cardsJson, TarotJson.Options)?.Cards
                    ?? throw new DeckValidationException(["cards.json 沒有 cards 陣列"]);
        var spreads = JsonSerializer.Deserialize<SpreadsFile>(spreadsJson, TarotJson.Options)?.Spreads
                      ?? throw new DeckValidationException(["spreads.json 沒有 spreads 陣列"]);

        var errors = new List<string>();
        ValidateCards(cards, errors);
        ValidateSpreads(spreads, errors);
        if (errors.Count > 0)
        {
            throw new DeckValidationException(errors);
        }

        return new DeckData(cards, spreads);
    }

    private static string ReadResource(Assembly assembly, string name)
    {
        using var stream = assembly.GetManifestResourceStream(name)
                           ?? throw new InvalidOperationException($"找不到內嵌資源 {name}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    internal static void ValidateCards(IReadOnlyList<Card> cards, List<string> errors)
    {
        if (cards.Count != ExpectedCardCount)
        {
            errors.Add($"牌的數量應為 {ExpectedCardCount}，實際為 {cards.Count}");
        }

        foreach (var dup in cards.GroupBy(c => c.Id).Where(g => g.Count() > 1))
        {
            errors.Add($"id 重複：{dup.Key}");
        }

        var majors = cards.Where(c => c.Arcana == "major").ToList();
        if (majors.Count != 22)
        {
            errors.Add($"大牌應有 22 張，實際為 {majors.Count}");
        }

        foreach (var suit in Suits)
        {
            var count = cards.Count(c => c.Arcana == "minor" && c.Suit == suit);
            if (count != 14)
            {
                errors.Add($"花色 {suit} 應有 14 張，實際為 {count}");
            }
        }

        foreach (var c in cards)
        {
            if (string.IsNullOrWhiteSpace(c.MeaningUpright)) errors.Add($"{c.Id} 缺少正位牌義");
            if (string.IsNullOrWhiteSpace(c.MeaningReversed)) errors.Add($"{c.Id} 缺少逆位牌義");
            if (c.KeywordsUpright.Count == 0) errors.Add($"{c.Id} 缺少正位關鍵字");
            if (c.KeywordsReversed.Count == 0) errors.Add($"{c.Id} 缺少逆位關鍵字");
            if (string.IsNullOrWhiteSpace(c.NameZh) || string.IsNullOrWhiteSpace(c.NameEn)) errors.Add($"{c.Id} 缺少名稱");

            var expectedId = c.Arcana == "major"
                ? $"major_{c.Number:00}"
                : $"{c.Suit}_{c.Number:00}";
            if (c.Id != expectedId) errors.Add($"id 與欄位不一致：{c.Id}（依欄位應為 {expectedId}）");

            if (c.Arcana == "major")
            {
                if (c.Suit is not null) errors.Add($"{c.Id} 是大牌，suit 必須為 null");
                if (c.Element is not null) errors.Add($"{c.Id} 是大牌，element 必須為 null");
                if (c.Number is < 0 or > 21) errors.Add($"{c.Id} 的編號必須在 0～21");
            }
            else
            {
                if (c.Element is null || !Elements.Contains(c.Element)) errors.Add($"{c.Id} 的 element 必須是 火／水／風／土");
                if (c.Number is < 1 or > 14) errors.Add($"{c.Id} 的編號必須在 1～14");
            }
        }
    }

    internal static void ValidateSpreads(IReadOnlyList<Spread> spreads, List<string> errors)
    {
        if (spreads.Count == 0)
        {
            errors.Add("至少需要一個牌陣");
        }

        foreach (var dup in spreads.GroupBy(s => s.Id).Where(g => g.Count() > 1))
        {
            errors.Add($"牌陣 id 重複：{dup.Key}");
        }

        foreach (var s in spreads)
        {
            if (s.CardCount != s.Positions.Count)
            {
                errors.Add($"牌陣 {s.Id} 的 card_count（{s.CardCount}）與位置數量（{s.Positions.Count}）不一致");
            }

            if (s.Positions.Count > ExpectedCardCount)
            {
                errors.Add($"牌陣 {s.Id} 的位置多於牌的總數");
            }

            var indexes = s.Positions.Select(p => p.Index).OrderBy(i => i).ToList();
            if (!indexes.SequenceEqual(Enumerable.Range(1, s.Positions.Count)))
            {
                errors.Add($"牌陣 {s.Id} 的位置編號必須是 1 到 {s.Positions.Count} 且不重複");
            }

            foreach (var dup in s.Positions.GroupBy(p => p.Id).Where(g => g.Count() > 1))
            {
                errors.Add($"牌陣 {s.Id} 的位置 id 重複：{dup.Key}");
            }
        }
    }

    private sealed record CardsFile(int Version, IReadOnlyList<Card> Cards);

    private sealed record SpreadsFile(IReadOnlyList<Spread> Spreads);
}
