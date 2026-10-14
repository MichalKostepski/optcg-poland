using OptcgPoland.Core.Models;

namespace OptcgPoland.Core.Parsing;


public class TournamentTextParser
{
    public ParsedPlacement? ParseLine(string line)
    {
        string cleanedLine = line.Trim();
        if (cleanedLine.Length == 0)
        {
            return null;
        }
        if (!char.IsDigit(cleanedLine[0]))
        {
            return null;
        }

        int howLongNumber = 0;
        for (int i = 0; i < cleanedLine.Length; i++)
        {
            if (!char.IsDigit(cleanedLine[i]))
            {
                break;
            }
            howLongNumber++;
        }

        if (howLongNumber >= cleanedLine.Length ||
            !(char.IsWhiteSpace(cleanedLine[howLongNumber]) || 
            cleanedLine[howLongNumber] == '.' || cleanedLine[howLongNumber] == ')'))
        {
            return null;
        }

        string ranking = cleanedLine.Substring(0, howLongNumber);
        bool succes = int.TryParse(ranking, out int intRanking);
        if (!succes)
        {
            return null;
        }
        string rawText = cleanedLine.Substring(howLongNumber + 1);
        string rawTextCleaned = rawText.Trim();

        ParsedPlacement result = new ParsedPlacement();
        result.Place = intRanking;
        result.RawText = rawTextCleaned;

        return result;
    }
}