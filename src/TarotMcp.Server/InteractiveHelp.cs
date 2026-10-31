using System.Text.Json;

namespace TarotMcp.Server;

/// <summary>使用者直接雙擊 exe 時顯示的說明與設定範例。</summary>
internal static class InteractiveHelp
{
    public static void Show()
    {
        var exePath = Environment.ProcessPath ?? "TarotMcp.exe";

        // 用 JSON 序列化路徑，Windows 的反斜線會自動跳脫，貼進設定檔可直接使用。
        var config = JsonSerializer.Serialize(
            new { mcpServers = new { tarot = new { command = exePath, args = Array.Empty<string>() } } },
            new JsonSerializerOptions { WriteIndented = true });

        Console.WriteLine("塔羅牌 MCP 伺服器");
        Console.WriteLine("==================");
        Console.WriteLine();
        Console.WriteLine("這個程式是 MCP 伺服器，不能直接開啟使用。");
        Console.WriteLine("請在支援 MCP 的程式（例如 Claude、Lumi）的 MCP 設定中加入下面這段，然後重新啟動該程式：");
        Console.WriteLine();
        Console.WriteLine(config);
        Console.WriteLine();
        Console.WriteLine("設定好之後，直接對 AI 說「幫我抽一張塔羅牌」就可以開始。");
        Console.WriteLine();
        Console.WriteLine("按任意鍵關閉…");

        try
        {
            Console.ReadKey(intercept: true);
        }
        catch (InvalidOperationException)
        {
            // 沒有可用的主控台（例如輸出被導向），直接結束。
        }
    }
}
