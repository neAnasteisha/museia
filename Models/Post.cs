using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace museia.Models
{
    public enum PostTag
    {
        Photo, 
        Poetry,
        Music,
        Sculpture,
        Painting
        //дописати
    }
    public class Post
    {
        [Key]
        public uint PostID { get; set; }

        [MaxLength(500)]
        public string? PostText { get; set; }


        [NotMapped]
        public IFormFile? PostAlbum { get; set; }


        [Required(ErrorMessage = "Tag is required")]
        public PostTag PostTag { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public uint UserID { get; set; }
        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }
        public ICollection<Reaction> Reactions { get; set; }
        public ICollection<Complaint> Complaints { get; set; }
    }
}
