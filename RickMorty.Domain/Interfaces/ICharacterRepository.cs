using RickMorty.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RickMorty.Domain.Interfaces
{
    public interface ICharacterRepository
    {
        Task SaveCharactersAndEpisodesAsync(List<Character> characters, List<Episode> episodes);
        Task<List<Character>> GetCharacterByStatusAsync(string status);
        Task<bool> DeleteCharacterWithEpisodes(int characterId);
    }
}
