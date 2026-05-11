namespace BlueBerryFinance.API.Application.Features.Category
{
    public class RegisterCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Type { get; set; } = "Expense";
    }
}
