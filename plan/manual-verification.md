# 人工驗收

## 第一次建置（M0、M1、M2 的前置）

- [ ] `dotnet build` 成功（若套件版本或 MCP SDK 的 API 與程式不符，先修正並更新 `Directory.Packages.props`）
- [ ] `dotnet test` 全部通過
- [ ] `DeckData.LoadEmbedded()` 載入成功（測試 `EmbeddedData_LoadsAndValidates` 通過即代表資料完整）

## M2 MCP server

- [ ] `dotnet publish` 產生單一執行檔
- [ ] 在 Claude Desktop（或 Lumi）的 MCP 設定加入該執行檔，重新啟動後工具列表有 `list_spreads`、`get_spread`、`draw_cards`、`get_card`
- [ ] 請模型「幫我抽一張塔羅牌」：呼叫 `draw_cards`，回傳的牌義與關鍵字與抽到的方向一致
- [ ] 凱爾特十字：10 個位置都有牌，沒有重複
- [ ] `allowReversed=false` 時全部正位
- [ ] 傳入不存在的牌陣 id：回傳錯誤，內容列出可用的牌陣
- [ ] 設定 `TAROT_TEST_SEED=1` 後，兩次相同呼叫得到相同結果；不設定時每次不同
- [ ] **雙擊執行檔**：顯示說明與含實際路徑的設定範例，按任意鍵後關閉，沒有卡住
- [ ] 對話指引：模型會先確認問題、選牌陣、只抽一次、逐張解讀後串成故事，語氣為反思而非預言
- [ ] 問醫療／投資類問題時，模型不做預測並建議諮詢專業人士

## M3 牌義內容

- [ ] 你審稿 78 張中文牌義，並依自己的用字微調
- [ ] 逐一看九張 `review_note` 的牌
- [ ] 確認權杖騎士逆位與聖杯二逆位的對照來源

## M4 牌面圖片

- [ ] 先試產愚者、教皇、聖杯三，確認風格一致後再批次生成
- [ ] 78 張比例一致（7:12）、檔名為 `<id>.webp`、無邊框與文字
