using Xunit;

namespace OazaDlaAutyzmu.Tests.E2E;

public class ContactTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;

    public ContactTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "Requires Playwright browsers installed. Run: playwright install")]
    public async Task UserCanSubmitContactForm()
    {
        if (_fixture.Page == null)
        {
            return;
        }

        var page = _fixture.Page;
        
        // Navigate to contact form
        await page.GotoAsync("/Contact/Index/1");
        
        // Verify form is present
        var formTitle = await page.Locator("h1:has-text('Kontakt')").IsVisibleAsync();
        Assert.True(formTitle);
        
        // Fill contact form
        await page.FillAsync("input[name='SenderName']", "Jan Kowalski");
        await page.FillAsync("input[name='SenderEmail']", "jan@example.com");
        await page.FillAsync("input[name='Subject']", "Pytanie o terapię");
        await page.FillAsync("textarea[name='Message']", "Dzień dobry, chciałbym zapytać o dostępność terapii.");
        
        // Submit form
        await page.ClickAsync("button[type='submit']:has-text('Wyślij')");
        
        // Wait for success message
        var successMessage = await page.Locator(".alert-success:has-text('Dziękujemy')").IsVisibleAsync();
        Assert.True(successMessage);
    }

    [Fact(Skip = "Requires Playwright browsers installed. Run: playwright install")]
    public async Task ContactFormValidatesRequiredFields()
    {
        if (_fixture.Page == null)
        {
            return;
        }

        var page = _fixture.Page;
        
        // Navigate to contact form
        await page.GotoAsync("/Contact/Index/1");
        
        // Try to submit empty form
        await page.ClickAsync("button[type='submit']:has-text('Wyślij')");
        
        // Verify validation errors are shown
        var validationErrors = await page.Locator(".text-danger, .invalid-feedback").CountAsync();
        Assert.True(validationErrors > 0);
    }

    [Fact(Skip = "Requires Playwright browsers installed. Run: playwright install")]
    public async Task CharacterCounterWorks()
    {
        if (_fixture.Page == null)
        {
            return;
        }

        var page = _fixture.Page;
        
        // Navigate to contact form
        await page.GotoAsync("/Contact/Index/1");
        
        // Type message and check character counter
        var messageText = "Test message";
        await page.FillAsync("textarea[name='Message']", messageText);
        
        // Verify character counter updates
        var counter = await page.Locator("#messageCounter").TextContentAsync();
        Assert.Contains(messageText.Length.ToString(), counter);
    }

    [Fact(Skip = "Requires Playwright browsers installed. Run: playwright install")]
    public async Task AdminCanViewContactMessages()
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
        
        // Navigate to messages page
        await page.GotoAsync("/Contact/Messages");
        
        // Verify messages table is present
        var messagesTable = await page.Locator("table").IsVisibleAsync();
        Assert.True(messagesTable);
        
        // Verify table headers
        var headers = await page.Locator("th").AllTextContentsAsync();
        Assert.Contains(headers, h => h.Contains("Data"));
        Assert.Contains(headers, h => h.Contains("Placówka"));
        Assert.Contains(headers, h => h.Contains("Nadawca"));
    }
}
