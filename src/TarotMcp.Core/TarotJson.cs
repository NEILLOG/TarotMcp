using System.Text.Encodings.Web;
using System.Text.Json;

namespace TarotMcp.Core;

/// <summary>全專案共用的 JSON 設定：欄位用 snake_case，中文不轉成 \uXXXX。</summary>
public static class TarotJson
{
    public static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);
}
