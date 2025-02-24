using BroadcastSocialMedia.Data;
using BroadcastSocialMedia.Models;
using BroadcastSocialMedia.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BroadcastSocialMedia.Controllers
{
    public class RecommendedUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public RecommendedUsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index() // Uppgift 7
        {
            var allUsers = await _dbContext.Users.ToListAsync();
            var user = await _userManager.GetUserAsync(User);

            var usersListenedTo = await _dbContext.Users
                .Where(u => u.Id == user.Id)
                .SelectMany(u => u.ListeningTo)
                .ToListAsync();

            usersListenedTo.Add(user);

            var userNotListenedTo = allUsers.Except(usersListenedTo).ToList();
            userNotListenedTo = ShuffleList(userNotListenedTo);

            var viewModel = new RecommendedUsersIndexViewModel()
            {
                RecommendedUsers = userNotListenedTo
            };

            return View(viewModel);
        }

        private List<T> ShuffleList<T>(List<T> list) // Uppgift 5
        {
            Random random = new Random();
            int n = list.Count;

            for (int i = n - 1; i > 0; i--)
            {
                int k = random.Next(i + 1);
                T value = list[k];
                list[k] = list[i];
                list[i] = value;
            }

            return list;
        }
    }
}
