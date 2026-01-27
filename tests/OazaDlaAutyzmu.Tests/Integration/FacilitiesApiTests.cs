using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OazaDlaAutyzmu.Infrastructure.Data;
using Xunit;

namespace OazaDlaAutyzmu.Tests.Integration;

public class FacilitiesApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public FacilitiesApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("UseInMemoryDatabase", "true");
            builder.ConfigureServices(services =>
            {
                // Seed test data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetFacilities_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/facilities");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetFacilities_ReturnsPaginatedData()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/facilities?page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"data\"", content);
        Assert.Contains("\"pagination\"", content);
    }

    [Fact]
    public async Task GetFacilities_WithSearchQuery_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/facilities?search=test");

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetFacilityById_WithValidId_ReturnsSuccess()
    {
        // Arrange - Create a test facility first
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var facility = new OazaDlaAutyzmu.Domain.Entities.Facility
        {
            Name = "Test Facility",
            Address = "Test Address",
            City = "Warsaw",
            Type = OazaDlaAutyzmu.Domain.Entities.FacilityType.Therapy,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(facility);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/facilities/{facility.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Test Facility", content);
    }

    [Fact]
    public async Task GetFacilityById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/facilities/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetFacilityReviews_ReturnsSuccess()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var facility = new OazaDlaAutyzmu.Domain.Entities.Facility
        {
            Name = "Test Facility",
            Address = "Test Address",
            City = "Warsaw",
            Type = OazaDlaAutyzmu.Domain.Entities.FacilityType.Therapy,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(facility);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/facilities/{facility.Id}/reviews");

        // Assert
        response.EnsureSuccessStatusCode();
    }
}
