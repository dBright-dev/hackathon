using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using CampusEvents.Models;
using Microsoft.AspNetCore.Mvc;

namespace CampusEvents.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact(string scrollTo = null)
        {
            ViewBag.ScrollTo = scrollTo;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Here you would typically save the contact form to a database
                // or send an email. For now, we'll just show a success message.
                TempData["SuccessMessage"] = "Thank you for your message! We'll get back to you soon.";
                return RedirectToAction("Contact");
            }
            return View(model);
        }



        // Contact ViewModel
        public class ContactViewModel
        {
            [Required(ErrorMessage = "Name is required")]
            public string Name { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Subject is required")]
            public string Subject { get; set; }

            [Required(ErrorMessage = "Message is required")]
            public string Message { get; set; }
        }
    }
}


