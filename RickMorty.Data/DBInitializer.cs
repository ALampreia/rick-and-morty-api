using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RickMorty.Data
{
    public static class DBInitializer
    {
        public static void InitializeDC(IConfiguration config)
        {
            var _connectionString = config.GetConnectionString("DefaultConnection");

            using var con = new SqliteConnection(_connectionString);
            con.Open();

            var command = new[]
            {
                @"CREATE TABLE IF NOT EXISTS Characters (
                    Id INTEGER PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Status TEXT NOT NULL,
                    Gender TEXT,
                    Url TEXT   
                );",
                @"CREATE TABLE IF NOT EXISTS Episodes (
                        Id INTEGER PRIMARY KEY,
                        Name TEXT NOT NULL,
                        AirDate TEXT,
                        EpisodeCode TEXT,
                        Url TEXT
                );",
                @"CREATE TABLE IF NOT EXISTS CharacterEpisodes (
                        CharacterId INTEGER,
                        EpisodeId INTEGER,
                        PRIMARY KEY (CharacterId, EpisodeId),
                        FOREIGN KEY (CharacterId) REFERENCES Characters(Id) ON DELETE CASCADE,
                        FOREIGN KEY (EpisodeId) REFERENCES Episodes(Id) ON DELETE CASCADE
                );"
            };

            foreach (var cmdTxt in command)
            {
                var cmd = con.CreateCommand();
                cmd.CommandText = cmdTxt;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
