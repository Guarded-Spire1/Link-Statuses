namespace LinkStatusDb.Models
{
    public class LinkStatusEntity
    {
        public int Id { get; set; } // Primary key
        public string Status { get; set; } = string.Empty; // Status of the link ("Available", "Unavailable")
        public DateTime CheckedAt { get; set; } // Timestamp of when the status was checked
        public required int LinkId { get; set; } // Foreign key to LinkEntity
        public LinkEntity? Link { get; set; } // Navigation property
    }
}
