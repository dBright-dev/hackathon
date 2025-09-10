using System.ComponentModel.DataAnnotations;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace CampusEvents.Models.ViewModels
{
    // Models/ViewModels/CommunityIndexViewModel.cs
    public class CommunityViewModel
    {
        [Required(ErrorMessage = "Community name is required.")]
        [Display(Name = "Community Name")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 5)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Community description is required.")]
        [Display(Name = "Description")]
        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 10)]
        public string Description { get; set; }

        [Display(Name = "Community Image")]
        public IFormFile? Image { get; set; }
    }
}
