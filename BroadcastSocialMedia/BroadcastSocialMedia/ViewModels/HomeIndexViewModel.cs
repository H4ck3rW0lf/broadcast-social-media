using BroadcastSocialMedia.Models;

namespace BroadcastSocialMedia.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Broadcast> Broadcasts { get; set; }
        public string ErrorMessage { get; set; }
    }
}
