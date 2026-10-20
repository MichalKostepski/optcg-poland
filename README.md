# OPTCG Poland

### From Discord tournament reports to structured One Piece Card Game data

**OPTCG Poland** is an open-in-development, community-oriented backend project written in **C# and .NET 10**. Its goal is to collect tournament results shared on Discord, turn informal messages into structured records, and eventually provide useful statistics for the Polish **One Piece Card Game (OPTCG)** community.

The long-term vision is a searchable tournament history and a website with metagame insights: which leaders appear most frequently, which decks place well, and how the local competitive scene changes over time.

> **Current status — early backend prototype.** The Discord bot can receive relevant messages, extract numbered placements with a rule-based parser, build in-memory tournament objects, and print their contents to the console. SQLite schema initialization and automated parser/model tests are implemented. **Saving parsed tournament results, statistics, a website, and an AI-powered parser are not implemented yet.**

**Tech:** C# · .NET 10 · Discord.Net · SQLite (`Microsoft.Data.Sqlite`) · xUnit

---

## Why this project?

Local OPTCG tournament reports are often published as ordinary Discord messages. They are useful to read individually, but difficult to aggregate:

- People format placements differently, such as `1. Player / Leader`, `2) Player / Leader`, or `3 Player / Leader`.
- Tournament details are spread across channels and free-form messages.
- Ranking lists can include headings, attendance counts, abbreviations, and inconsistent deck names.
- Without structured data, comparing events or following metagame trends means doing a lot of manual work.

This project tackles the process incrementally. The first milestone is to **identify placement lines reliably without pretending to understand every tournament report**. Later milestones will add persistence, normalization, statistical analysis, and AI-assisted extraction.

## What works today?

| Component | Current implementation | Status |
| --- | --- | --- |
| Discord bot connection | Connects using `Discord.Net` and a token from `DISCORD_BOT_TOKEN` | Implemented |
| Channel filtering | Processes messages in text channels whose Discord category is named exactly `WYNIKI TURNIEJÓW` | Implemented |
| Placement extraction | Recognizes numbered ranking lines and preserves their original text | Implemented |
| Tournament model | Stores channel name, inferred city, metagame set, format, and placements in memory | Implemented |
| SQLite preparation | Creates a `Tournaments` table in `optcg.db` if it does not exist | Implemented |
| Automated tests | xUnit tests cover parser behavior and tournament-model defaults | Implemented |
| Saving received results | Inserting tournaments and their placements into SQLite | **Planned** |
| AI-powered parser | Understanding varied, less structured tournament reports | **Planned** |
| Leader/deck statistics | Aggregations, win rates, historical trends, metagame comparisons | **Planned** |
| Public website / API | Browsing events and visualizing statistics | **Planned** |

**Important:** A created database table does **not** mean incoming tournaments are persisted. At the moment, the Discord message handler prints parsed results to the console.

## Example: turning a Discord message into placements

Imagine someone posts the following message in a tournament-results channel:

```text
Weekly Kraków — 16 players
16 graczy

1. U Crocodile
2) B Shanks
3 P Enel
4. ?
```

The current `TournamentTextParser` examines each line and produces this placement information:

| Place | Preserved text |
| ---: | --- |
| 1 | `U Crocodile` |
| 2 | `B Shanks` |
| 3 | `P Enel` |
| 4 | `?` |

The attendance line `16 graczy` is ignored, as are headings that do not begin with a rank. The unknown `?` entry is intentionally preserved rather than silently inventing a deck or player.

For now, the remainder of each placement is kept in `RawText`. **The project does not yet separate player names from leaders/deck archetypes, validate their identities, or infer missing information.**

### How the current parser works

The parser in `OptcgPoland.Core/Parsing/TournamentTextParser.cs`:

1. Trims each input line and skips empty lines.
2. Requires the line to start with a number.
3. Accepts whitespace, `.`, or `)` immediately after the place number.
4. Extracts the numeric placement and stores the remaining line as raw text.
5. Filters recognized player-count labels such as `graczy`, `zawodników`, `uczestników`, and `osób` (including Polish text normalization).
6. Combines valid lines into a list of `ParsedPlacement` objects.

