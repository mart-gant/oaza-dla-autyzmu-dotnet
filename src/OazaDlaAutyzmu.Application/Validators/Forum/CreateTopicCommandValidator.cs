using FluentValidation;
using OazaDlaAutyzmu.Application.Commands.Forum;

namespace OazaDlaAutyzmu.Application.Validators.Forum;

public class CreateTopicCommandValidator : AbstractValidator<CreateTopicCommand>
{
    public CreateTopicCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Musisz wybrać kategorię.");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("Użytkownik musi być zalogowany.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Tytuł tematu jest wymagany.")
            .MinimumLength(5)
            .WithMessage("Tytuł musi mieć co najmniej 5 znaków.")
            .MaximumLength(200)
            .WithMessage("Tytuł nie może być dłuższy niż 200 znaków.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Treść tematu jest wymagana.")
            .MinimumLength(10)
            .WithMessage("Treść musi mieć co najmniej 10 znaków.")
            .MaximumLength(10000)
            .WithMessage("Treść nie może być dłuższa niż 10000 znaków.");
    }
}
