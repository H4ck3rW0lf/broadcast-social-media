using BroadcastSocialMedia.Data;
using BroadcastSocialMedia.Models;
using BroadcastSocialMedia.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BroadcastSocialMedia.Controllers
{
    public class FeaturedController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public FeaturedController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index() // Uppgift 6
        {
            var user = await _userManager.GetUserAsync(User);

            var broadcasts = await _dbContext.Broadcasts // Hämtar alla broadcasts och sorterar dem på flest likes
                .Include(b => b.User)
                .Include(b => b.UserThatLikeBroadcasts)
                .OrderByDescending(b => b.UserThatLikeBroadcasts.Count)
                .Take(5)
                .ToListAsync();

            var viewModel = new FeaturedIndexViewModel()
            {
                SortedBroadcasts = broadcasts
            };

            return View(viewModel);
        }
    }
}
