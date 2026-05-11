using BlueBerryFinance.Common.DomainObjects;

namespace BlueBerryFinance.API.Data.Entities
{
    public class Store : EntityBase
    {
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;

        public Category Category { get; set; } = null!;

        public Store() { }
    }
}
