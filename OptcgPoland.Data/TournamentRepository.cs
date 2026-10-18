
using Microsoft.Data.Sqlite;

namespace OptcgPoland.Data;

public class TournamentRepository
{
    private readonly string _connectionString =
        "Data Source=optcg.db;Foreign Keys=True";

    public void InitializeDatabase()
    {
        using SqliteConnection connection =
            new SqliteConnection(_connectionString);

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

        using SqliteCommand command =
            new SqliteCommand(sql, connection);

        command.ExecuteNonQuery();

        Console.WriteLine("Baza danych została zainicjalizowana!");
    }
}
