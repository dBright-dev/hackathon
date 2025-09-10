using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CampusEvents.Data;
using CampusEvents.Models;
using CampusEvents.Models.ViewModels;
using CampusEvents.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusEvents.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAzureStorageService _storageService;

        public EventController(ApplicationDbContext context, IAzureStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        // GET: Event/Index
        // Lists all events for a specific community including RSVP info
        public IActionResult Index(int communityId)
        {
            // Retrieve events for the given community, including their RSVPs
            var events = _context.Events
                .Where(e => e.CommunityId == communityId)
                .Include(e => e.RSVPs)
                .ToList();

            // Pass the communityId to the view for context
            ViewBag.CommunityId = communityId;
            return View(events);
        }

        // GET: Event/Create?communityId=...
        // Displays the form to create a new event for a community
        public IActionResult Create(int communityId)
        {
            // Initialize the view model with the communityId
            var model = new EventViewModel { CommunityId = communityId };
            return View(model);
        }

        // POST: Event/Create
        // Handles submission of the new event form, including file uploads
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel model)
        {
            if (ModelState.IsValid)
            {
                string imageUrl = null;
                string materialsUrl = null;

                // Upload event image to Azure Blob Storage if provided
                if (model.Image != null)
                {
                    imageUrl = await _storageService.UploadFileAsync(model.Image, "event-images");
                }

                // Upload event materials to Azure Blob Storage if provided
                if (model.Materials != null)
                {
                    materialsUrl = await _storageService.UploadFileAsync(model.Materials, "event-materials");
                }

                // Create new Event entity with data from the view model
                var eventItem = new Event
                {
                    Title = model.Title,
                    Description = model.Description,
                    EventDate = model.EventDate,
                    Location = model.Location,
                    ImageUrl = imageUrl,
                    MaterialsUrl = materialsUrl,
                    CommunityId = model.CommunityId
                };

                // Add and save the new event to the database
                _context.Events.Add(eventItem);
                await _context.SaveChangesAsync();

                // Redirect to the event list for the community
                return RedirectToAction(nameof(Index), new { communityId = model.CommunityId });
            }

            // If model validation failed, redisplay the form with validation errors
            return View(model);
        }

        // POST: Event/RSVP
        // Allows the current user to RSVP to an event
        [HttpPost]
        public async Task<IActionResult> RSVP(int eventId)
        {
            var userId = User.Identity.Name;

            // Check if the user has already RSVP'd to this event
            var existingRSVP = _context.Rsvps
                .FirstOrDefault(r => r.EventId == eventId && r.UserId == userId);

            if (existingRSVP == null)
            {
                // Create a new RSVP record
                var rsvp = new RSVP
                {
                    EventId = eventId,
                    UserId = userId,
                    RespondedAt = DateTime.Now
                };

                _context.Rsvps.Add(rsvp);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "You have successfully RSVP'd for this event!";
            }
            else
            {
                // Inform user they have already RSVP'd
                TempData["InfoMessage"] = "You have already RSVP'd for this event.";
            }

            // Redirect to the event details page
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        // GET: Event/Details/{id}
        // Displays details of a specific event including community info and RSVP status
        public IActionResult Details(int id)
        {
            // Retrieve the event with related community and RSVPs
            var eventItem = _context.Events
                .Include(e => e.Community)
                .Include(e => e.RSVPs)
                .FirstOrDefault(e => e.Id == id);

            if (eventItem == null)
            {
                // Return 404 if event not found
                return NotFound();
            }

            // Check if the current user has RSVP'd to this event
            var userId = User.Identity.Name;
            ViewBag.HasRSVPd = _context.Rsvps
                .Any(r => r.EventId == id && r.UserId == userId);

            return View(eventItem);
        }

        // GET: Event/Edit/{id}
        // Displays the form to edit an existing event
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                // Return 404 if no id provided
                return NotFound();
            }

            // Find the event by id
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                // Return 404 if event not found
                return NotFound();
            }
            return View(@event);
        }

        // POST: Event/Edit/{id}
        // Handles submission of the event edit form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventID,EventName,EventDescription,EventType,EventDate,ImageUrl,Location")] Event @event)
        {
            if (id != @event.Id)
            {
                // Return 404 if route id does not match event id
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the event in the database
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency conflicts
                    if (!EventExists(@event.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Redirect to the event list after successful edit
                return RedirectToAction(nameof(Index));
            }
            // If validation failed, redisplay the form
            return View(@event);
        }

        // GET: Event/Delete/{id}
        // Displays confirmation page to delete an event
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                // Return 404 if no id provided
                return NotFound();
            }

            // Find the event by id
            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@event == null)
            {
                // Return 404 if event not found
                return NotFound();
            }

            return View(@event);
        }

        // POST: Event/DeleteConfirmed/{id}
        // Handles the actual deletion of the event after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Find the event by id
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                // Remove the event from the database
                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
            }
            // Redirect to the event list after deletion
            return RedirectToAction(nameof(Index));
        }

        // Helper method to check if an event exists by id
        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
