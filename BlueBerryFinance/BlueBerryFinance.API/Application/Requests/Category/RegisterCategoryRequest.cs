namespace BlueBerryFinance.API.Application.Requests.Category
{
    public class RegisterCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Type { get; set; } = "Expense"; // Income | Expense
    }
}
