
namespace OptcgPoland.Core.Models;

public class Tournament
{
    public string ChannelName { get; set; }
    public string City { get; set; }
    public string? StoreName { get; set; }
    public string MetaSetCode { get; set; }
    public DateOnly? Date { get; set; }

    public List<ParsedPlacement> Placements { get; set; }

    public TournamentFormat Format { get; set; } =
        TournamentFormat.Constructed;

    public Tournament(
        string channelName,
        string metaSetCode,
        List<ParsedPlacement> placements)
    {
        ChannelName = channelName;
        City = channelName;
        MetaSetCode = metaSetCode;
        Placements = placements;
    }
}
