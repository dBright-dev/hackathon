using System.ComponentModel.DataAnnotations;
using System;

namespace CampusEvents.Models
{
    public class RSVP
    {
        [Key]
        public int Id { get; set; }
        public int EventId { get; set; }
        public string UserId { get; set; }
        public DateTime RespondedAt { get; set; }
        public virtual Event Event { get; set; }
    }
}
