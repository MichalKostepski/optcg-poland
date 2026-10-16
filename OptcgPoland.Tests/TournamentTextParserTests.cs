
using Xunit;
using OptcgPoland.Core.Models;
using OptcgPoland.Core.Parsing;
using System.Reflection;

namespace OptcgPoland.Tests;

public class TournamentTextParserTests
{
    [Fact]
    public void ParseLine_SingleValidInput_ReturnsCorrectPlacement()
    {
        TournamentTextParser parser = new TournamentTextParser();

        ParsedPlacement? result = parser.ParseLine("1. U Crocodile");

        Assert.NotNull(result);
        Assert.Equal(1, result.Place);
        Assert.Equal("U Crocodile", result.RawText);
    }

    [Theory]
    [InlineData("1. U Crocodile", 1, "U Crocodile")]
    [InlineData("12) B Shanks", 12, "B Shanks")]
    [InlineData("3 P Enel", 3, "P Enel")]
    [InlineData("9.", 9, "")]
    public void ParseLine_ValidInput_ReturnsCorrectPlacement(
        string line,
        int expectedPlace,
        string expectedRawText)
    {
        TournamentTextParser parser = new TournamentTextParser();

        ParsedPlacement? result = parser.ParseLine(line);

        Assert.NotNull(result);
        Assert.Equal(expectedPlace, result.Place);
        Assert.Equal(expectedRawText, result.RawText);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("Turniej Kraków")]
    [InlineData("12Shanks")]
    [InlineData("123")]
    [InlineData("16 graczy")]
    public void ParseLine_InvalidInput_ReturnsNull(string line)
    {
        TournamentTextParser parser = new TournamentTextParser();

        ParsedPlacement? result = parser.ParseLine(line);

        Assert.Null(result);
    }
    
    [Fact]
    public void ParseMessage_MultiplePlacements_ReturnsCorrectList()
    {
        // Arrange - przygotowanie danych
        TournamentTextParser parser = new TournamentTextParser();

        string message = """
            Weekly Kraków
            16 graczy

            1. U Crocodile
            2) B Shanks
            3 P Enel
            4. ?
            """;

        // Act - uruchomienie testowanej metody
        List<ParsedPlacement> results = parser.ParseMessage(message);

        // Assert - sprawdzenie wyników
        Assert.Equal(4, results.Count);

        Assert.Equal(1, results[0].Place);
        Assert.Equal("U Crocodile", results[0].RawText);

        Assert.Equal(4, results[3].Place);
        Assert.Equal("?", results[3].RawText);
        }

}
