using FluentValidation;
using OazaDlaAutyzmu.Application.Commands.Reviews;

namespace OazaDlaAutyzmu.Application.Validators.Reviews;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.FacilityId)
            .GreaterThan(0)
            .WithMessage("Musisz wybrać placówkę.");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("Użytkownik musi być zalogowany.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Ocena musi być w zakresie od 1 do 5.");

        RuleFor(x => x.Comment)
            .MaximumLength(2000)
            .WithMessage("Komentarz nie może być dłuższy niż 2000 znaków.")
            .When(x => !string.IsNullOrEmpty(x.Comment));
    }
}
