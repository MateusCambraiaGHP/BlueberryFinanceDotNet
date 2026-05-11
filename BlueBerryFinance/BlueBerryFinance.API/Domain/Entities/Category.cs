using BlueBerryFinance.Common.DomainObjects;

namespace BlueBerryFinance.API.Domain.Entities
{
    public class Category : EntityBase
    {
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Type { get; set; } = "Expense"; // Income | Expense

        public ICollection<Store> Stores { get; set; } = [];

        public Category() { }
    }
}
