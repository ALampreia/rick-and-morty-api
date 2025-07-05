using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RickAndMorty.WebAPI.Dtos;
using RickMorty.Domain.Interfaces;
using RickMorty.Domain.Model;
using System.Text.Json;

namespace RickAndMorty.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RickMortyController : ControllerBase
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly HttpClient _httpClient;

        public RickMortyController(ICharacterRepository characterRepository)
        {
            _characterRepository = characterRepository;
            _httpClient = new HttpClient();
        }

        [HttpGet("characters/status/{status}")]
        public async Task<IActionResult> GetCharacterbyStatus(string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return BadRequest("Invalid status.");
            }

            var allowedStatuses = new[] { "Alive", "Dead", "Unknown" };
            if (!allowedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest("Status must be one of the following: Alive, Dead, Unknown.");
            }

            var characters = await _characterRepository.GetCharacterByStatusAsync(status);
            return Ok(characters);
        }

        [HttpPost("characters")]
        public async Task<IActionResult> SaveCharacters()
        {
            var allCharacters = new List<Character>();
            var allEpisodesId = new HashSet<int>();
            var url = "https://rickandmortyapi.com/api/character";
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            while (!string.IsNullOrEmpty(url))
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                var characterData = JsonSerializer.Deserialize<ApiResponse<CharacterDto>>(json, options);
                if (characterData?.Results == null)
                {
                    return BadRequest("No characters found");
                }

                foreach (var dto in characterData.Results)
                {
                    var episodesId = dto.Episode.Select(e => int.Parse(e.Split('/').Last())).ToList();
                    allCharacters.Add(new Character(dto.Id, dto.Name, dto.Status, dto.Gender, dto.Url, episodesId));

                    foreach (var episodeId in episodesId)
                    {
                        allEpisodesId.Add(episodeId);
                    }
                }

                using var doc = JsonDocument.Parse(json);
                url = doc.RootElement.GetProperty("info").TryGetProperty("next", out var nextProp)
                    && nextProp.ValueKind != JsonValueKind.Null
                    ? nextProp.GetString() : null;
            }

            var episodes = new List<Episode>();
            if (allEpisodesId.Count > 0)
            {
                var idParam = string.Join(",", allEpisodesId);
                var episodeResponse = await _httpClient.GetAsync($"https://rickandmortyapi.com/api/episode/{idParam}");
                episodeResponse.EnsureSuccessStatusCode();
                var episodeJson = await episodeResponse.Content.ReadAsStringAsync();
                var episodeOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (allEpisodesId.Count == 1)
                {
                    var episode = JsonSerializer.Deserialize<EpisodeDto>(episodeJson, episodeOptions);
                    episodes.Add(new Episode(episode.Id, episode.Name, episode.AirDate, episode.EpisodeCode, episode.Url));
                }
                else
                {
                    var episode = JsonSerializer.Deserialize<List<EpisodeDto>>(episodeJson, episodeOptions);
                    episodes.AddRange(episode.Select(e => new Episode(e.Id, e.Name, e.AirDate, e.EpisodeCode, e.Url)));
                }
            }

            await _characterRepository.SaveCharactersAndEpisodesAsync(allCharacters, episodes);
            return Ok("Characters saved successfully");
        }

        [HttpDelete("characters/{characterId}")]
        public async Task<IActionResult> DeleteCharacterWithEpisodes(int characterId)
        {
            var result = await _characterRepository.DeleteCharacterWithEpisodes(characterId);
            if (result)
            {
                return Ok("Character deleted successfully");
            }
            return NotFound("Character not found or could not be deleted.");
        }
    }
}
