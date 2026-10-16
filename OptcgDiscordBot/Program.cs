using System.Runtime.CompilerServices;
using Discord;
using Discord.WebSocket;
using OptcgPoland.Core.Parsing;
using OptcgPoland.Core.Models;

// Konfiguracja
const string ResultsCategoryName = "WYNIKI TURNIEJÓW";

DiscordSocketConfig config = new DiscordSocketConfig();

config.GatewayIntents =
    GatewayIntents.Guilds |
    GatewayIntents.GuildMessages |
    GatewayIntents.MessageContent;

DiscordSocketClient client = new DiscordSocketClient(config);

string? discordBotToken = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");

client.MessageReceived += HandleMessage;

// Sprawdzenie tokenu
if (!ValidateDiscordBotToken(discordBotToken))
{
    return;
}

// Uruchomienie bota
await client.LoginAsync(TokenType.Bot, discordBotToken);
await client.StartAsync();

// Utrzymanie programu przy życiu
await Task.Delay(Timeout.Infinite);


// Walidacja tokenu
bool ValidateDiscordBotToken(string? token)
{
    if (string.IsNullOrWhiteSpace(token))
    {
        Console.WriteLine("Brak tokenu Discord!");
        return false;
    }

    return true;
}


// Obsługa wiadomości
Task HandleMessage(SocketMessage message)
{
    if (message.Channel is SocketTextChannel textChannel)
    {
        if (textChannel.Category?.Name == ResultsCategoryName)
        {
            TournamentTextParser parser = new TournamentTextParser();
            List<ParsedPlacement> parsedPlacementList = parser.ParseMessage(message.Content);
            foreach (ParsedPlacement result in parsedPlacementList)
            {
                Console.WriteLine($"Miejsce: {result.Place} | Lider: {result.RawText}");
            }
        }
    }

    return Task.CompletedTask;
}

string GetRanking(string line)
{
    string result = "";
    int counterForReadLetters = 0;
    for (int i = 0; i < line.Length ; i ++)
    {
        char character = line[i];
        if (!char.IsDigit(line[i]))
        {
            break;
        }
        result = result + character;
        counterForReadLetters++;
    }
    if (counterForReadLetters < line.Length && line[counterForReadLetters] == '.' && result.Length > 0 )
    {
         return result;
    }
    return "X";
    
}

string GetLeader(string line)
{
    string leaderName = "";
    if ( GetRanking(line) == "X")
    {
        return "-";
    }
    bool findFirstLetter = false;
    for (int i = line.IndexOf('.');i < line.Length; i ++)
    {
        if (char.IsLetter(line[i]))
        {
            findFirstLetter = true;
        }
        if (findFirstLetter)
        {
            leaderName = leaderName + line[i];
        }
        
    }
    return leaderName.Trim();

}