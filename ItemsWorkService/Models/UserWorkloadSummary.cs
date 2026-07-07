using ItemsWorkService.Enums;
namespace ItemsWorkService.Models
{
    public class UserWorkloadSummary
    {
        public string Username { get; set; }
        public List<WorkItem> Items { get; set; } = new();
        public int TotalItems => Items.Count;
        public int PendingCount => Items.Count(i => i.Status == ItemStatus.Pending);
        public int HighRelevanceCount => Items.Count(i => i.Relevance == Relevance.High && i.Status == ItemStatus.Pending);
    }
}
