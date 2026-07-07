using ItemsWorkService.Enums;

namespace ItemsWorkService.Models
{
    public class WorkItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime DeliveryDate { get; set; }
        public Relevance Relevance { get; set; }
        public ItemStatus Status { get; set; }
        public string AssignedUsername { get; set; }
    }
}
