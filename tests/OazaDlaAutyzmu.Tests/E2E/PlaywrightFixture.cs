using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Playwright;

namespace OazaDlaAutyzmu.Tests.E2E;

public class PlaywrightFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private WebApplicationFactory<Program>? _factory;
    public IBrowserContext? Context { get; private set; }
    public IPage? Page { get; private set; }
    public string BaseUrl { get; private set; } = "http://localhost:5050";

    public async Task InitializeAsync()
    {
        // Start web application
        _factory = new WebApplicationFactory<Program>();
        var client = _factory.CreateClient();
        
        try
        {
            // Initialize Playwright
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
            
            Context = await _browser.NewContextAsync(new BrowserNewContextOptions
            {
                BaseURL = BaseUrl,
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
            });
            
            Page = await Context.NewPageAsync();
        }
        catch (Exception ex)
        {
            // If Playwright browsers are not installed, skip E2E tests
            Console.WriteLine($"Warning: Could not initialize Playwright: {ex.Message}");
            Console.WriteLine("Run 'playwright install' to enable E2E tests");
        }
    }

    public async Task DisposeAsync()
    {
        if (Page != null)
            await Page.CloseAsync();
        
        if (Context != null)
            await Context.CloseAsync();
        
        if (_browser != null)
            await _browser.CloseAsync();
        
        _playwright?.Dispose();
        
        if (_factory != null)
            await _factory.DisposeAsync();
    }
}
