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
    public class FeaturedController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public FeaturedController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var broadcasts = await _dbContext.Broadcasts
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
