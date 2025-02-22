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
        private readonly ApplicationDbContext _dbContext; // Dependency Injection
        private readonly UserManager<ApplicationUser> _userManager;
        public UsersController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index(UsersIndexViewModel viewModel)
        {
            if(viewModel.Search != null)
            {
                var users = await _dbContext.Users.Where(u => u.Name.Contains(viewModel.Search))        // Hämtar lista på användare
                    .ToListAsync();
                viewModel.Result = users;
            }
            
            return View(viewModel);
        }

        [Route("/Users/{id}")]
        public async Task<IActionResult> ShowUser(string id)
        {
            var broadcasts = await _dbContext.Broadcasts.Where(b => b.User.Id == id)
                .OrderByDescending(b => b.Published)
                .ToListAsync();
            var user = await _userManager.GetUserAsync(User);

            var viewModel = new UsersShowUserViewModel
            {
                Broadcasts = broadcasts,
                User = user
            };

            return View(viewModel);
        }
    }
}
