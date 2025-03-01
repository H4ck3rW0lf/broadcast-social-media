using BroadcastSocialMedia.Data;
using BroadcastSocialMedia.Models;
using BroadcastSocialMedia.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BroadcastSocialMedia.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            await _dbContext.Entry(user).Collection(u => u.ListeningTo).LoadAsync();

            return View(new ProfileIndexViewModel
            {
                Name = user.Name ?? "",
                ImageFilename = user.ProfileImageFilename,
                FollowingUsers = user.ListeningTo.ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> Update(ProfileIndexViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            var userNameTaken = await _dbContext.Users.AnyAsync(u => u.Name == viewModel.Name && u.Id != user.Id);
            if (userNameTaken)
            {
                TempData["Message"] = "This name is already in use. Choose a different username.";
                return RedirectToAction("Index");
            }

            if (viewModel.ProfileImageFile != null)
            {
                var fileName = SaveImageFile(viewModel.ProfileImageFile);
                user.ProfileImageFilename = fileName;
            }

            user.Name = viewModel.Name;
            await _userManager.UpdateAsync(user);

            TempData["Message"] = "Your profile has been updated!";
            return RedirectToAction("Index");
        }

        private string SaveImageFile(IFormFile imageFile)
        {
            string projectDirectory = Directory.GetCurrentDirectory();
            string relativePath = "wwwroot/images/profilePictures";
            string fullPath = Path.Combine(projectDirectory, relativePath);
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            string fullPathAndImageFileName = Path.Combine(fullPath, fileName);

            using (var stream = new FileStream(fullPathAndImageFileName, FileMode.Create))
            {
                imageFile.CopyTo(stream);
            }

            return fileName;
        }
    }
}
