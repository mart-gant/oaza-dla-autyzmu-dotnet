using FluentValidation;
using OazaDlaAutyzmu.Application.Commands.Forum;

namespace OazaDlaAutyzmu.Application.Validators.Forum;

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.TopicId)
            .GreaterThan(0)
            .WithMessage("Musisz wybrać temat.");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("Użytkownik musi być zalogowany.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Treść posta jest wymagana.")
            .MinimumLength(3)
            .WithMessage("Treść musi mieć co najmniej 3 znaki.")
            .MaximumLength(10000)
            .WithMessage("Treść nie może być dłuższa niż 10000 znaków.");
    }
}
