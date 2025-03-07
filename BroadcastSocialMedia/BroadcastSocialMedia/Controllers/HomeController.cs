using BroadcastSocialMedia.Data;
using BroadcastSocialMedia.Models;
using BroadcastSocialMedia.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BroadcastSocialMedia.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger,
                              UserManager<ApplicationUser> userManager,
                              ApplicationDbContext dbContext,
                              IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _userManager = userManager;
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var broadcasts = await _dbContext.Broadcasts
                .Include(b => b.User)
                .Include(b => b.UserThatLikeBroadcasts)
                .OrderByDescending(b => b.Published)
                .ToListAsync();

            return View(new HomeIndexViewModel
            {
                Broadcasts = broadcasts,
                ErrorMessage = TempData["ErrorMessage"] as string
            });
        }

        public async Task<IActionResult> Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Broadcast(HomeBroadcastViewModel viewModel)
        {
            // Uppgift 8 - Visa felmeddeland //
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to post.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrEmpty(viewModel.Message))
            {
                TempData["ErrorMessage"] = "Please enter a message before posting.";
                return RedirectToAction("Index");
            }

            string fileName = "";
            if (viewModel.ImageFile != null)
            {
                fileName = SaveImageFile(viewModel.ImageFile);
            }

            var newBroadcast = new Broadcast
            {
                Message = viewModel.Message,
                User = user,
                ImageFilename = fileName,
                Published = DateTime.Now
            };

            _dbContext.Broadcasts.Add(newBroadcast);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Uppgift 5 - Gilla markeringar utan dubbletter 
        [HttpPost]
        public async Task<IActionResult> LikeBroadcast(HomeLikeBroadcastViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            var broadcast = await _dbContext.Broadcasts
                .Include(b => b.UserThatLikeBroadcasts)         // Laddar list av anv�ndare som gillar inl�gg //
                .FirstOrDefaultAsync(b => b.Id == viewModel.BroadcastId);

            if (broadcast == null || broadcast.UserThatLikeBroadcasts.Any(l => l.UserId == user.Id))
            {
                return RedirectToAction("Index");
            }

            var userName = user.Name ?? "Unknown User";

            broadcast.UserThatLikeBroadcasts.Add(new UserThatLikeBroadcast
            {
                UserId = user.Id,
                NameOfUserThatLike = userName,
                BroadcastId = broadcast.Id
            });

            await _dbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Uppgift 2 - Ladda upp bild till inl�gg //
        private string SaveImageFile(IFormFile imageFile)
        {
            string projectDirectory = Directory.GetCurrentDirectory();
            string relativePath = "wwwroot/images/broadcastImages";
            string fullPath = Path.Combine(projectDirectory, relativePath);
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string fullPathAndImageFileName = Path.Combine(fullPath, fileName);

            using (var stream = new FileStream(fullPathAndImageFileName, FileMode.Create))
            {
                imageFile.CopyTo(stream);
            }

            return fileName;
        }
    }
}


