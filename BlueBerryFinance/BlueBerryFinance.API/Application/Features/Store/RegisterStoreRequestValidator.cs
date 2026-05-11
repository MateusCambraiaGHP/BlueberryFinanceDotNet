using FluentValidation;

namespace BlueBerryFinance.API.Application.Features.Store
{
    public class RegisterStoreRequestValidator : AbstractValidator<RegisterStoreRequest>
    {
        public RegisterStoreRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required.");
        }
    }
}
