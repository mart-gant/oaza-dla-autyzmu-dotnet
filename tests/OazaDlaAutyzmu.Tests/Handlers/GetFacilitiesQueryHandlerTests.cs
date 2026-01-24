using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Application.Queries.Facilities;
using OazaDlaAutyzmu.Domain.Entities;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Tests.Handlers;

public class GetFacilitiesQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly GetFacilitiesQueryHandler _handler;

    public GetFacilitiesQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _handler = new GetFacilitiesQueryHandler(_context);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var facilities = new[]
        {
            new Facility
            {
                Name = "Centrum Terapii Warszawa",
                Address = "ul. Testowa 1",
                City = "Warszawa",
                PostalCode = "00-001",
                Type = FacilityType.Therapy,
                VerificationStatus = VerificationStatus.Verified,
                Description = "Terapia dzieci z autyzmem"
            },
            new Facility
            {
                Name = "Szkoła Specjalna Kraków",
                Address = "ul. Testowa 2",
                City = "Kraków",
                PostalCode = "30-001",
                Type = FacilityType.School,
                VerificationStatus = VerificationStatus.Certified,
                Description = "Szkoła dla dzieci ze specjalnymi potrzebami"
            },
            new Facility
            {
                Name = "Centrum Wsparcia Warszawa",
                Address = "ul. Testowa 3",
                City = "Warszawa",
                PostalCode = "00-002",
                Type = FacilityType.SupportCenter,
                VerificationStatus = VerificationStatus.Unverified
            }
        };

        _context.Facilities.AddRange(facilities);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_Should_Return_All_Facilities_When_No_Filter()
    {
        // Arrange
        var query = new GetFacilitiesQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_Should_Filter_By_City()
    {
        // Arrange
        var query = new GetFacilitiesQuery { City = "Warszawa" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().OnlyContain(f => f.City == "Warszawa");
    }

    [Fact]
    public async Task Handle_Should_Filter_By_Type()
    {
        // Arrange
        var query = new GetFacilitiesQuery { Type = FacilityType.Therapy };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.First().Name.Should().Be("Centrum Terapii Warszawa");
    }

    [Fact]
    public async Task Handle_Should_Filter_By_Status()
    {
        // Arrange
        var query = new GetFacilitiesQuery { Status = VerificationStatus.Certified };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.First().VerificationStatus.Should().Be("Certified");
    }

    [Fact]
    public async Task Handle_Should_Search_By_Term()
    {
        // Arrange
        var query = new GetFacilitiesQuery { SearchTerm = "Terapia" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.First().Name.Should().Contain("Terapii");
    }

    [Fact]
    public async Task Handle_Should_Apply_Pagination()
    {
        // Arrange
        var query = new GetFacilitiesQuery { PageNumber = 1, PageSize = 2 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task Handle_Should_Combine_Multiple_Filters()
    {
        // Arrange
        var query = new GetFacilitiesQuery 
        { 
            City = "Warszawa",
            Type = FacilityType.Therapy
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(1);
        result.Items.First().Name.Should().Be("Centrum Terapii Warszawa");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
