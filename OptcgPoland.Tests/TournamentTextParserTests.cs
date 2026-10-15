namespace OptcgPoland.Tests;
using Xunit;
using OptcgPoland.Core.Parsing;
using OptcgPoland.Core.Models;

public class TestParsingText
{
    [Fact]
    public void Test1()
    {
        TournamentTextParser test = new TournamentTextParser();
        ParsedPlacement? result = test.ParseLine("1. U Crocodile");
        Assert.NotNull(result);
        Assert.Equal(1, result.Place);
        Assert.Equal("U Crcodile", result.RawText);
    }
}

public class ParseLine_ValidInput_ReturnsCorrectPlacement
{
    [Theory]
    [InlineData("1. U Crocodile", 1, "U Crocodile")]
    [InlineData("12) B Shanks", 12,  "B Shanks")]
    [InlineData("3 P Enel",3, "P Enel")]
    [InlineData("9.", 9, "")]
    public void Test2(string line, int expectedPlace, string expectedRawText)
    {
        TournamentTextParser test = new TournamentTextParser();
        ParsedPlacement? result = test.ParseLine(line);
        Assert.NotNull(result);
        Assert.Equal(expectedPlace, result.Place);
        Assert.Equal(expectedRawText, result.RawText);
    }
}


