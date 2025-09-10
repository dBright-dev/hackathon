using CampusEvents.Data;
using CampusEvents.Models;
using CampusEvents.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CampusEvents.Controllers
{
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CalendarController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? year, int? month)
        {
            var currentDate = DateTime.Now;
            var viewYear = year ?? currentDate.Year;
            var viewMonth = month ?? currentDate.Month;

            // Validate month and year
            if (viewMonth < 1 || viewMonth > 12) viewMonth = currentDate.Month;
            if (viewYear < 2000 || viewYear > 2100) viewYear = currentDate.Year;

            var firstDayOfMonth = new DateTime(viewYear, viewMonth, 1);
            var daysFromPreviousMonth = ((int)firstDayOfMonth.DayOfWeek + 6) % 7; // Monday-based week

            var startDate = firstDayOfMonth.AddDays(-daysFromPreviousMonth);
            var endDate = startDate.AddDays(41); // 6 weeks of calendar view

            // Get current user ID - adjust this based on your authentication system
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // If you're using integer UserId from your User model, you might need to convert
            // var userIdInt = int.Parse(userId); // Only if your UserId is integer

            var events = await _context.Events
                .Where(e => e.EventDate >= startDate && e.EventDate < endDate)
                .Include(e => e.Community)
                .Include(e => e.RSVPs)
                .Select(e => new CalendarEvent
                {
                    Id = e.Id,  
                    Title = e.Title,
                    EventDate = e.EventDate,
                    CommunityName = e.Community.Name,
                    Location = e.Location,
                    EventUrl = Url.Action("Details", "Events", new { id = e.Id }), // Fixed controller name
                    HasRSVPd = e.RSVPs.Any(r => r.UserId.ToString() == userId) // Adjust based on your UserId type
                })
                .ToListAsync();

            // Fetch unread notifications for the user (only if user is authenticated)
            List<Notification> notifications = new List<Notification>();

            if (!string.IsNullOrEmpty(userId))
            {
                notifications = await _context.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
            }

            var model = new CalenderViewModel  // Fixed typo: CalenderViewModel -> CalendarViewModel
            {
                Year = viewYear,
                Month = viewMonth,
                StartDate = startDate,
                Events = events,
                Notifications = notifications
            };

            return View(model);
        }

        // Action to get events for a specific day
        public async Task<IActionResult> DayEvents(int year, int month, int day)
        {
            var date = new DateTime(year, month, day);
            var nextDay = date.AddDays(1);

            // Get current user ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var events = await _context.Events
                .Where(e => e.EventDate >= date && e.EventDate < nextDay)
                .Include(e => e.Community)
                .Include(e => e.RSVPs)
                .Select(e => new CalendarEvent
                {
                    Id = e.Id, 
                    Title = e.Title,
                    EventDate = e.EventDate,
                    CommunityName = e.Community.Name,
                    Location = e.Location,
                    EventUrl = Url.Action("Details", "Events", new { id = e.Id }), // Fixed controller name
                    HasRSVPd = e.RSVPs.Any(r => r.UserId.ToString() == userId) // Adjust based on your UserId type
                })
                .ToListAsync();

            ViewBag.SelectedDate = date;
            return PartialView("_DayEvents", events);
        }
    }
}