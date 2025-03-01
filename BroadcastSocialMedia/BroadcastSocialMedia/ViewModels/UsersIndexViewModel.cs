using BroadcastSocialMedia.Models;

namespace BroadcastSocialMedia.ViewModels
{
    public class UsersIndexViewModel
    {
        public string Search { get; set; }
        public List<ApplicationUser> Result { get; set; } = new List<ApplicationUser>();
        public List<string> FollowingId { get; set; } = new List<string>();
        public List<ApplicationUser> RecommendedUsers { get; set; } = new List<ApplicationUser>();
        public string ErrorMessage { get; set; }
        public List<ApplicationUser> FollowingUsers { get; set; } = new List<ApplicationUser>();
    }
}
