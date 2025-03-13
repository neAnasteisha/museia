using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace museia.Models
{

    public enum UserType
    {
        SimpleUser,
        Moderator
    }
    public class User
    {
        [Key]
        public uint UserID { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(100)]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string UserEmail { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string UserPassword { get; set; } = null!;

        public string? UserDescription { get; set; }

        [Required(ErrorMessage = "Type is required")]
        public UserType UserType { get; set; }

        [NotMapped]
        public IFormFile? UserAvatar { get; set; }

        public ICollection<Post> Posts { get; set; }
        public ICollection<Reaction> Reactions { get; set; }
        public ICollection<Complaint> Complaints { get; set; }
    }
}
