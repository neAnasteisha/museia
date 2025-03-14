using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace museia.Models
{
    public enum PostTag
    {
        Фото, 
        Поезія,
        Музика,
        Скульптура,
        Малюнок
        //дописати
    }
    public class Post
    {
        [Key]
        public uint PostID { get; set; }

        [MaxLength(500)]
        public string? PostText { get; set; }

        public string? PostPhoto { get; set; }

        public PostTag PostTag { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public string UserID { get; set; }
        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }
        public ICollection<Reaction> Reactions { get; set; }
        public ICollection<Complaint> Complaints { get; set; }
    }
}
