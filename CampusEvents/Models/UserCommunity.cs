using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusEvents.Models
{
    public class UserCommunity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int CommunityId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime JoinedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("CommunityId")]
        public virtual Community Community { get; set; }
    }
}
