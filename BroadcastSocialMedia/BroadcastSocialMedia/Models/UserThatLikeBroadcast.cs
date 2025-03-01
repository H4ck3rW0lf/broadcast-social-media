using System.ComponentModel.DataAnnotations;

namespace BroadcastSocialMedia.Models
{
    public class UserThatLikeBroadcast
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string NameOfUserThatLike { get; set; }
        public int BroadcastId { get; set; }
    }
}