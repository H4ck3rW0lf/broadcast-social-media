using BroadcastSocialMedia.Data;
using BroadcastSocialMedia.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BroadcastSocialMedia.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _dbContext; // Dependency Injection

        public UsersController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
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
    }
}
