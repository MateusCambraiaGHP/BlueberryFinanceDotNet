namespace BlueBerryFinance.Common.ViewModels
{
    public class CategoryViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Active { get; set; }

        public CategoryViewModel(
            Guid id,
            string name,
            string icon,
            string color,
            string type,
            bool active)
        {
            Id = id;
            Name = name;
            Icon = icon;
            Color = color;
            Type = type;
            Active = active;
        }
    }
}
