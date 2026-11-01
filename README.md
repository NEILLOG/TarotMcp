# TarotMcp

塔羅牌 MCP server：負責**洗牌、抽牌、依牌陣擺放**並回傳牌義；解讀由模型負責。
採用萊德-韋特-史密斯（Rider–Waite–Smith）78 張牌組。

> **狀態（2026-10-31）**：程式在沒有 .NET SDK 的環境寫成，只做過語法檢查，**尚未實際編譯、執行**。
> 第一次請先執行 `dotnet build` 與 `dotnet test`，套件版本（`Directory.Packages.props`）也請確認並更新到最新穩定版。
> 進度與待辦見 [`plan/00-master-plan.md`](plan/00-master-plan.md)。

## 它做什麼、不做什麼

| 做 | 不做 |
|---|---|
| 密碼學安全的洗牌與不放回抽牌 | 解讀牌義、跟使用者對話（模型負責） |
| 決定正逆位（可關閉） | 接收使用者的問題（問題留在對話裡） |
| 依牌陣填入位置，回傳牌義、關鍵字與畫面座標 | 生成或繪製圖片 |

## 專案結構

```
src/TarotMcp.Core/      核心：資料載入與驗證、洗牌、抽牌（不依賴 MCP）
src/TarotMcp.Server/    MCP server（stdio），只是核心外面的一層薄殼
tests/                  單元測試（xunit）
plan/                   計畫與規格
assets/cards/           牌面插畫（<id>.webp，由其他工具生成）
assets/frame/           共用的華麗邊框範本（frame.png＋frame.json，由其他工具生成）
```

核心庫與 MCP 外殼分開，之後 Lumi 可以直接引用 `TarotMcp.Core` 做成內建工具。

## 建置與測試

需要 .NET 10 SDK。

```bash
dotnet build
dotnet test
```

## 在 MCP 客戶端使用

先發佈成單一執行檔：

```bash
dotnet publish src/TarotMcp.Server -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

再把路徑加進客戶端設定（以 Claude Desktop 格式為例）：

```json
{
  "mcpServers": {
    "tarot": { "command": "C:\\path\\to\\TarotMcp.exe" }
  }
}
```

直接雙擊執行檔不會卡住：它會顯示說明與含實際路徑的設定範例，然後等按鍵關閉。

## 工具

| 工具 | 參數 | 說明 |
|---|---|---|
| `list_spreads` | — | 列出牌陣 |
| `get_spread` | `spreadId` | 牌陣每個位置的名稱、含義與畫面座標 |
| `draw_cards` | `spreadId`、`allowReversed`（預設 true） | 洗牌並抽牌 |
| `get_card` | `cardId` | 單張牌完整資料 |

牌陣：`single_card`、`three_card_timeline`、`celtic_cross`（依韋特原書）。

## 測試模式

設定環境變數 `TAROT_TEST_SEED=<整數>` 會改用固定亂數，結果可重現。**正式使用請不要設定。**

## 資料與授權

- 牌義（`src/TarotMcp.Core/Data/cards.json`）是依 A. E. Waite《The Pictorial Key to the Tarot》（1910/1911）與常見的現代解讀改寫的原創繁體中文；韋特英文原文保留在每張牌的 `source` 欄位。
- 韋特原文的著作權狀態是依作者逝世年推斷，**正式發布前請自行確認**。
- 授權條款尚未決定。
- 牌面插畫與邊框範本由其他 AI 工具生成，各工具的商用條款不同，要販售前請先確認。
