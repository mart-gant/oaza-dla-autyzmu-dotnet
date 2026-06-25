using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OazaDlaAutyzmu.Infrastructure.Data;
using OazaDlaAutyzmu.Web.Controllers.Api;
using Xunit;

namespace OazaDlaAutyzmu.Tests.Integration;

public class IdentityApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public IdentityApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("UseInMemoryDatabase", "true");
            builder.ConfigureServices(services =>
            {
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterApiRequest
        {
            Email = $"test-{Guid.NewGuid()}@oaza.pl",
            Password = "Password123!",
            FirstName = "Jan",
            LastName = "Kowalski"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(body);
        Assert.True(body.ContainsKey("message"));
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var email = $"test-{Guid.NewGuid()}@oaza.pl";
        var request = new RegisterApiRequest
        {
            Email = email,
            Password = "Password123!",
            FirstName = "Jan",
            LastName = "Kowalski"
        };

        // First registration
        await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Act - Second registration with the same email
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var email = $"test-{Guid.NewGuid()}@oaza.pl";
        var registerRequest = new RegisterApiRequest
        {
            Email = email,
            Password = "Password123!",
            FirstName = "Jan",
            LastName = "Kowalski"
        };

        // Register the user
        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = new LoginApiRequest
        {
            Email = email,
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("accessToken", content);
        Assert.Contains("tokenType", content);
        Assert.Contains("expiresIn", content);
        Assert.Contains("refreshToken", content);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginApiRequest
        {
            Email = "nonexistent@oaza.pl",
            Password = "WrongPassword!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithValidToken_ReturnsUserData()
    {
        // Arrange - Register and login to get bearer token
        var email = $"test-{Guid.NewGuid()}@oaza.pl";
        var registerRequest = new RegisterApiRequest
        {
            Email = email,
            Password = "Password123!",
            FirstName = "Jan",
            LastName = "Kowalski"
        };

        await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);

        var loginRequest = new LoginApiRequest
        {
            Email = email,
            Password = "Password123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var tokenInfo = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(tokenInfo);
        
        var accessToken = tokenInfo["accessToken"].ToString();
        Assert.NotNull(accessToken);

        // Create authenticated client
        var authClient = _factory.CreateClient();
        authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Act
        var response = await authClient.GetAsync("/api/v1/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var userData = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(userData);
        Assert.Equal(email, userData["email"].ToString());
        Assert.Equal("Jan", userData["firstName"].ToString());
        Assert.Equal("Kowalski", userData["lastName"].ToString());
    }
}
