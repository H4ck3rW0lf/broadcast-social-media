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
    public class RecommendedUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public RecommendedUsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Załaduj listę obserwowanych użytkowników
            await _dbContext.Entry(loggedInUser).Collection(u => u.ListeningTo).LoadAsync();
            var followedUsers = loggedInUser.ListeningTo.ToList();

            // Pobierz wszystkich użytkowników oprócz siebie oraz tych, których już śledzisz
            var recommendedUsers = await _dbContext.Users
                .Where(u => u.Id != loggedInUser.Id && !followedUsers.Contains(u))
                .ToListAsync();

            var viewModel = new RecommendedUsersIndexViewModel
            {
                RecommendedUsers = recommendedUsers
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ListenToUser(UsersListenToUserViewModel viewModel)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                return RedirectToAction("Index");
            }

            var userToFollow = await _dbContext.Users.FindAsync(viewModel.UserId);
            if (userToFollow != null && userToFollow.Id != loggedInUser.Id)
            {
                if (!loggedInUser.ListeningTo.Contains(userToFollow))
                {
                    loggedInUser.ListeningTo.Add(userToFollow);
                    await _userManager.UpdateAsync(loggedInUser);
                    await _dbContext.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index", "RecommendedUsers");
        }
    }
}

