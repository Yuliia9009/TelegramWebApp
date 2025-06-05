using FluentValidation;
using TelegramWebAPI.Models.Requests;

namespace TelegramWebAPI.Validators
{
    public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
    {
        public SendMessageRequestValidator()
        {
            // RuleFor(x => x.ChatId)
            //     .NotEmpty().WithMessage("ChatId обязателен");

            RuleFor(x => x.Text)
                .NotEmpty().WithMessage("Текст сообщения обязателен")
                .MaximumLength(1000).WithMessage("Сообщение не должно превышать 1000 символов");
        }
    }
}
