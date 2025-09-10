// Controllers/EventController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CampusEvents.Data;
using CampusEvents.Models;
using CampusEvents.Models.ViewModels;
using CampusEvents.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace CampusEvents.Controllers
{
    [Authorize]
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
        public IActionResult Index(int communityId)
        {
            // Check if user is a member of this community
            var userId = User.Identity.Name;
            var isMember = _context.UserCommunities
                .Any(uc => uc.UserId == userId && uc.CommunityId == communityId);

            if (!isMember)
            {
                TempData["ErrorMessage"] = "You must join the community before viewing its events.";
                return RedirectToAction("Index", "Community");
            }

            var events = _context.Events
                .Where(e => e.CommunityId == communityId)
                .Include(e => e.RSVPs)
                .ToList();

            ViewBag.CommunityId = communityId;

            // Get community name for display
            var community = _context.Communities.FirstOrDefault(c => c.Id == communityId);
            ViewBag.CommunityName = community?.Name;

            return View(events);
        }

        // GET: Event/Create?communityId=...
        [Authorize(Roles = "Faculty")]
        public IActionResult Create(int communityId)
        {
            // Check if user is a member of this community
            var userId = User.Identity.Name;
            var isMember = _context.UserCommunities
                .Any(uc => uc.UserId == userId && uc.CommunityId == communityId);

            if (!isMember)
            {
                TempData["ErrorMessage"] = "You must join the community before viewing its events.";
                return RedirectToAction("Index", "Community");
            }

            // Get the community to display its name
            var community = _context.Communities.FirstOrDefault(c => c.Id == communityId);

            if (community == null)
            {
                return NotFound();
            }

            // Initialize the view model with both CommunityId and CommunityName
            var model = new EventViewModel
            {
                CommunityId = communityId,
                CommunityName = community.Name
            };

            return View(model);
        }

        // POST: Event/Create
        // Controllers/EventController.cs
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Faculty")]
        public async Task<IActionResult> Create(EventViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verify user is a member of the community
                    var userId = User.Identity.Name;
                    var isMember = await _context.UserCommunities
                        .AnyAsync(uc => uc.UserId == userId && uc.CommunityId == model.CommunityId);

                    if (!isMember)
                    {
                        TempData["ErrorMessage"] = "You must be a member of the community to create events.";
                        return RedirectToAction("Index", "Community");
                    }

                    string imageUrl = null;
                    string materialsUrl = null;

                    if (model.Image != null)
                    {
                        imageUrl = await _storageService.UploadFileAsync(model.Image, "event-images");
                    }

                    if (model.Materials != null)
                    {
                        materialsUrl = await _storageService.UploadFileAsync(model.Materials, "event-materials");
                    }

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

                    _context.Events.Add(eventItem);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Event created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log the error
                    ModelState.AddModelError("", "An error occurred while creating the event. Please try again.");

                }
                
            }

            // If we got this far, something failed; reload community name
            var community = await _context.Communities
                .FirstOrDefaultAsync(c => c.Id == model.CommunityId);
            model.CommunityName = community?.Name;

            return View(model);
        }

        // POST: Event/RSVP
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RSVP(int eventId)
        {

            var userId = User.Identity.Name;
            var existingRSVP = await _context.Rsvps
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

            if (existingRSVP == null)
            {
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
                TempData["InfoMessage"] = "You have already RSVP'd for this event.";
            }

            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        // GET: Event/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Community)
                .Include(e => e.RSVPs)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            // Check if current user has RSVP'd
            var userId = User.Identity.Name;
            ViewBag.HasRSVPd = await _context.Rsvps
                .AnyAsync(r => r.EventId == id && r.UserId == userId);

            return View(eventItem);
        }

        // GET: Event/Edit/{id}
        [Authorize(Roles = "Faculty")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        // POST: Event/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Faculty")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,EventDate,Location,ImageUrl,MaterialsUrl,CommunityId")] Event eventItem)
        {
            if (id != eventItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eventItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { communityId = eventItem.CommunityId });
            }
            return View(eventItem);
        }

        // GET: Event/Delete/{id}
        [Authorize(Roles = "Faculty")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventItem = await _context.Events
                .Include(e => e.Community)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem);
        }

        // POST: Event/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Faculty")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem != null)
            {
                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { communityId = eventItem.CommunityId });
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}