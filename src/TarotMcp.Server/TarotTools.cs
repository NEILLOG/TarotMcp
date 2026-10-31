using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TarotMcp.Core;

namespace TarotMcp.Server;

/// <summary>
/// MCP 工具外殼。這裡只負責參數與錯誤轉換，邏輯都在 TarotMcp.Core。
/// 工具回傳 JSON 字串（欄位為 snake_case）。
/// </summary>
[McpServerToolType]
public sealed class TarotTools
{
    private readonly DeckData _deck;
    private readonly DrawService _draw;

    public TarotTools(DeckData deck, DrawService draw)
    {
        _deck = deck;
        _draw = draw;
    }

    [McpServerTool(Name = "list_spreads", ReadOnly = true)]
    [Description("列出可用的塔羅牌陣（id、名稱、張數、適用情境）。不確定該用哪個牌陣時先呼叫這個工具。")]
    public string ListSpreads() => TarotJson.Serialize(_deck.Spreads.Select(s => s.ToSummary()).ToList());

    [McpServerTool(Name = "get_spread", ReadOnly = true)]
    [Description("取得單一牌陣的每個位置名稱與含義，以及畫面擺放座標（layout）。")]
    public string GetSpread([Description("牌陣 id，例如 single_card、three_card_timeline、celtic_cross")] string spreadId)
    {
        var spread = _deck.FindSpread(spreadId)
                     ?? throw new McpException(new UnknownSpreadException(spreadId, _deck.Spreads.Select(s => s.Id)).Message);
        return TarotJson.Serialize(spread.ToView());
    }

    [McpServerTool(Name = "draw_cards", ReadOnly = false, Idempotent = false)]
    [Description(
        "洗牌並依牌陣抽牌（不放回）。每次呼叫都是全新的一次隨機抽取。" +
        "回傳每個位置的牌、正逆位，以及當下方向的關鍵字與牌義。牌義是反思視角，僅供娛樂與自我探索，不做絕對性預言；" +
        "醫療、法律、投資、人身安全的問題請勿據此下結論。抽牌後請依位置逐張解讀並串成一段完整的說明；不要因結果不理想而重抽。")]
    public string DrawCards(
        [Description("牌陣 id，例如 single_card、three_card_timeline、celtic_cross")] string spreadId,
        [Description("是否允許逆位，預設 true。不使用逆位的流派請設為 false")] bool allowReversed = true)
    {
        try
        {
            return TarotJson.Serialize(_draw.Draw(spreadId, allowReversed).ToView());
        }
        catch (UnknownSpreadException ex)
        {
            throw new McpException(ex.Message);
        }
    }

    [McpServerTool(Name = "get_card", ReadOnly = true)]
    [Description("取得單張牌的完整資料（正逆位關鍵字與牌義、花色、元素）。用於使用者追問某張牌時，不涉及抽牌。")]
    public string GetCard([Description("牌的 id，例如 major_00（愚者）、wands_01（權杖王牌）、swords_13（寶劍皇后）")] string cardId)
    {
        var card = _deck.FindCard(cardId) ?? throw new McpException(new UnknownCardException(cardId).Message);
        return TarotJson.Serialize(card.ToDetailView());
    }
}
