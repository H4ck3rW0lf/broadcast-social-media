using BroadcastSocialMedia.Data;
using BroadcastSocialMedia.Models;
using BroadcastSocialMedia.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BroadcastSocialMedia.Controllers
{
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
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Index", new HomeIndexViewModel
                {
                    Broadcasts = new List<Broadcast>(),
                    ErrorMessage = "You are not logged in."
                });
            }

            // Ladda in ListeningTo-samlingen
            await _dbContext.Entry(user).Collection(u => u.ListeningTo).LoadAsync();

            // Samla in broadcasts manuellt från varje följd användare
            List<Broadcast> broadcasts = new List<Broadcast>();
            if (user.ListeningTo != null)
            {
                foreach (var followedUser in user.ListeningTo)
                {
                    await _dbContext.Entry(followedUser).Collection(u => u.Broadcasts).LoadAsync();
                    if (followedUser.Broadcasts != null)
                    {
                        foreach (var broadcast in followedUser.Broadcasts)
                        {
                            broadcasts.Add(broadcast);
                        }
                    }
                }
            }

            // Sortera manuellt (nyaste först)
            broadcasts.Sort((a, b) => b.Published.CompareTo(a.Published));

            HomeIndexViewModel viewModel = new HomeIndexViewModel
            {
                Broadcasts = broadcasts,
                ErrorMessage = null
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        [HttpPost]
        public async Task<IActionResult> Broadcast(HomeBroadcastViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Index", new HomeIndexViewModel
                {
                    Broadcasts = new List<Broadcast>(),
                    ErrorMessage = "You are not logged in."
                });
            }

            if (string.IsNullOrEmpty(viewModel.Message))
            {
                await _dbContext.Entry(user).Collection(u => u.ListeningTo).LoadAsync();
                List<Broadcast> broadcasts = new List<Broadcast>();
                if (user.ListeningTo != null)
                {
                    foreach (var followedUser in user.ListeningTo)
                    {
                        await _dbContext.Entry(followedUser).Collection(u => u.Broadcasts).LoadAsync();
                        if (followedUser.Broadcasts != null)
                        {
                            foreach (var broadcast in followedUser.Broadcasts)
                            {
                                broadcasts.Add(broadcast);
                            }
                        }
                    }
                }
                broadcasts.Sort((a, b) => b.Published.CompareTo(a.Published));

                HomeIndexViewModel indexViewModel = new HomeIndexViewModel
                {
                    Broadcasts = broadcasts,
                    ErrorMessage = "Please fill in a message before posting."
                };

                return View("Index", indexViewModel);
            }

            string fileName = "";
            if (viewModel.ImageFile != null)
            {
                fileName = SaveImageFile(viewModel.ImageFile);
            }

            Broadcast newBroadcast = new Broadcast
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

        [HttpPost]
        public async Task<IActionResult> LikeBroadcast(HomeLikeBroadcastViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Index", new HomeIndexViewModel
                {
                    Broadcasts = new List<Broadcast>(),
                    ErrorMessage = "You are not logged in."
                });
            }

            var broadcast = await _dbContext.Broadcasts.FindAsync(viewModel.BroadcastId);
            if (broadcast == null)
            {
                return NotFound();
            }

            if (broadcast.UserThatLikeBroadcasts == null)
            {
                broadcast.UserThatLikeBroadcasts = new List<UserThatLikeBroadcast>();
            }

            bool alreadyLiked = false;
            foreach (var like in broadcast.UserThatLikeBroadcasts)
            {
                if (like.UserId == user.Id)
                {
                    alreadyLiked = true;
                    break;
                }
            }

            if (!alreadyLiked)
            {
                UserThatLikeBroadcast newLike = new UserThatLikeBroadcast
                {
                    UserId = user.Id,
                    NameOfUserThatLike = user.Name,
                    BroadcastId = broadcast.Id
                };

                broadcast.UserThatLikeBroadcasts.Add(newLike);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        private string SaveImageFile(IFormFile imageFile)
        {
            string projectDirectory = Directory.GetCurrentDirectory();
            string relativePath = "wwwroot\\images\\broadcastImages";
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
