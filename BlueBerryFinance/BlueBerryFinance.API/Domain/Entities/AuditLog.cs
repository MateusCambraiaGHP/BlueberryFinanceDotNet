namespace BlueBerryFinance.API.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string CorrelationId { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public string Controller { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Payload { get; set; } = "{}";
        public string Response { get; set; } = "{}";
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }
        public DateTime InsertionDate { get; set; } = DateTime.UtcNow;
    }
}
