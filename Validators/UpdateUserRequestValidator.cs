using FluentValidation;
using TelegramWebAPI.Models.Requests;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Nickname)
            .MaximumLength(30)
            .When(x => x.Nickname != null);

        RuleFor(x => x.DateOfBirth.Value)
            .LessThan(DateTime.Today)
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+\d{10,15}$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

    }
}