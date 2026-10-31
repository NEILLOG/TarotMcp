using System.Security.Cryptography;

namespace TarotMcp.Core.Randomness;

/// <summary>
/// 亂數來源。抽牌邏輯只依賴這個介面，測試時可以換成固定序列。
/// </summary>
public interface IRandomSource
{
    /// <summary>回傳 [0, toExclusive) 之間均勻分布的整數。<paramref name="toExclusive"/> 必須大於 0。</summary>
    int NextInt(int toExclusive);
}

/// <summary>正式使用的亂數來源，使用作業系統的密碼學安全亂數。</summary>
public sealed class SecureRandomSource : IRandomSource
{
    public int NextInt(int toExclusive)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(toExclusive, 0);
        return RandomNumberGenerator.GetInt32(toExclusive);
    }
}

/// <summary>固定種子的亂數來源，只用於測試與除錯。<b>不得</b>在正式環境啟用。</summary>
public sealed class SeededRandomSource : IRandomSource
{
    private readonly Random _random;

    public SeededRandomSource(int seed) => _random = new Random(seed);

    public int NextInt(int toExclusive)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(toExclusive, 0);
        return _random.Next(toExclusive);
    }
}
