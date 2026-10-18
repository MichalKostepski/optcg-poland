
using OptcgPoland.Core.Models;
using Xunit;

namespace OptcgPoland.Tests;

public class TournamentTests
{
    // TEST 1: Konstruktor poprawnie zapisuje dane
    [Fact]
    public void Constructor_ValidData_SetsProperties()
    {
        // Arrange
        List<ParsedPlacement> placements = new List<ParsedPlacement>
        {
            new ParsedPlacement { Place = 1, RawText = "U Crocodile" },
            new ParsedPlacement { Place = 2, RawText = "B Shanks" }
        };

        // Act
        Tournament tournament = new Tournament(
            "krakow",
            "OP17",
            placements
        );

        // Assert
        Assert.Equal("krakow", tournament.ChannelName);
        Assert.Equal("krakow", tournament.City);
        Assert.Equal("OP17", tournament.MetaSetCode);

        Assert.Equal(2, tournament.Placements.Count);
        Assert.Equal(1, tournament.Placements[0].Place);
        Assert.Equal("U Crocodile", tournament.Placements[0].RawText);
        Assert.Equal(2, tournament.Placements[1].Place);
        Assert.Equal("B Shanks", tournament.Placements[1].RawText);
    }

    // TEST 2: Domyślne wartości są poprawne
    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        Tournament tournament = new Tournament(
            "warszawa",
            "OP17",
            new List<ParsedPlacement>()
        );

        // Assert
        Assert.Equal(TournamentFormat.Constructed, tournament.Format);
        Assert.Null(tournament.Date);
        Assert.Null(tournament.StoreName);
    }

    // TEST 3: Konstruktor akceptuje pustą listę
    [Fact]
    public void Constructor_EmptyPlacements_AcceptsEmptyList()
    {
        // Arrange
        List<ParsedPlacement> placements = new List<ParsedPlacement>();

        // Act
        Tournament tournament = new Tournament(
            "rzeszow",
            "EB05",
            placements
        );

        // Assert
        Assert.NotNull(tournament.Placements);
        Assert.Empty(tournament.Placements);
        Assert.Equal("EB05", tournament.MetaSetCode);
    }
}
