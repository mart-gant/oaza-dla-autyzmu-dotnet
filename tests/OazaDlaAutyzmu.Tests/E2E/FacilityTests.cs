using Xunit;

namespace OazaDlaAutyzmu.Tests.E2E;

public class FacilityTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;

    public FacilityTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "Requires Playwright browsers installed. Run: playwright install")]
    public async Task UserCanBrowseFacilities()
    {
        if (_fixture.Page == null)
        {
            return;
        }

        var page = _fixture.Page;
        
        // Navigate to facilities page
        await page.GotoAsync("/Facilities");
        
        // Verify page loaded
        var pageTitle = await page.Locator("h1:has-text('Placówki')").IsVisibleAsync();
        Assert.True(pageTitle);
        
        // Verify facilities list is present
        var facilitiesList = await page.Locator(".facility-card").CountAsync();
        Assert.True(facilitiesList > 0);
    }

    [Fact(Skip = "Requires Playwright browsers installed. Run: playwright install")]
    public async Task UserCanSearchFacilities()
    {
        if (_fixture.Page == null)
        {
            return;
        }

        var page = _fixture.Page;
        
        // Navigate to facilities page
        await page.GotoAsync("/Facilities");
        
        // Enter search query
        await page.FillAsync("input[name='search']", "Test");
        await page.ClickAsync("button[type='submit']");
        
        // Wait for results
        await page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
        
        // Verify search results contain the query
        var firstFacility = page.Locator(".facility-card").First;
        var facilityText = await firstFacility.TextContentAsync();
        Assert.Contains("Test", facilityText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(Skip = "Requires Playwright browsers installed. Run: playwright install")]
    public async Task UserCanViewFacilityDetails()
    {
        if (_fixture.Page == null)
        {
            return;
        }

        var page = _fixture.Page;
        
        // Navigate to facilities page
        await page.GotoAsync("/Facilities");
        
        // Click on first facility
        var firstFacility = page.Locator(".facility-card").First;
        await firstFacility.ClickAsync();
        
        // Wait for details page
        await page.WaitForURLAsync("**/Facilities/**");
        
        // Verify details are displayed
        var facilityName = await page.Locator("h1").IsVisibleAsync();
        var description = await page.Locator(".facility-description").IsVisibleAsync();
        var contactInfo = await page.Locator(".contact-info").IsVisibleAsync();
        
        Assert.True(facilityName);
        Assert.True(description);
        Assert.True(contactInfo);
    }

    [Fact(Skip = "Requires Playwright browsers installed. Run: playwright install")]
    public async Task LoggedInUserCanSubmitReview()
    {
        if (_fixture.Page == null)
        {
            return;
        }

        var page = _fixture.Page;
        
        // Login first
        await page.GotoAsync("/Account/Login");
        await page.FillAsync("#Input_Email", "test@oaza.pl");
        await page.FillAsync("#Input_Password", "Test123!");
        await page.ClickAsync("button[type='submit']");
        
        // Navigate to facility details
        await page.GotoAsync("/Facilities/1");
        
        // Scroll to review form
        await page.Locator("#review-form").ScrollIntoViewIfNeededAsync();
        
        // Fill review form
        await page.ClickAsync("input[name='Rating'][value='5']");
        await page.FillAsync("textarea[name='Comment']", "Świetna placówka! Bardzo pomocny personel.");
        
        // Submit review
        await page.ClickAsync("button[type='submit']:has-text('Dodaj opinię')");
        
        // Wait for success message
        var successMessage = await page.Locator(".alert-success:has-text('Dziękujemy')").IsVisibleAsync();
        Assert.True(successMessage);
    }
}
