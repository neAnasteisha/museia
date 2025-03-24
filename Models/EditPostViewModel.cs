namespace museia.Models
{
    public class EditPostViewModel
    {
        public uint PostID { get; set; }
        public string? PostText { get; set; }
        public PostTag? PostTag { get; set; }
        public string? PostPhotoUrl { get; set; }
        public IFormFile? PostPhoto { get; set; }
    }
}
