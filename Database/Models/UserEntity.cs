namespace LinkStatusDb.Models
{
    public class UserEntity
    {
        public long Id { get; set; } // Primary key
        public bool Subscription { get; set; } = false; //Subscription to broadcast
        public List<LinkEntity> Link { get; set; } = []; // Navigation property
    }
}
