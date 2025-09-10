namespace CampusEvents.Models.ViewModels
{
    public class CalenderViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public DateTime StartDate { get; set; } // First day of the calendar view (may include days from previous month)
        public List<CalendarEvent> Events { get; set; } = new List<CalendarEvent>();
        public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");

        // Navigation properties
        public int PreviousMonth => Month == 1 ? 12 : Month - 1;
        public int PreviousMonthYear => Month == 1 ? Year - 1 : Year;
        public int NextMonth => Month == 12 ? 1 : Month + 1;
        public int NextMonthYear => Month == 12 ? Year + 1 : Year;
        public List<Notification> Notifications { get; set; }
    }

    public class CalendarEvent
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime EventDate { get; set; }
        public string CommunityName { get; set; }
        public string Location { get; set; }
        public string EventUrl { get; set; } // URL to event details
        public bool HasRSVPd { get; set; } // Whether the current user has RSVP'd
        public string TimeDisplay => EventDate.ToString("h:mm tt");
        public string DateDisplay => EventDate.ToString("MMM d");
    }
}

