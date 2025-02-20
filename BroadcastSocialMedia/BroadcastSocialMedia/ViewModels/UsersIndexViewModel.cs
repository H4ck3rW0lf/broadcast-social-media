using BroadcastSocialMedia.Models;

namespace BroadcastSocialMedia.ViewModels
{
    public class UsersIndexViewModel
    {
        public string Search { get; set; }
        public List<ApplicationUser> Result { get; set; } = new List<ApplicationUser>();
    }
}
