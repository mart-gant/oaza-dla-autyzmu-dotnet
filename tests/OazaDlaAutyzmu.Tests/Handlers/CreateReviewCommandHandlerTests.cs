using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Application.Commands.Reviews;
using OazaDlaAutyzmu.Domain.Entities;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Tests.Handlers;

public class CreateReviewCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly CreateReviewCommandHandler _handler;

    public CreateReviewCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _handler = new CreateReviewCommandHandler(_context);
    }

    [Fact]
    public async Task Handle_Should_Create_Review_Successfully()
    {
        // Arrange
        var facility = new Facility
        {
            Name = "Test Facility",
            Address = "Test Address",
            City = "Warszawa",
            PostalCode = "00-001",
            Type = FacilityType.Therapy,
            VerificationStatus = VerificationStatus.Verified
        };
        _context.Facilities.Add(facility);
        await _context.SaveChangesAsync();

        var command = new CreateReviewCommand
        {
            FacilityId = facility.Id,
            UserId = 1,
            Rating = 5,
            Comment = "Excellent facility!"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);
        
        var review = await _context.Reviews.FindAsync(result);
        review.Should().NotBeNull();
        review!.FacilityId.Should().Be(facility.Id);
        review.Rating.Should().Be(5);
        review.Comment.Should().Be("Excellent facility!");
        review.IsApproved.Should().BeFalse(); // Default should be false
    }

    [Fact]
    public async Task Handle_Should_Create_Review_Without_Comment()
    {
        // Arrange
        var facility = new Facility
        {
            Name = "Test Facility 2",
            Address = "Test Address 2",
            City = "Kraków",
            PostalCode = "30-001",
            Type = FacilityType.School,
            VerificationStatus = VerificationStatus.Verified
        };
        _context.Facilities.Add(facility);
        await _context.SaveChangesAsync();

        var command = new CreateReviewCommand
        {
            FacilityId = facility.Id,
            UserId = 2,
            Rating = 4,
            Comment = null
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeGreaterThan(0);
        
        var review = await _context.Reviews.FindAsync(result);
        review.Should().NotBeNull();
        review!.Comment.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
