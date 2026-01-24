using FluentAssertions;
using FluentValidation.TestHelper;
using OazaDlaAutyzmu.Application.Commands.Reviews;
using OazaDlaAutyzmu.Application.Validators.Reviews;

namespace OazaDlaAutyzmu.Tests.Validators;

public class CreateReviewCommandValidatorTests
{
    private readonly CreateReviewCommandValidator _validator;

    public CreateReviewCommandValidatorTests()
    {
        _validator = new CreateReviewCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_FacilityId_Is_Zero()
    {
        // Arrange
        var command = new CreateReviewCommand { FacilityId = 0, UserId = 1, Rating = 5 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FacilityId)
            .WithErrorMessage("Musisz wybrać placówkę.");
    }

    [Fact]
    public void Should_Have_Error_When_Rating_Is_Too_Low()
    {
        // Arrange
        var command = new CreateReviewCommand { FacilityId = 1, UserId = 1, Rating = 0 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Rating)
            .WithErrorMessage("Ocena musi być w zakresie od 1 do 5.");
    }

    [Fact]
    public void Should_Have_Error_When_Rating_Is_Too_High()
    {
        // Arrange
        var command = new CreateReviewCommand { FacilityId = 1, UserId = 1, Rating = 6 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Fact]
    public void Should_Have_Error_When_Comment_Is_Too_Long()
    {
        // Arrange
        var longComment = new string('a', 2001);
        var command = new CreateReviewCommand { FacilityId = 1, UserId = 1, Rating = 5, Comment = longComment };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Comment)
            .WithErrorMessage("Komentarz nie może być dłuższy niż 2000 znaków.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new CreateReviewCommand 
        { 
            FacilityId = 1, 
            UserId = 1, 
            Rating = 5,
            Comment = "Świetna placówka!"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Comment_Is_Null()
    {
        // Arrange
        var command = new CreateReviewCommand 
        { 
            FacilityId = 1, 
            UserId = 1, 
            Rating = 5,
            Comment = null
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Comment);
    }
}
