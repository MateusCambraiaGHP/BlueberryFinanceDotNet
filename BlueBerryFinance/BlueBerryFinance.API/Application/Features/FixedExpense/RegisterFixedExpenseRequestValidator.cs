using FluentValidation;

namespace BlueBerryFinance.API.Application.Features.FixedExpense
{
    public class RegisterFixedExpenseRequestValidator : AbstractValidator<RegisterFixedExpenseRequest>
    {
        public RegisterFixedExpenseRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.");

            RuleFor(x => x.CurrencyId)
                .NotEmpty().WithMessage("CurrencyId is required.");

            RuleFor(x => x.StoreId)
                .NotEmpty().WithMessage("StoreId is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.DayOfMonth)
                .InclusiveBetween(1, 31).WithMessage("DayOfMonth must be between 1 and 31.");
        }
    }
}
