
using Microsoft.Data.Sqlite;
using OptcgPoland.Core.Models;

namespace OptcgPoland.Data;

public class TournamentRepository
{
    private readonly string _connectionString =
        "Data Source=optcg.db;Foreign Keys=True";

    public void InitializeDatabase()
    {
        using SqliteConnection connection = new SqliteConnection(_connectionString);

        connection.Open();

        string sql = """
            CREATE TABLE IF NOT EXISTS Tournaments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ChannelName TEXT NOT NULL,
                City TEXT NOT NULL,
                StoreName TEXT,
                MetaSetCode TEXT NOT NULL,
                Date TEXT,
                Format TEXT NOT NULL
            );
            """;

        using SqliteCommand command = new SqliteCommand(sql, connection);

        command.ExecuteNonQuery();
        
    string placementsSql = """
        CREATE TABLE IF NOT EXISTS Placements (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            TournamentId INTEGER NOT NULL,
            Place INTEGER NOT NULL,
            RawText TEXT,
            FOREIGN KEY (TournamentId)
                REFERENCES Tournaments(Id)
                ON DELETE CASCADE
        );
        """;

    using SqliteCommand placementsCommand = new SqliteCommand(placementsSql, connection);

    placementsCommand.ExecuteNonQuery();


        Console.WriteLine("Baza danych została zainicjalizowana!");
    }

    
    public void SaveTournament(Tournament tournament)
    {
        using SqliteConnection connection =
            new SqliteConnection(_connectionString);

        connection.Open();

        // Rozpoczęcie transakcji
        using SqliteTransaction transaction = connection.BeginTransaction();

        // 1. Zapisanie turnieju
        using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = """
            INSERT INTO Tournaments
            (ChannelName, City, StoreName, MetaSetCode, Date, Format)
            VALUES
            ($channelName, $city, $storeName, $metaSetCode, $date, $format);
            """;

        command.Parameters.AddWithValue("$channelName", tournament.ChannelName);
        command.Parameters.AddWithValue("$city", tournament.City);
        command.Parameters.AddWithValue("$metaSetCode", tournament.MetaSetCode);
        command.Parameters.AddWithValue("$format", tournament.Format.ToString());

        command.Parameters.AddWithValue(
            "$storeName",
            (object?)tournament.StoreName ?? DBNull.Value
        );

        command.Parameters.AddWithValue(
            "$date",
            tournament.Date.HasValue
                ? tournament.Date.Value.ToString("yyyy-MM-dd")
                : DBNull.Value
        );

        command.ExecuteNonQuery();

        // 2. Pobranie ID zapisanego turnieju
        using SqliteCommand idCommand = connection.CreateCommand();
        idCommand.Transaction = transaction;
        idCommand.CommandText = "SELECT last_insert_rowid();";

        long tournamentId = (long)idCommand.ExecuteScalar()!;

        // 3. Zapisanie wszystkich wyników turnieju
        foreach (ParsedPlacement line in tournament.Placements)
        {
            using SqliteCommand placementCommand = connection.CreateCommand();
            placementCommand.Transaction = transaction;

            placementCommand.CommandText = """
                INSERT INTO Placements (TournamentId, Place, RawText)
                VALUES ($tournamentId, $place, $rawText);
                """;

            placementCommand.Parameters.AddWithValue("$tournamentId", tournamentId);
            placementCommand.Parameters.AddWithValue("$place", line.Place);
            placementCommand.Parameters.AddWithValue(
                "$rawText",
                (object?)line.RawText ?? DBNull.Value
            );

            placementCommand.ExecuteNonQuery();
        }

        // 4. Zatwierdzenie wszystkich zmian
        transaction.Commit();

        Console.WriteLine($"Zapisano turniej ID: {tournamentId}");
    }

    
public List<Tournament> GetTournaments()
    {
        List<Tournament> tournaments = new List<Tournament>();

        Dictionary<long, Tournament> tournamentsById =
            new Dictionary<long, Tournament>();

        using SqliteConnection connection =
            new SqliteConnection(_connectionString);

        connection.Open();

        // 1. Pobieranie turniejów
        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT Id, ChannelName, City, StoreName,
                    MetaSetCode, Date, Format
                FROM Tournaments
                ORDER BY Id;
                """;

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                long id = reader.GetInt64(reader.GetOrdinal("Id"));

                string channelName =
                    reader.GetString(reader.GetOrdinal("ChannelName"));

                string metaSetCode =
                    reader.GetString(reader.GetOrdinal("MetaSetCode"));

                Tournament tournament = new Tournament(
                    channelName,
                    metaSetCode,
                    new List<ParsedPlacement>()
                );

                tournament.City =
                    reader.GetString(reader.GetOrdinal("City"));

                int storeColumn = reader.GetOrdinal("StoreName");

                tournament.StoreName = reader.IsDBNull(storeColumn)
                    ? null
                    : reader.GetString(storeColumn);

                int dateColumn = reader.GetOrdinal("Date");

                if (!reader.IsDBNull(dateColumn))
                {
                    string dateText = reader.GetString(dateColumn);

                    tournament.Date = DateOnly.ParseExact(
                        dateText,
                        "yyyy-MM-dd"
                    );
                }

                string formatText =
                    reader.GetString(reader.GetOrdinal("Format"));

                tournament.Format =
                    Enum.Parse<TournamentFormat>(formatText);

                tournaments.Add(tournament);
                tournamentsById.Add(id, tournament);
            }
        }

        // 2. Pobieranie wyników turniejów
        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT TournamentId, Place, RawText
                FROM Placements
                ORDER BY TournamentId, Place;
                """;

            using SqliteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                long tournamentId =
                    reader.GetInt64(reader.GetOrdinal("TournamentId"));

                int place =
                    reader.GetInt32(reader.GetOrdinal("Place"));

                int rawTextColumn = reader.GetOrdinal("RawText");

                string? rawText = reader.IsDBNull(rawTextColumn)
                    ? null
                    : reader.GetString(rawTextColumn);

                ParsedPlacement placement = new ParsedPlacement
                {
                    Place = place,
                    RawText = rawText
                };

                Tournament tournament = tournamentsById[tournamentId];

                tournament.Placements.Add(placement);
            }
        }

        return tournaments;
    }

}
