using FluentValidation;

namespace BlueBerryFinance.API.Application.Features.Category
{
    public class RegisterCategoryRequestValidator : AbstractValidator<RegisterCategoryRequest>
    {
        public RegisterCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.");

            RuleFor(x => x.Icon)
                .NotEmpty().WithMessage("Icon is required.");

            RuleFor(x => x.Color)
                .NotEmpty().WithMessage("Color is required.");

            RuleFor(x => x.Type)
                .Must(t => t == "Income" || t == "Expense")
                .WithMessage("Type must be 'Income' or 'Expense'.");
        }
    }
}
