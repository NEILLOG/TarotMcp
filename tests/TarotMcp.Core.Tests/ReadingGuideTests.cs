using TarotMcp.Core;
using Xunit;

namespace TarotMcp.Core.Tests;

public class ReadingGuideTests
{
    [Fact]
    public void Short_HasReasonableLength()
    {
        Assert.False(string.IsNullOrWhiteSpace(ReadingGuide.Short));
        Assert.InRange(ReadingGuide.Short.Length, 200, 450);
    }

    [Fact]
    public void Short_KeepsKeyRules()
    {
        Assert.Contains("反思", ReadingGuide.Short);
        Assert.Contains("重抽", ReadingGuide.Short);
    }

    [Fact]
    public void Full_IsServerInstructions()
    {
        Assert.StartsWith("你是塔羅占卜的引導者", ReadingGuide.Full);
        Assert.Contains("list_spreads", ReadingGuide.Full);
        Assert.Contains("draw_cards", ReadingGuide.Full);
    }

    [Fact]
    public void Short_DoesNotMentionServerToolNames()
    {
        // Lumi 的工具名稱不同（tarot_draw），Short 不能提 Server 專用的名稱
        Assert.DoesNotContain("list_spreads", ReadingGuide.Short);
        Assert.DoesNotContain("draw_cards", ReadingGuide.Short);
    }
}
