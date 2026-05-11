namespace BlueBerryFinance.Common.ViewModels
{
    public class AgentApprovalViewModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public string Tool { get; set; } = string.Empty;
        public string Payload { get; set; } = "{}";
        public string Status { get; set; } = string.Empty;
        public DateTime? ResolvedAt { get; set; }
        public DateTime InsertionDate { get; set; }
    }
}
