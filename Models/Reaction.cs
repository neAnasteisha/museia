using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace museia.Models
{
    public enum Emoji
    {
        Heart = 0x1F497,
        ThumbsUp = 0x1F44D,
        Cry = 0x1F62D,
        Dragon = 0x1F409,
        Laughing = 0x1F639,
        Throwing_Up = 0x1F92E,
        Sparkle = 0x2728
    }

    public class Reaction
    {
        [Key]
        public uint ReactionID { get; set; }

        [Required]
        public Emoji ReactionType { get; set; }

        [Required]
        public uint PostID { get; set; } 
        [Required]
        public string UserID { get; set; } 

        [ForeignKey("PostID")]
        public Post Post { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }
    }
}
