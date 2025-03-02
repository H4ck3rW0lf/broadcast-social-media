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

    // Uppfift 7 - Rekommendera användare //
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

            // Ladda upp list av följda användare //
            await _dbContext.Entry(loggedInUser).Collection(u => u.ListeningTo).LoadAsync();
            var followedUsers = loggedInUser.ListeningTo.ToList();

            // Ladda upp alla användare förutom logged In och redan följd användare //
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

