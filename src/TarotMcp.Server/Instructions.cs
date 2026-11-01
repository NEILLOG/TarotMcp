using TarotMcp.Core;

namespace TarotMcp.Server;

/// <summary>交給 MCP 客戶端的伺服器指引（initialize 回應的 instructions）。內容在 <see cref="ReadingGuide.Full"/>，與 Lumi 共用。</summary>
internal static class Instructions
{
    public const string Text = ReadingGuide.Full;
}
