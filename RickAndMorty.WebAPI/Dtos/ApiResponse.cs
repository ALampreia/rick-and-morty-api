namespace RickAndMorty.WebAPI.Dtos
{
    public class ApiResponse<T>
    {
        public List<T> Results { get; set; }
    }
}
