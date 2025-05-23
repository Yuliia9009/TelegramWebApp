using FluentValidation;
using TelegramWebAPI.Models.Requests; 
public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Nickname)
            .NotEmpty().WithMessage("Ник обязателен")
            .MaximumLength(100).WithMessage("Ник не должен превышать 100 символов");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today).WithMessage("Дата рождения должна быть в прошлом");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Номер телефона обязателен")
            .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Неверный формат номера телефона");
    }
}