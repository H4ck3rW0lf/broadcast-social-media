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

            var viewModel = new ProfileIndexViewModel()
            {
                Name = user.Name ?? "",
                ImageFilename = user.ProfileImageFilename
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ProfileIndexViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            var userNameTaken = false;

            if (viewModel.Name != user.Name) // Uppgift 3
            {
                userNameTaken = _dbContext.Users.Any(u => u.Name == viewModel.Name);
            }

            if (userNameTaken)
            {
                var profileImageViewModel = new ProfileIndexViewModel()
                {
                    Name = viewModel.Name ?? "",
                    ImageFilename = user.ProfileImageFilename,
                    ErrorMessage = "This name is already in use! Choose a different username."
                };

                return View(profileImageViewModel);
            }

            // Om en ny profilbild har laddats upp
            if (viewModel.ProfileImageFile != null)
            {
                var fileName = SaveImageFile(viewModel.ProfileImageFile);
                user.ProfileImageFilename = fileName;
            }

            user.Name = viewModel.Name;

            await _userManager.UpdateAsync(user);

            TempData["Message"] = "Your name is saved!";

            return Redirect("/");
        }

        private string SaveImageFile(IFormFile imageFile) // Uppgift 2
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
