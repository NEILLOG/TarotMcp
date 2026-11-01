using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TarotMcp.Core;
using TarotMcp.Core.Randomness;
using TarotMcp.Server;

// 這是 MCP 的 stdio 伺服器，不是給人直接開的程式。
// 被客戶端（Claude、Lumi…）啟動時，標準輸入是被導向的；使用者雙擊時不是，這時顯示說明後離開，不要卡住。
if (!Console.IsInputRedirected)
{
    InteractiveHelp.Show();
    return 0;
}

var builder = Host.CreateApplicationBuilder(args);

// stdout 是 MCP 的傳輸通道，所有記錄一律寫到 stderr。
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

DeckData deck;
try
{
    deck = DeckData.LoadEmbedded();
}
catch (DeckValidationException ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

// 測試模式：設定環境變數 TAROT_TEST_SEED（整數）會使用固定亂數，結果可重現。正式使用請不要設定。
IRandomSource random = int.TryParse(Environment.GetEnvironmentVariable("TAROT_TEST_SEED"), out var seed)
    ? new SeededRandomSource(seed)
    : new SecureRandomSource();

builder.Services.AddSingleton(deck);
builder.Services.AddSingleton(random);
builder.Services.AddSingleton<DrawService>();
builder.Services.AddSingleton<AssetCache>();

builder.Services
    .AddMcpServer(options => options.ServerInstructions = Instructions.Text)
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
return 0;
