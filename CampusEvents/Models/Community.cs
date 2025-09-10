namespace CampusEvents.Models
{
    public class Community
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public virtual ICollection<Event> Events { get; set; }
    }
}
