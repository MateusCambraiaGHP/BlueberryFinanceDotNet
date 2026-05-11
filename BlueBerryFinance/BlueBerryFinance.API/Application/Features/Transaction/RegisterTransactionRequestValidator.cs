using FluentValidation;

namespace BlueBerryFinance.API.Application.Features.Transaction
{
    public class RegisterTransactionRequestValidator : AbstractValidator<RegisterTransactionRequest>
    {
        public RegisterTransactionRequestValidator()
        {
            RuleFor(x => x.BankAccountId)
                .NotEmpty().WithMessage("BankAccountId is required.");

            RuleFor(x => x.StoreId)
                .NotEmpty().WithMessage("StoreId is required.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required.");

            RuleFor(x => x.CurrencyId)
                .NotEmpty().WithMessage("CurrencyId is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.");
        }
    }
}
