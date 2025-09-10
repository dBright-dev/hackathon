using CampusEvents.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampusEvents.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Community> Communities { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<RSVP> Rsvps { get; set; }
        public DbSet<UserCommunity> UserCommunities { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<UserCommunity>()
                .HasIndex(uc => new { uc.UserId, uc.CommunityId })
                .IsUnique();

            modelBuilder.Entity<RSVP>()
                .HasIndex(r => new { r.UserId, r.EventId })
                .IsUnique();
        }
    }
}
