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

到 GitHub 的 Releases 頁面下載對應平台的 zip（`TarotMcp-<版本>-win-x64.zip` 或 `TarotMcp-<版本>-osx-arm64.zip`），解開後只有一個執行檔：`TarotMcp.exe`（Windows）或 `TarotMcp`（macOS）。檔名固定，更新版本時覆蓋同一個路徑，客戶端設定不用改。

macOS 從瀏覽器下載的檔案會被加上隔離屬性，第一次要先解除：`xattr -d com.apple.quarantine TarotMcp`。

也可以自己發佈（見下方「發版」）。把執行檔的路徑加進客戶端設定（以 Claude Desktop 格式為例）：

```json
{
  "mcpServers": {
    "tarot": { "command": "C:\\path\\to\\TarotMcp.exe" }
  }
}
```

直接雙擊執行檔不會卡住：它會顯示說明與含實際路徑的設定範例，然後等按鍵關閉。

## 發版

```bash
scripts/release.sh 1.0.0        # 在乾淨且已同步的 main 上打 tag v1.0.0 並推送
scripts/release.sh 1.1.0-rc1    # 版本號含 "-" 是預覽版：Release 標成 prerelease
```

推送 `v*` tag 後，`.github/workflows/release.yml` 會自動：跑測試 → 用 tag 的版本號發佈 Windows x64 與 macOS Apple Silicon（單一檔案、自包含）→ 建立 GitHub Release，附上兩個 zip。也可以在 Actions 頁面手動執行，輸入版本號。

- 發佈說明：有 `docs/release-notes/v<版本>.md` 就用它，沒有就用自動產生的變更清單。
- 本機試打包（不推送、不建立 Release）：`scripts/publish-release.sh --version 0.0.0-local`，產物在 `artifacts/release/`（已被 `.gitignore` 忽略）。
- 版本號會寫進程式（MCP 握手回報的 `serverInfo.version`）；沒有指定版本時，腳本用最近的 tag，再沒有就用 `TarotMcp.Server.csproj` 的 `<Version>`。

## 牌面圖片

78 張插畫（`assets/cards/*.webp`）與共用邊框範本（`assets/frame/`）會**內嵌進組件**，單一檔案發佈時就在 exe 裡，不需要另外帶檔案（代價是 exe 約多 45 MB）。

- `TarotMcp.Core.CardAssets` 可以直接讀內嵌的插畫與邊框（給 Lumi 這類自己疊圖的程式用）。
- MCP server 第一次用到某張牌時，把它的插畫解出到 `%LOCALAPPDATA%/TarotMcp/assets-<版本>/cards/`（macOS／Linux 是對應的本機資料夾），工具結果的 `image` 欄位就是這個**完整檔案路徑**；解出失敗時維持相對路徑 `cards/<id>.webp`，不影響抽牌。
- 疊圖（插畫、邊框、文字）由使用牌面的程式負責；MCP server 本身只回傳素材位置。

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
