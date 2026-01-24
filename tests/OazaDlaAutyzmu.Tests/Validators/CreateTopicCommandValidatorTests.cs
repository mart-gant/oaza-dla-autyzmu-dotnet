using FluentValidation.TestHelper;
using OazaDlaAutyzmu.Application.Commands.Forum;
using OazaDlaAutyzmu.Application.Validators.Forum;

namespace OazaDlaAutyzmu.Tests.Validators;

public class CreateTopicCommandValidatorTests
{
    private readonly CreateTopicCommandValidator _validator;

    public CreateTopicCommandValidatorTests()
    {
        _validator = new CreateTopicCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Title_Is_Empty()
    {
        // Arrange
        var command = new CreateTopicCommand 
        { 
            CategoryId = 1, 
            UserId = 1, 
            Title = "", 
            Content = "Test content" 
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Have_Error_When_Title_Is_Too_Short()
    {
        // Arrange
        var command = new CreateTopicCommand 
        { 
            CategoryId = 1, 
            UserId = 1, 
            Title = "AB", 
            Content = "Test content" 
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Tytuł musi mieć co najmniej 5 znaków.");
    }

    [Fact]
    public void Should_Have_Error_When_Content_Is_Too_Short()
    {
        // Arrange
        var command = new CreateTopicCommand 
        { 
            CategoryId = 1, 
            UserId = 1, 
            Title = "Valid Title", 
            Content = "Short" 
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Treść musi mieć co najmniej 10 znaków.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new CreateTopicCommand 
        { 
            CategoryId = 1, 
            UserId = 1, 
            Title = "Valid Topic Title", 
            Content = "This is valid content with more than 10 characters." 
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
