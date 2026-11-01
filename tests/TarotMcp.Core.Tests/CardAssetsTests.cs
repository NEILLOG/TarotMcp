using TarotMcp.Core;
using TarotMcp.Core.Randomness;
using Xunit;

namespace TarotMcp.Core.Tests;

public class CardAssetsTests
{
    private static readonly DeckData Deck = DeckData.LoadEmbedded();

    [Fact]
    public void EveryCard_HasEmbeddedWebpImage()
    {
        foreach (var card in Deck.Cards)
        {
            Assert.True(CardAssets.HasCardImage(card.Id), card.Id);

            using var stream = CardAssets.OpenCardImage(card.Id);
            Assert.NotNull(stream);
            var header = new byte[12];
            Assert.Equal(12, stream!.Read(header, 0, 12));
            Assert.Equal("RIFF", System.Text.Encoding.ASCII.GetString(header, 0, 4));
            Assert.Equal("WEBP", System.Text.Encoding.ASCII.GetString(header, 8, 4));
        }
    }

    [Theory]
    [InlineData("nope")]
    [InlineData("../major_00")]
    [InlineData("MAJOR_00")]
    [InlineData("major_0")]
    [InlineData("major_99")]
    public void UnknownOrMalformedId_HasNoImage(string cardId)
    {
        Assert.False(CardAssets.HasCardImage(cardId));
        Assert.Null(CardAssets.OpenCardImage(cardId));
    }

    [Fact]
    public void View_UsesImageLocator_WhenGiven_AndFallsBackWhenItReturnsNull()
    {
        var service = new DrawService(Deck, new SeededRandomSource(1));
        var id = service.Draw("single_card").Cards[0].Card.Id;

        var located = TarotJson.Serialize(new DrawService(Deck, new SeededRandomSource(1)).Draw("single_card").ToView(c => "loc:" + c));
        var fallback = TarotJson.Serialize(new DrawService(Deck, new SeededRandomSource(1)).Draw("single_card").ToView(_ => null));

        Assert.Contains($"\"image\":\"loc:{id}\"", located);
        Assert.Contains($"\"image\":\"cards/{id}.webp\"", fallback);
    }
}
