using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RickMorty.Domain.Model
{
    public class Character
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Gender { get; set; }
        public string Url { get; set; }
        public List<int> EpisodesId { get; set; }

        public Character(int id, string name, string status, string gender, string url, List<int> episodesId)
        {
            Id = id;
            Name = name;
            Status = status;
            Gender = gender;
            Url = url;
            EpisodesId = episodesId;
        }
    }
}
