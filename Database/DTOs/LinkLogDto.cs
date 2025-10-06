

namespace LinkStatusDb.DTOs
{
    public class LinkLogDto //Data transfer object
    {
        public required string LinkUrl { get; set; }
        public required List<string> Status { get; set; } 
        public required List<int> LogId { get; set; } 
        public required List<DateTime> LogTime { get; set; }
    }
}