It is **deterministic and rule-based**, not an AI model. That is intentional for the first iteration: it is simple to debug, cheap to run, and straightforward to unit-test.

## Architecture

The project separates Discord integration, domain logic, database access, and tests:

```text
Discord server
    |
    | New message in "WYNIKI TURNIEJÓW" category
    v
OptcgDiscordBot/Program.cs
    |   - Discord.Net gateway client
    |   - category filter
    |   - message handler
    v
OptcgPoland.Core/Parsing/TournamentTextParser.cs
    |   - parse numbered lines
    |   - produce ParsedPlacement entries
    v
OptcgPoland.Core/Models/Tournament.cs
    |   - create an in-memory Tournament
    v
Console output (current implementation)

OptcgPoland.Data/TournamentRepository.cs
    |
    +--> SQLite schema initialization (implemented)
    +--> inserts / queries / result persistence (planned)

OptcgPoland.Tests/
    +--> parser and domain-model unit tests (xUnit)
```

### Project structure

```text
optcg-poland/
├── OptcgDiscordBot/
│   ├── Program.cs                    # Discord gateway, filtering, console output
│   └── OptcgDiscordBot.csproj
├── OptcgPoland.Core/
│   ├── Models/
│   │   ├── ParsedPlacement.cs        # Place + unprocessed result text
│   │   ├── Tournament.cs             # Tournament domain model
│   │   └── TournamentFormat.cs       # Constructed / Sealed
│   ├── Parsing/
│   │   └── TournamentTextParser.cs   # Deterministic placement parser
│   └── OptcgPoland.Core.csproj
├── OptcgPoland.Data/
│   ├── TournamentRepository.cs       # SQLite schema initialization
│   └── OptcgPoland.Data.csproj
├── OptcgPoland.Tests/
│   ├── TournamentTextParserTests.cs
│   ├── TournamentTests.cs
│   └── OptcgPoland.Tests.csproj
└── README.md
```

The current `Tournament` model derives `City` from the Discord channel name, sets its format to `Constructed` by default, and allows nullable store/date values. These are **initial modeling assumptions**, not comprehensive metadata extraction.

## Run it locally

### Prerequisites

