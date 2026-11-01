using TarotMcp.Core;

namespace TarotMcp.Server;

/// <summary>
/// 把內嵌在 exe 裡的牌面插畫在第一次用到時解出到本機資料夾，讓客戶端能用檔案路徑讀到圖片。
/// 資料夾依版本區分（升級後自動換一份）；解出失敗時回 null，工具結果就維持相對路徑，不影響抽牌。
/// </summary>
public sealed class AssetCache
{
    private readonly string _directory;
    private readonly object _gate = new();

    public AssetCache()
    {
        var version = typeof(AssetCache).Assembly.GetName().Version?.ToString() ?? "0";
        _directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TarotMcp",
            "assets-" + version,
            "cards");
    }

    /// <summary>牌的插畫在本機的完整路徑；沒有內嵌或解出失敗回 null。</summary>
    public string? GetCardImagePath(string cardId)
    {
        try
        {
            var path = Path.Combine(_directory, cardId + ".webp");
            lock (_gate)
            {
                if (File.Exists(path))
                    return path;

                using var source = CardAssets.OpenCardImage(cardId);
                if (source is null)
                    return null;

                Directory.CreateDirectory(_directory);
                var temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
                using (var target = File.Create(temp))
                    source.CopyTo(target);
                File.Move(temp, path, overwrite: true);
                return path;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"無法解出牌面圖片 {cardId}：{ex.Message}");   // stderr；stdout 是 MCP 的通道
            return null;
        }
    }
}
