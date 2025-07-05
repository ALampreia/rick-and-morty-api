namespace RickAndMorty.WebAPI.Dtos
{
    public class CharacterDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Gender { get; set; }
        public string Url { get; set; }
        public List<string> Episode { get; set; }
    }
}
