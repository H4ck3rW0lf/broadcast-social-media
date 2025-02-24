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

        // Index: Hanterar användarsökning och fyller viewmodellen med alla användare (filtreras manuellt).
        public async Task<IActionResult> Index(UsersIndexViewModel viewModel)
        {
            List<ApplicationUser> allUsers = await _dbContext.Users.ToListAsync();
            List<ApplicationUser> filteredUsers = new List<ApplicationUser>();

            if (!string.IsNullOrEmpty(viewModel.Search))
            {
                foreach (var user in allUsers)
                {
                    if (user.Name != null && user.Name.Contains(viewModel.Search))
                    {
                        filteredUsers.Add(user);
                    }
                }
            }
            else
            {
                filteredUsers = allUsers;
            }
            viewModel.Result = filteredUsers;
            return View(viewModel);
        }

        // ShowUser: Visar en användares profil med deras broadcasts, filtrerat och sorterat manuellt.
        [Route("/Users/{id}")]
        public async Task<IActionResult> ShowUser(string id)
        {
            List<Broadcast> allBroadcasts = await _dbContext.Broadcasts.ToListAsync();
            List<Broadcast> userBroadcasts = new List<Broadcast>();

            foreach (var broadcast in allBroadcasts)
            {
                if (broadcast.User != null && broadcast.User.Id == id)
                {
                    userBroadcasts.Add(broadcast);
                }
            }

            // Sortera manuellt (nyaste först)
            userBroadcasts.Sort((a, b) => b.Published.CompareTo(a.Published));

            ApplicationUser foundUser = null;
            List<ApplicationUser> allUsers = await _dbContext.Users.ToListAsync();
            foreach (var user in allUsers)
            {
                if (user.Id == id)
                {
                    foundUser = user;
                    break;
                }
            }

            UsersShowUserViewModel viewModel = new UsersShowUserViewModel
            {
                Broadcasts = userBroadcasts,
                User = foundUser
            };

            return View(viewModel);
        }

        // ListenToUser: Lägger till en användare i den inloggade användarens ListeningTo-lista.
        [HttpPost, Route("/Users/Listen")]
        public async Task<IActionResult> ListenToUser(UsersListenToUserViewModel viewModel)
        {
            ApplicationUser loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                // Returnera istället en vy med ett meddelande
                return View("Index", new UsersIndexViewModel { Result = new List<ApplicationUser>() });
            }
            List<ApplicationUser> allUsers = await _dbContext.Users.ToListAsync();
            ApplicationUser userToListenTo = null;
            foreach (var user in allUsers)
            {
                if (user.Id == viewModel.UserId)
                {
                    userToListenTo = user;
                    break;
                }
            }

            if (userToListenTo != null)
            {
                if (loggedInUser.ListeningTo == null)
                {
                    loggedInUser.ListeningTo = new List<ApplicationUser>();
                }
                bool alreadyFollowing = false;
                foreach (var u in loggedInUser.ListeningTo)
                {
                    if (u.Id == userToListenTo.Id)
                    {
                        alreadyFollowing = true;
                        break;
                    }
                }
                if (!alreadyFollowing)
                {
                    loggedInUser.ListeningTo.Add(userToListenTo);
                    await _userManager.UpdateAsync(loggedInUser);
                    await _dbContext.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
        }

        // StopListenToUser: Tar bort en användare från ListeningTo-listan.
        [HttpPost, Route("/Users/StopListen")]
        public async Task<IActionResult> StopListenToUser(UsersListenToUserViewModel viewModel)
        {
            ApplicationUser loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                return RedirectToAction("Index");
            }
            List<ApplicationUser> allUsers = await _dbContext.Users.ToListAsync();
            ApplicationUser userToStopListenTo = null;
            foreach (var user in allUsers)
            {
                if (user.Id == viewModel.UserId)
                {
                    userToStopListenTo = user;
                    break;
                }
            }

            if (userToStopListenTo != null && loggedInUser.ListeningTo != null)
            {
                ApplicationUser removeUser = null;
                foreach (var u in loggedInUser.ListeningTo)
                {
                    if (u.Id == userToStopListenTo.Id)
                    {
                        removeUser = u;
                        break;
                    }
                }
                if (removeUser != null)
                {
                    loggedInUser.ListeningTo.Remove(removeUser);
                    await _userManager.UpdateAsync(loggedInUser);
                    await _dbContext.SaveChangesAsync();
                }
            }
            return RedirectToAction("Index");
        }
    }
}
