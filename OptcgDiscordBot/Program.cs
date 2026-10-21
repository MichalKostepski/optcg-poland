using System.Runtime.CompilerServices;
using Discord;
using Discord.WebSocket;
using OptcgPoland.Core.Parsing;
using OptcgPoland.Core.Models;
using OptcgPoland.Data;

// Konfiguracja
const string ResultsCategoryName = "WYNIKI TURNIEJÓW";
const string CurrentMetaSetCode = "op17";

DiscordSocketConfig config = new DiscordSocketConfig();
TournamentTextParser parser = new TournamentTextParser();

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

// Inicjalizacja bazy danych
Console.WriteLine("START INICJALIZACJI BAZY");

TournamentRepository repository = new TournamentRepository();
repository.InitializeDatabase();

if (args.Contains("--test-db"))
{
    List<ParsedPlacement> testResults = new List<ParsedPlacement>
    {
        new ParsedPlacement { Place = 1, RawText = "U Crocodile" },
        new ParsedPlacement { Place = 2, RawText = "B Shanks" },
        new ParsedPlacement { Place = 3, RawText = "P Enel" }
    };

    Tournament testTournament = new Tournament(
        "krakow",
        "OP17",
        testResults
    );

    testTournament.StoreName = "Testowy Sklep";
    testTournament.Date = new DateOnly(2026, 10, 18);

    repository.SaveTournament(testTournament);

    Console.WriteLine("Test zapisu do SQLite zakończony!");
    return;
}

Console.WriteLine("KONIEC INICJALIZACJI BAZY");

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
    if (message.Channel is not SocketTextChannel textChannel)
    {
        return Task.CompletedTask;
    }

    if (textChannel.Category?.Name != ResultsCategoryName)
    {
        return Task.CompletedTask;
    }

    // Odczyt wyników z wiadomości
    List<ParsedPlacement> results = parser.ParseMessage(message.Content);

    if (results.Count == 0)
    {
        return Task.CompletedTask;
    }

    // Utworzenie obiektu turnieju
    Tournament tournament = new Tournament(
        textChannel.Name,
        CurrentMetaSetCode,
        results
    );

    // Wyświetlenie informacji o turnieju
    Console.WriteLine();
    Console.WriteLine("========== TURNIEJ ==========");
    Console.WriteLine($"Miasto: {tournament.City}");
    Console.WriteLine($"Dodatek: {tournament.MetaSetCode}");
    Console.WriteLine($"Format: {tournament.Format}");
    Console.WriteLine($"Liczba wyników: {tournament.Placements.Count}");
    Console.WriteLine();

    // Wyświetlenie wyników
    foreach (ParsedPlacement placement in tournament.Placements)
    {
        Console.WriteLine(
            $"{placement.Place}. {placement.RawText}"
        );
    }

    Console.WriteLine("=============================");
    Console.WriteLine();

    return Task.CompletedTask;
}

