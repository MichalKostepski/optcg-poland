
using System.Globalization;
using System.Text;
using OptcgPoland.Core.Models;

namespace OptcgPoland.Core.Parsing;

public class TournamentTextParser
{
    public ParsedPlacement? ParseLine(string line)
    {
        string cleanedLine = line.Trim();

        // Sprawdzenie, czy linia jest pusta
        if (cleanedLine.Length == 0)
        {
            return null;
        }

        // Sprawdzenie, czy linia zaczyna się od cyfry
        if (!char.IsDigit(cleanedLine[0]))
        {
            return null;
        }

        // Obliczenie długości numeru miejsca
        int numberLength = 0;

        for (int i = 0; i < cleanedLine.Length; i++)
        {
            if (!char.IsDigit(cleanedLine[i]))
            {
                break;
            }

            numberLength++;
        }

        // Sprawdzenie separatora po numerze miejsca
        if (numberLength >= cleanedLine.Length ||
            !(char.IsWhiteSpace(cleanedLine[numberLength]) ||
              cleanedLine[numberLength] == '.' ||
              cleanedLine[numberLength] == ')'))
        {
            return null;
        }

        // Pobranie i konwersja numeru miejsca
        string rankingText = cleanedLine.Substring(0, numberLength);

        if (!int.TryParse(rankingText, out int place))
        {
            return null;
        }

        // Pobranie tekstu po numerze miejsca
        string rawText = cleanedLine.Substring(numberLength + 1).Trim();

        // Odrzucenie informacji o liczbie uczestników
        if (IsPlayerCount(rawText))
        {
            return null;
        }

        // Utworzenie wyniku parsowania
        ParsedPlacement result = new ParsedPlacement();

        result.Place = place;
        result.RawText = rawText;

        return result;
    }

    public List<ParsedPlacement> ParseMessage  (string message)
    {
        List<ParsedPlacement> parsedPlacementList = new List<ParsedPlacement>();
        string[] messageLines = message.Split('\n');
        foreach(string line in messageLines)
        {
            ParsedPlacement? parsedPlacement = ParseLine(line);
            if (parsedPlacement is not null)
            {
                parsedPlacementList.Add(parsedPlacement);
            }
        }
        return parsedPlacementList;
    }

    private static bool IsPlayerCount(string text)
    {
        string normalizedText = NormalizeText(text);

        string[] playerCountKeywords =
        {
            "graczy",
            "zawodnikow",
            "uczestnikow",
            "osob"
        };

        foreach (string keyword in playerCountKeywords)
        {
            if (normalizedText == keyword)
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeText(string text)
    {
        // Zamiana na małe litery i normalizacja Unicode
        string normalizedText = text
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        StringBuilder result = new StringBuilder();

        // Usunięcie znaków diakrytycznych
        foreach (char character in normalizedText)
        {
            UnicodeCategory category =
                CharUnicodeInfo.GetUnicodeCategory(character);

            if (category != UnicodeCategory.NonSpacingMark)
            {
                result.Append(character);
            }
        }

        return result.ToString().Normalize(NormalizationForm.FormC);
    }
}
