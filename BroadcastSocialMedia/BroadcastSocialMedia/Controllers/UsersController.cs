using BroadcastSocialMedia.Data;
using BroadcastSocialMedia.Models;
using BroadcastSocialMedia.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BroadcastSocialMedia.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(UsersIndexViewModel viewModel)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            await _dbContext.Entry(loggedInUser).Collection(u => u.ListeningTo).LoadAsync();
            var followedUsers = loggedInUser.ListeningTo.ToList();

            var allUsers = await _dbContext.Users
                .Where(u => u.Id != loggedInUser.Id)
                .ToListAsync();

            var filteredUsers = new List<ApplicationUser>();

            if (!string.IsNullOrEmpty(viewModel.Search))
            {
                filteredUsers = allUsers
                    .Where(u => u.Name != null && u.Name.ToLower().Contains(viewModel.Search.ToLower()))
                    .ToList();

                if (!filteredUsers.Any())
                {
                    viewModel.ErrorMessage = "User not found. Check spelling and try again.";
                }
            }

            viewModel.Result = filteredUsers;
            viewModel.FollowingUsers = followedUsers;

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

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> StopListenToUser(UsersListenToUserViewModel viewModel)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                return RedirectToAction("Index");
            }

            var userToUnfollow = await _dbContext.Users.FindAsync(viewModel.UserId);
            if (userToUnfollow != null)
            {
                await _dbContext.Entry(loggedInUser).Collection(u => u.ListeningTo).LoadAsync();

                if (loggedInUser.ListeningTo.Contains(userToUnfollow))
                {
                    loggedInUser.ListeningTo.Remove(userToUnfollow);
                    await _userManager.UpdateAsync(loggedInUser);
                    await _dbContext.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
        }
    }
}
