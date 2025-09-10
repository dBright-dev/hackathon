using System.ComponentModel.DataAnnotations;

namespace CampusEvents.Models.ViewModels
{
    public class EventViewModel
    {
        [Required]
        [Display(Name = "Event Title")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 5)]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Event Description")]
        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 10)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Event Date and Time")]
        [DataType(DataType.DateTime)]
        [FutureDate(ErrorMessage = "Event date must be in the future.")]
        public DateTime EventDate { get; set; } = DateTime.Now.AddDays(1);

        [Required]
        [Display(Name = "Location")]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 5)]
        public string Location { get; set; }

        [Display(Name = "Event Image")]
        public IFormFile? Image { get; set; }

        [Display(Name = "Event Materials (PDF, DOC, etc.)")]
        public IFormFile? Materials { get; set; }

        [Required]
        public int CommunityId { get; set; }

        // Property to display community name in the view
        public string CommunityName { get; set; }
    }

    // Custom validation attribute for future date
    public class FutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value is DateTime date)
            {
                return date > DateTime.Now;
            }
            return false;
        }
    }
}
