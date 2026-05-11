using BlueBerryFinance.API.Domain.Entities.Enums;
using BlueBerryFinance.Common.DomainObjects;

namespace BlueBerryFinance.API.Domain.Entities
{
    public class FiscalNote : EntityBase
    {
        public Guid UserId { get; set; }
        public Guid? TransactionId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string RawText { get; set; } = string.Empty;
        public string ExtractedData { get; set; } = "{}"; // JSON
        public FiscalNoteStatus Status { get; set; } = FiscalNoteStatus.Pending;

        public User User { get; set; } = null!;
        public Transaction? Transaction { get; set; }

        public FiscalNote() { }
    }
}
