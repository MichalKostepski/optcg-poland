using System.Runtime.CompilerServices;
using Discord;
using Discord.WebSocket;

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
            string messageContent = message.Content;
            string[] messageContentSplitLines = messageContent.Split('\n');
            foreach (string line in messageContentSplitLines)
            {
                string cleanLine = line.Trim();
                int ranking = GetRanking(cleanLine);
                Console.WriteLine(ranking);
            }
        }
    }

    return Task.CompletedTask;
}
string GetRanking(string line)
{
    string result = "";
    for (int i = 0; i < line.Length ; i ++)
    {
        char character = line[i];
        if (!char.IsDigit(line[i]))
        {
            break;
        }
        result = result + character;
    }
}