- **.NET 10 SDK** — [download .NET](https://dotnet.microsoft.com/download/dotnet/10.0)
- A Discord application and bot from the [Discord Developer Portal](https://discord.com/developers/applications)
- A test Discord server with a category called **`WYNIKI TURNIEJÓW`** and at least one text channel inside it
- Appropriate bot permissions to view/read the channel, plus **Message Content Intent** enabled in the Discord Developer Portal

The application uses `Guilds`, `GuildMessages`, and `MessageContent` gateway intents. The category name currently has to match exactly; it is set in `OptcgDiscordBot/Program.cs`.

### 1. Clone the project

```powershell
git clone https://github.com/MichalKostepski/optcg-poland.git
cd optcg-poland
```

### 2. Configure the Discord token

In **PowerShell**, set the token for the current terminal session:

```powershell
$env:DISCORD_BOT_TOKEN = Read-Host "Paste your Discord bot token"
```

On Linux/macOS, you can set `DISCORD_BOT_TOKEN` as an environment variable in your shell instead.

Never hard-code the bot token in `Program.cs`, commit it to Git, publish it in screenshots, or include it in an issue. If you accidentally expose it, regenerate it in the Discord Developer Portal.

### 3. Start the bot

From the repository root:

```powershell
dotnet run --project OptcgDiscordBot/OptcgDiscordBot.csproj
```

On startup, the app initializes the SQLite table and connects the bot to Discord. When a new eligible Discord message contains recognizable placements, the program prints a tournament summary and its parsed results to the terminal.

The SQLite file `optcg.db` is created relative to the **process working directory**. Local `*.db` files are excluded by `.gitignore`.

### 4. Run automated tests

```powershell
dotnet test OptcgPoland.Tests/OptcgPoland.Tests.csproj
```

The current xUnit suite tests representative supported ranking formats, ignored lines, multi-line messages, and the initial `Tournament` model properties. These tests do **not** constitute an end-to-end Discord-to-database integration test.

## Planned development

The roadmap below describes **intentions, not features that are already available**.

### Phase 1 — reliable tournament storage

- Add repository methods to save tournament metadata and placements to SQLite.
- Introduce a dedicated placements table and relationships between events and results.
- Handle repeated Discord messages so the same result is not imported twice.
- Store Discord message/channel identifiers to support corrections and deduplication.
- Make the current metagame-set code configurable rather than keeping `op17` in source.

### Phase 2 — richer structured data

- Extract player names, leaders, deck/archetype identifiers, store, date, format, and attendance when provided.
- Normalize spelling, abbreviations and common leader/deck aliases.
- Validate ranks, handle ties and incomplete lists, and flag ambiguous reports instead of guessing.
- Add more unit tests using **anonymized or synthetic** tournament-report examples.

### Phase 3 — **AI-powered tournament parser**

**Planned feature:** add an optional AI-assisted parsing path for Discord reports that are too irregular for the current rules. Rather than just returning a text summary, the AI parser should produce **validated structured output** such as:

```json
{
  "event": {
    "city": "Krakow",
    "date": null,
    "attendance": 16
  },
  "placements": [
    { "place": 1, "leader": "U Crocodile", "player": null },
    { "place": 2, "leader": "B Shanks", "player": null }
  ]
}
```

This is a **proposed output format**, not one currently produced by the bot.

The intended design is a **hybrid parser**:

1. Try the fast, deterministic parser for well-structured results.
2. For unsupported or ambiguous formats, optionally use an AI model to extract fields.
3. Validate the model's output against a strict schema and the original source.
4. Mark uncertain fields as unknown or request review instead of inventing results.
5. Save only validated, approved records; log enough context to trace parsing errors.

Before any real Discord data is sent to an external model, the project would need an appropriate privacy approach, server permission, data-minimization rules, and a clear choice of provider or local model. **There is no AI API, model integration, or associated cost in the current code.**

### Phase 4 — metagame statistics and public interface

- Produce rankings of leaders and deck archetypes across recorded tournaments.
- Compare results by event, city, date range, format, and card set.
- Explore leader representation versus tournament placements, including sample-size caveats.
- Build a read-only API and website with searchable events, leader pages and simple charts.
- Add moderation/correction tools so organizers can fix an incorrectly imported result.

### Phase 5 — robustness and deployment

- Add integration tests for Discord event handling and SQLite persistence.
- Improve error handling, logging, configuration, and recovery after restart.
- Document safe data retention and access policies.
- Package the bot for repeatable deployment once the ingestion pipeline is stable.

## Technical challenges and learning goals

Some of the engineering problems this project is intended to explore:

- **Working with real-world text:** turning inconsistent user-generated ranking lists into predictable domain objects.
- **Event-driven programming:** reacting to Discord gateway events without mixing network concerns with business rules.
- **Separation of concerns:** keeping the Discord adapter, parsing logic, database layer and tests independent.
- **Database modeling:** designing durable relationships between tournaments, players and results.
- **Data quality:** ensuring that invalid or incomplete information never silently becomes an authoritative statistic.
- **AI integration with guardrails:** using structured outputs and validation as an enhancement rather than treating an LLM as a source of truth.

The codebase is intentionally incremental: the implemented parts are small enough to understand and test independently, while the roadmap describes how they could form a complete service.

## Current limitations

- Results are **printed to the console**, not yet stored.
- Only the initial tournament table is created; there is no complete event/placement persistence schema.
- The parser handles numbered lines, **not arbitrary natural language** or full deck-name interpretation.
- It currently accepts some incomplete entries (for example, `4. ?`) as raw text.
- Channel names are used as city names; this can be inaccurate.
- The Discord category filter and metagame set are hard-coded.
- The bot processes newly received messages; it does **not** import Discord channel history.
- No dashboard, ranking system, HTTP API, or AI parser exists yet.
- The project is not presented as a production-grade data collection service.

## Community and attribution

This is an independent, non-commercial **fan/community project** related to the One Piece Card Game. It is not an official product of, endorsed by, or affiliated with the game's rights holders or Discord.

The goal is to build a useful community tool while learning modern C#/.NET backend development. Feedback, reproducible bug reports, and examples of **fictional or appropriately anonymized** tournament-report formats are welcome.

---

**Repository:** [MichalKostepski/optcg-poland](https://github.com/MichalKostepski/optcg-poland)
