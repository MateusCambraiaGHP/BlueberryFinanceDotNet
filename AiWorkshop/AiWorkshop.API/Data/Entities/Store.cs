using BlueBerryFinance.API.Data.Entities.enums;
using BlueBerryFinance.Common.DomainObjects;

namespace BlueBerryFinance.API.Data.Entities
{
    public class Store : EntityBase
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public Category Category { get; set; }
        public Store() { }
    }
}
