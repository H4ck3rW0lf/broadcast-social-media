namespace BroadcastSocialMedia.Models
{
    public class Broadcast
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime Published { get; set; } = DateTime.Now;
        public string ImageFilename { get; set; }
        public ICollection<UserThatLikeBroadcast> UserThatLikeBroadcasts { get; set; } = new List<UserThatLikeBroadcast>();
    }
}

