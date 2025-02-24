using Microsoft.AspNetCore.Http;

namespace BroadcastSocialMedia.ViewModels
{
    public class ProfileIndexViewModel
    {
        public string Name { get; set; }
        public string ImageFilename { get; set; }
        public IFormFile ProfileImageFile { get; set; }
        public string ErrorMessage { get; set; }
    }
}
