namespace BlueBerryFinance.Common.ViewModels
{
    public class StoreViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool Active { get; set; }

        public StoreViewModel(
            Guid id,
            string name,
            Guid categoryId,
            string categoryName,
            bool active)
        {
            Id = id;
            Name = name;
            CategoryId = categoryId;
            CategoryName = categoryName;
            Active = active;
        }
    }
}
