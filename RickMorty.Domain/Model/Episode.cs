using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RickMorty.Domain.Model
{
    public class Episode
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AirDate { get; set; }
        public string EpisodeCode { get; set; }
        public string Url { get; set; }

        public Episode(int id, string name, string airDate, string episodeCode, string url)
        {
            Id = id;
            Name = name;
            AirDate = airDate;
            EpisodeCode = episodeCode;
            Url = url;
        }
    }
}
