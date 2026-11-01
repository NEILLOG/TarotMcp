namespace TarotMcp.Core.Models;

/// <summary>牌義的英文出處（韋特原文，保留供追溯與審稿）。</summary>
public sealed record CardSource(string Work, string WaiteUprightEn, string WaiteReversedEn);

/// <summary>一張牌的完整資料。對應 cards.json 的一筆。</summary>
public sealed record Card(
    string Id,
    string NameZh,
    string NameEn,
    string Arcana,                       // "major" | "minor"
    string? Suit,                        // wands | cups | swords | pentacles；大牌為 null
    int Number,
    string? Element,                     // 小牌：火／水／風／土；大牌為 null
    IReadOnlyList<string> KeywordsUpright,
    IReadOnlyList<string> KeywordsReversed,
    string MeaningUpright,
    string MeaningReversed,
    string Image,                        // 例如 cards/major_00.webp
    string ImagePrompt,
    CardSource Source,
    string? ReviewNote);

/// <summary>牌陣位置在畫面上的擺放。x 以牌寬、y 以牌高為單位（牌高 = 牌寬 × 12/7），(0,0) 為牌陣中心，y 向下為正。</summary>
public sealed record SpreadLayout(double X, double Y, double Rotation, int Z);

public sealed record SpreadPosition(
    int Index,
    string Id,
    string NameZh,
    string? NameEn,
    string MeaningZh,
    SpreadLayout Layout);

public sealed record Spread(
    string Id,
    string NameZh,
    string? NameEn,
    int CardCount,
    string UseFor,
    string Source,
    IReadOnlyList<SpreadPosition> Positions);

public enum Orientation
{
    Upright,
    Reversed,
}

public static class OrientationExtensions
{
    public static string ToWireName(this Orientation o) => o == Orientation.Reversed ? "reversed" : "upright";

    public static string ToChinese(this Orientation o) => o == Orientation.Reversed ? "逆位" : "正位";
}
