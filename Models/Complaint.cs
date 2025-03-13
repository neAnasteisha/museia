using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace museia.Models
{
    public enum ComplaintStatus
    {
        Sent,
        Processing,
        Declined,
        Accepted
    }
    public class Complaint
    {
        [Key]
        public uint ComplaintID { get; set; }

        [Required]
        public string UserID { get; set; } 

        [Required]
        public uint PostID { get; set; } 

        [Required]
        public string ComplaintReason { get; set; }

        [Required]
        public ComplaintStatus ComplaintStatus { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; } = null!;

        [ForeignKey("PostID")]
        public Post Post { get; set; } = null!;
    }
}
