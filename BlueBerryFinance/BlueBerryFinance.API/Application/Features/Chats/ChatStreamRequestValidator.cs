using FluentValidation;

namespace BlueBerryFinance.API.Application.Features.Chats
{
    public class ChatStreamRequestValidator : AbstractValidator<ChatStreamRequest>
    {
        public ChatStreamRequestValidator()
        {
            RuleFor(x => x.Prompt)
                .NotEmpty().WithMessage("Prompt must not be empty.");
        }
    }
}
