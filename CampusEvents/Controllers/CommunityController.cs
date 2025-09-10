using System;
using System.Linq;
using System.Threading.Tasks;
using CampusEvents.Data;
using CampusEvents.Models;
using CampusEvents.Models.ViewModels;
using CampusEvents.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CampusEvents.Controllers
{
    [Authorize]
    public class CommunityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAzureStorageService _storageService;
        private readonly ILogger<CommunityController> _logger;

        public CommunityController(ApplicationDbContext context,
                                  IAzureStorageService storageService,
                                  ILogger<CommunityController> logger)
        {
            _context = context;
            _storageService = storageService;
            _logger = logger;
        }

        // GET: Community/Index
        // Displays a list of all communities with indication if the current user is a member
        public async Task<IActionResult> Index()
        {
            var userId = User.Identity.Name; // Get current user's username

            // Retrieve all communities from the database
            var communities = await _context.Communities.ToListAsync();

            // Retrieve the list of community IDs the user has joined
            var userCommunities = await _context.UserCommunities
                .Where(uc => uc.UserId == userId)
                .Select(uc => uc.CommunityId)
                .ToListAsync();

            // Create a view model list indicating membership status for each community
            var viewModel = communities.Select(c => new CommunityIndexViewModel
            {
                Community = c,
                IsMember = userCommunities.Contains(c.Id)
            }).ToList();

            return View(viewModel);
        }

        // POST: Community/Join
        // Allows the current user to join a community by communityId
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(int communityId)
        {
            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                // Store the intended action
                TempData["ActionMessage"] = "You need to login to join communities.";
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = Url.Action("Index", "Community") });
            }

            var userId = User.Identity.Name;

            // Check if the user is already a member of the community
            var existingMembership = await _context.UserCommunities
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CommunityId == communityId);

            if (existingMembership == null)
            {
                // Create a new membership record
                var userCommunity = new UserCommunity
                {
                    UserId = userId,
                    CommunityId = communityId,
                    JoinedAt = DateTime.Now
                };

                _context.UserCommunities.Add(userCommunity);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "You have successfully joined the community!";
            }
            else
            {
                // Inform user they are already a member
                TempData["InfoMessage"] = "You are already a member of this community.";
            }

            // Redirect back to the community list
            return RedirectToAction(nameof(Index));
        }

        // POST: Community/Leave
        // Allows the current user to leave a community by communityId
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int communityId)
        {
            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                // Store the intended action
                TempData["ActionMessage"] = "You need to login to join communities.";
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = Url.Action("Index", "Community") });
            }
            var userId = User.Identity.Name;

            // Find the membership record for the user and community
            var membership = await _context.UserCommunities
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CommunityId == communityId);

            if (membership != null)
            {
                // Remove the membership record
                _context.UserCommunities.Remove(membership);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "You have left the community.";
            }

            // Redirect back to the community list
            return RedirectToAction(nameof(Index));
        }

        // GET: Community/Create
        // Displays the form to create a new community
        [Authorize(Roles = "Faculty")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Community/Create
        // Handles the submission of the new community creation form
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Faculty")]
        public async Task<IActionResult> Create(CommunityViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Get the current user's username
                    var userId = User.Identity.Name;

                    // Ensure the user is logged in
                    if (string.IsNullOrEmpty(userId))
                    {
                        ModelState.AddModelError("", "You must be logged in to create a community.");
                        return View(model);
                    }

                    string imageUrl = null;

                    // If an image file was uploaded, attempt to upload it to Azure Blob Storage
                    if (model.Image != null)
                    {
                        try
                        {
                            imageUrl = await _storageService.UploadFileAsync(model.Image, "community-images");
                        }
                        catch (Exception ex)
                        {
                            // Log the error and add a model error to inform the user
                            _logger.LogError(ex, "Error uploading community image");
                            ModelState.AddModelError("Image", "Error uploading image. Please try again.");
                            return View(model);
                        }
                    }

                    // Create a new Community entity with the provided data
                    var community = new Community
                    {
                        Name = model.Name,
                        Description = model.Description,
                        ImageUrl = imageUrl,
                        CreatedBy = userId, // Set the creator of the community
                        CreatedAt = DateTime.Now
                    };

                    // Add the new community to the database
                    _context.Communities.Add(community);
                    await _context.SaveChangesAsync();

                    // Automatically add the creator as a member of the new community
                    var userCommunity = new UserCommunity
                    {
                        UserId = userId,
                        CommunityId = community.Id,
                        JoinedAt = DateTime.Now
                    };

                    _context.UserCommunities.Add(userCommunity);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Community '{model.Name}' created successfully! You've been automatically joined as a member.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                // Log any unexpected errors during community creation
                _logger.LogError(ex, "Error creating community");
                ModelState.AddModelError("", "An error occurred while creating the community. Please try again.");
            }

            // If we got this far, something failed; redisplay form
            return View(model);
        }

        // GET: Community/Details/{id}
        // Displays details of a specific community including its events
        public IActionResult Details(int id)
        {
            // Retrieve the community with its related events
            var community = _context.Communities
                .Include(c => c.Events)
                .FirstOrDefault(c => c.Id == id);

            if (community == null)
            {
                // Return 404 if community not found
                return NotFound();
            }

            return View(community);
        }

        // GET: Community/MyCommunities
        // Displays the list of communities the current user has joined
        [Authorize(Roles = "Faculty,Student")]
        public async Task<IActionResult> MyCommunities()
        {
            var userId = User.Identity.Name;

            // Retrieve communities where the user is a member
            var userCommunities = await _context.UserCommunities
                .Where(uc => uc.UserId == userId)
                .Include(uc => uc.Community)
                .Select(uc => uc.Community)
                .ToListAsync();

            return View(userCommunities);
        }
    }
}
