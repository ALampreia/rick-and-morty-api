using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using RickMorty.Domain.Interfaces;
using RickMorty.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RickMorty.Data.Repositories
{
    public class CharacterRepository : ICharacterRepository
    {
        private readonly string _connectionString;

        public CharacterRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }
        public async Task SaveCharactersAndEpisodesAsync(List<Character> characters, List<Episode> episodes)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            foreach (var character in characters)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "INSERT OR IGNORE INTO Characters (Id, Name, Status, Gender, Url) VALUES (@Id, @Name, @Status, @Gender, @Url);";
                cmd.Parameters.AddWithValue("@Id", character.Id);
                cmd.Parameters.AddWithValue("@Name", character.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", character.Status ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Gender", character.Gender ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Url", character.Url ?? (object)DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }

            foreach (var episode in episodes)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "INSERT OR IGNORE INTO Episodes (Id, Name, AirDate, Url) VALUES (@Id, @Name, @AirDate, @Url);";
                cmd.Parameters.AddWithValue("@Id", episode.Id);
                cmd.Parameters.AddWithValue("@Name", episode.Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AirDate", episode.AirDate ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Url", episode.Url ?? (object)DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }

            foreach (var character in characters)
            {
                foreach(var episodesId in character.EpisodesId)
                {
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = "INSERT OR IGNORE INTO CharacterEpisodes (CharacterId, EpisodeId) VALUES (@CharacterId, @EpisodeId);";
                    cmd.Parameters.AddWithValue("@CharacterId", character.Id);
                    cmd.Parameters.AddWithValue("@EpisodeId", episodesId);
                    
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            await transaction.CommitAsync();
        }
        public async Task<List<Character>> GetCharacterByStatusAsync(string status)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Status, Gender, Url FROM Characters WHERE Status = @Status";
            cmd.Parameters.AddWithValue("@Status", status);

            var characters = new List<Character>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                characters.Add(new Character(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    new List<int>()
                    ));
            }
            return characters;
        }
        public async Task<bool> DeleteCharacterWithEpisodes(int characterId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            using (var transaction = await connection.BeginTransactionAsync())
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "DELETE FROM Characters WHERE Id = @Id;";
                cmd.Parameters.AddWithValue("@Id", characterId);
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                
                if(rowsAffected == 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                } 
                await transaction.CommitAsync();
            }
            
            var cleanupCmd = connection.CreateCommand();
            cleanupCmd.CommandText = "DELETE FROM Episodes WHERE Id NOT in (SELECT DISTINCT EpisodeId From CharacterEpisodes);";
            await cleanupCmd.ExecuteNonQueryAsync();
            return true;
        }

    }
}
