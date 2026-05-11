using FluentValidation;

namespace BlueBerryFinance.API.Application.Features.Report
{
    public class GetMonthlyReportRequestValidator : AbstractValidator<GetMonthlyReportRequest>
    {
        public GetMonthlyReportRequestValidator()
        {
            RuleFor(x => x.Year)
                .InclusiveBetween(2000, 2100).WithMessage("Year must be between 2000 and 2100.");

            RuleFor(x => x.Month)
                .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12.");
        }
    }
}
