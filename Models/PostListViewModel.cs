namespace museia.Models
{
    public class PostListViewModel
    {
        public IEnumerable<Post> Posts { get; set; } = new List<Post>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
