namespace LinkStatusDb.Models
{
    public class LinkEntity
    {
        public int Id { get; set; } // Primary key
        public string Url { get; set; } = string.Empty; // URL's of the link
        public required long UserId { get; set; } // Foreign key to UserEntity
        public UserEntity? User { get; set; }// Navigation property
        public List<LinkStatusEntity> LinkStatus { get; set; } = []; // Navigation property
    }
}
