using System.Reflection;
using System.Text.RegularExpressions;

namespace TarotMcp.Core;

/// <summary>
/// 內嵌在組件裡的牌面素材：每張牌一張插畫（webp，7:12 滿版）與一張共用的邊框範本。
/// 疊圖與文字由使用牌面的程式負責（見 plan/00-master-plan.md 的「牌面圖片」）。
/// </summary>
public static partial class CardAssets
{
    private const string Prefix = "TarotMcp.Assets.";

    private static readonly Assembly Assembly = typeof(CardAssets).Assembly;

    [GeneratedRegex("^[a-z]+_\\d{2}$")]
    private static partial Regex CardIdPattern();

    /// <summary>該牌是否有內嵌的插畫。id 格式不對回 false。</summary>
    public static bool HasCardImage(string cardId) => ResourceName(cardId) is { } name && Exists(name);

    /// <summary>開啟牌的插畫（webp）。沒有或 id 格式不對回 null；呼叫端負責關閉串流。</summary>
    public static Stream? OpenCardImage(string cardId) => ResourceName(cardId) is { } name ? Assembly.GetManifestResourceStream(name) : null;

    /// <summary>開啟共用的邊框範本（png，窗口已去背）。沒有回 null。</summary>
    public static Stream? OpenFrame() => Assembly.GetManifestResourceStream(Prefix + "frame.frame.png");

    /// <summary>開啟邊框範本的窗口資料（frame.json）。沒有回 null。</summary>
    public static Stream? OpenFrameInfo() => Assembly.GetManifestResourceStream(Prefix + "frame.frame.json");

    private static string? ResourceName(string cardId) =>
        CardIdPattern().IsMatch(cardId) ? $"{Prefix}cards.{cardId}.webp" : null;

    private static bool Exists(string name)
    {
        using var stream = Assembly.GetManifestResourceStream(name);
        return stream is not null;
    }
}
