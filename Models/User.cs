using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace museia.Models
{
    public enum UserType
    {
        SimpleUser,
        Moderator
    }

    public class User : IdentityUser 
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100)]
        public override string UserName { get; set; } = null!;

        public string? UserDescription { get; set; }

        public UserType UserType { get; set; } = UserType.SimpleUser;

        [NotMapped]
        public IFormFile? UserAvatar { get; set; }

        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
        public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
    }
}
