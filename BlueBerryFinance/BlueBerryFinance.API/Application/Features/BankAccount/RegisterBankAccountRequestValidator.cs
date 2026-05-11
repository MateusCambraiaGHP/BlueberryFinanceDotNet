using FluentValidation;

namespace BlueBerryFinance.API.Application.Features.BankAccount
{
    public class RegisterBankAccountRequestValidator : AbstractValidator<RegisterBankAccountRequest>
    {
        public RegisterBankAccountRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.");

            RuleFor(x => x.CurrencyId)
                .NotEmpty().WithMessage("CurrencyId is required.");

            RuleFor(x => x.InitialBalance)
                .GreaterThanOrEqualTo(0).WithMessage("Initial balance cannot be negative.");
        }
    }
}
