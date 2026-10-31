using TarotMcp.Core.Models;

namespace TarotMcp.Core;

// 給模型與客戶端看的精簡檢視。欄位用 snake_case 序列化（見 TarotJson）。
// 抽牌結果只帶「當下方向」的牌義與關鍵字，不把正逆位兩份都回傳。

public sealed record SpreadSummaryView(string Id, string NameZh, int CardCount, string UseFor);

public sealed record PositionView(int Index, string Id, string NameZh, string MeaningZh, SpreadLayout Layout);

public sealed record SpreadView(string Id, string NameZh, int CardCount, string UseFor, IReadOnlyList<PositionView> Positions);

public sealed record DrawnCardView(
    string Id,
    string NameZh,
    string NameEn,
    string Orientation,
    string OrientationZh,
    string? Element,
    IReadOnlyList<string> Keywords,
    string Meaning,
    string Image);

public sealed record DrawnPositionView(
    int Index,
    string PositionId,
    string PositionName,
    string PositionMeaning,
    SpreadLayout Layout,
    DrawnCardView Card);

public sealed record DrawResultView(string SpreadId, string SpreadName, bool AllowReversed, IReadOnlyList<DrawnPositionView> Positions);

public sealed record CardDetailView(
    string Id,
    string NameZh,
    string NameEn,
    string Arcana,
    string? Suit,
    int Number,
    string? Element,
    IReadOnlyList<string> KeywordsUpright,
    IReadOnlyList<string> KeywordsReversed,
    string MeaningUpright,
    string MeaningReversed,
    string Image);

public static class ViewMapper
{
    public static SpreadSummaryView ToSummary(this Spread s) => new(s.Id, s.NameZh, s.CardCount, s.UseFor);

    public static SpreadView ToView(this Spread s) => new(
        s.Id,
        s.NameZh,
        s.CardCount,
        s.UseFor,
        s.Positions.OrderBy(p => p.Index).Select(p => new PositionView(p.Index, p.Id, p.NameZh, p.MeaningZh, p.Layout)).ToList());

    public static DrawResultView ToView(this DrawResult r) => new(
        r.Spread.Id,
        r.Spread.NameZh,
        r.AllowReversed,
        r.Cards.Select(d => new DrawnPositionView(
            d.Position.Index,
            d.Position.Id,
            d.Position.NameZh,
            d.Position.MeaningZh,
            d.Position.Layout,
            new DrawnCardView(
                d.Card.Id,
                d.Card.NameZh,
                d.Card.NameEn,
                d.Orientation.ToWireName(),
                d.Orientation.ToChinese(),
                d.Card.Element,
                d.Keywords,
                d.Meaning,
                d.Card.Image))).ToList());

    public static CardDetailView ToDetailView(this Card c) => new(
        c.Id,
        c.NameZh,
        c.NameEn,
        c.Arcana,
        c.Suit,
        c.Number,
        c.Element,
        c.KeywordsUpright,
        c.KeywordsReversed,
        c.MeaningUpright,
        c.MeaningReversed,
        c.Image);
}
