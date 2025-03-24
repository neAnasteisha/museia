using Microsoft.AspNetCore.Http;

namespace museia.Models
{
    public class EditProfileViewModel
    {
        public string? Username { get; set; }
        public string? Description { get; set; }
        public string? AvatarUrl { get; set; }
        public IFormFile? Avatar { get; set; }
    }
}
