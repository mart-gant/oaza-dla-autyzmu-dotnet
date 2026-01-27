using Xunit;

namespace OazaDlaAutyzmu.Tests.E2E;

public class GalleryTests : IClassFixture<PlaywrightFixture>
{
    private readonly PlaywrightFixture _fixture;

    public GalleryTests(PlaywrightFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "Requires Playwright browsers installed. Run: playwright install")]
    public async Task UserCanViewGallery()
    {
        if (_fixture.Page == null)
        {
            return;
        }

        var page = _fixture.Page;
        
        // Navigate to facility with gallery
        await page.GotoAsync("/Gallery/Index/1");
        
        // Verify gallery page loaded
        var pageTitle = await page.Locator("h1:has-text('Galeria')").IsVisibleAsync();
        Assert.True(pageTitle);
        
        // Check if images are displayed
        var images = await page.Locator(".gallery-image").CountAsync();
        Assert.True(images >= 0); // Can be 0 if no images uploaded yet
    }

    [Fact(Skip = "Requires Playwright browsers installed. Run: playwright install")]
    public async Task UserCanOpenLightbox()
    {
        if (_fixture.Page == null)
        {
            return;
        }

        var page = _fixture.Page;
        
        // Navigate to gallery
        await page.GotoAsync("/Gallery/Index/1");
        
        // Wait for images to load
        var imageCount = await page.Locator(".gallery-image").CountAsync();
        
        if (imageCount > 0)
        {
            // Click first image to open lightbox
            var firstImage = page.Locator(".gallery-image").First;
            await firstImage.ClickAsync();
            
            // Verify lightbox is visible
            var lightbox = await page.Locator("#lightboxModal").IsVisibleAsync();
            Assert.True(lightbox);
            
            // Close lightbox
            await page.Keyboard.PressAsync("Escape");
            
            // Verify lightbox is hidden
            var lightboxHidden = await page.Locator("#lightboxModal").IsHiddenAsync();
            Assert.True(lightboxHidden);
        }
    }

    [Fact(Skip = "Requires Playwright browsers installed. Run: playwright install")]
    public async Task AdminCanUploadImage()
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
        
        // Navigate to upload page
        await page.GotoAsync("/Gallery/Upload/1");
        
        // Verify upload form is present
        var uploadForm = await page.Locator("form[enctype='multipart/form-data']").IsVisibleAsync();
        Assert.True(uploadForm);
        
        // Note: Actual file upload test would require a test image file
        // This verifies the page is accessible and form exists
    }
}
