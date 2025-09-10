using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CampusEvents.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public string? ImageUrl { get; set; }
        public string MaterialsUrl { get; set; }
        public int CommunityId { get; set; }
        public virtual Community Community { get; set; }
        public virtual ICollection<RSVP> RSVPs { get; set; }
    }
}

