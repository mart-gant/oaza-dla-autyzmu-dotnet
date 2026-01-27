using Xunit;

namespace OazaDlaAutyzmu.Tests.E2E;

public class AuthenticationTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;

    public AuthenticationTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "Requires Playwright browsers installed. Run: playwright install")]
    public async Task UserCanRegister()
    {
        if (_fixture.Page == null)
        {
            // Skip if Playwright not initialized
            return;
        }

        var page = _fixture.Page;
        
        // Navigate to registration page
        await page.GotoAsync("/Account/Register");
        
        // Fill registration form
        var timestamp = DateTime.UtcNow.Ticks;
        await page.FillAsync("#Input_Email", $"test{timestamp}@example.com");
        await page.FillAsync("#Input_FirstName", "Test");
        await page.FillAsync("#Input_LastName", "User");
        await page.FillAsync("#Input_Password", "Test123!");
        await page.FillAsync("#Input_ConfirmPassword", "Test123!");
        
        // Submit form
        await page.ClickAsync("button[type='submit']");
        
        // Wait for redirect to confirmation page
        await page.WaitForURLAsync("**/Account/RegisterConfirmation**");
        
        // Verify success message
        var successMessage = await page.Locator("text=Sprawdź swoją skrzynkę email").IsVisibleAsync();
        Assert.True(successMessage);
    }

    [Fact(Skip = "Requires Playwright browsers installed. Run: playwright install")]
    public async Task UserCanLogin()
    {
        if (_fixture.Page == null)
        {
            return;
        }

        var page = _fixture.Page;
        
        // Navigate to login page
        await page.GotoAsync("/Account/Login");
        
        // Fill login form with seeded test user
        await page.FillAsync("#Input_Email", "test@oaza.pl");
        await page.FillAsync("#Input_Password", "Test123!");
        
        // Submit form
        await page.ClickAsync("button[type='submit']");
        
        // Wait for redirect to home page
        await page.WaitForURLAsync("**/");
        
        // Verify user is logged in (check for user menu or logout button)
        var userMenu = await page.Locator("text=Wyloguj").IsVisibleAsync();
        Assert.True(userMenu);
    }

    [Fact(Skip = "Requires Playwright browsers installed. Run: playwright install")]
    public async Task AdminCanAccessDashboard()
    {
        if (_fixture.Page == null)
        {
            return;
        }

        var page = _fixture.Page;
        
        // Login as admin
        await page.GotoAsync("/Account/Login");
        await page.FillAsync("#Input_Email", "admin@oaza.pl");
        await page.FillAsync("#Input_Password", "Admin123!");
        await page.ClickAsync("button[type='submit']");
        
        // Navigate to admin dashboard
        await page.GotoAsync("/Admin/Dashboard");
        
        // Verify dashboard content
        var dashboardTitle = await page.Locator("h1:has-text('Panel Administracyjny')").IsVisibleAsync();
        Assert.True(dashboardTitle);
        
        // Verify statistics cards are present
        var facilitiesCard = await page.Locator("text=Placówki").IsVisibleAsync();
        var usersCard = await page.Locator("text=Użytkownicy").IsVisibleAsync();
        
        Assert.True(facilitiesCard);
        Assert.True(usersCard);
    }
}
