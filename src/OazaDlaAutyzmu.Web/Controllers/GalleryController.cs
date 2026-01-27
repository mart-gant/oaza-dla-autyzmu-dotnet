using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Domain.Entities;
using OazaDlaAutyzmu.Infrastructure.Data;
using OazaDlaAutyzmu.Infrastructure.Services;

namespace OazaDlaAutyzmu.Web.Controllers;

[Authorize]
public class GalleryController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IImageService _imageService;
    private readonly ILogger<GalleryController> _logger;

    public GalleryController(
        ApplicationDbContext context,
        IWebHostEnvironment environment,
        IImageService imageService,
        ILogger<GalleryController> logger)
    {
        _context = context;
        _environment = environment;
        _imageService = imageService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(int facilityId)
    {
        var facility = await _context.Facilities
            .Include(f => f.VerifiedBy)
            .FirstOrDefaultAsync(f => f.Id == facilityId);

        if (facility == null)
            return NotFound();

        var images = await _context.FacilityImages
            .Where(i => i.FacilityId == facilityId)
            .OrderByDescending(i => i.IsMain)
            .ThenBy(i => i.DisplayOrder)
            .ToListAsync();

        ViewBag.Facility = facility;
        ViewBag.CanEdit = User.Identity?.IsAuthenticated == true && 
                          (User.IsInRole("Admin") || (facility.VerifiedById.HasValue && facility.VerifiedById.ToString() == User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value));

        return View(images);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> Upload(int facilityId)
    {
        var facility = await _context.Facilities.FindAsync(facilityId);
        if (facility == null)
            return NotFound();

        // Check if user owns the facility or is admin
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!User.IsInRole("Admin") && (!facility.VerifiedById.HasValue || facility.VerifiedById.ToString() != userId))
            return Forbid();

        return View(facility);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Moderator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(int facilityId, IFormFile file, string? caption)
    {
        var facility = await _context.Facilities.FindAsync(facilityId);
        if (facility == null)
            return NotFound();

        // Check ownership
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!User.IsInRole("Admin") && (!facility.VerifiedById.HasValue || facility.VerifiedById.ToString() != userId))
            return Forbid();

        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("file", "Proszę wybrać plik do przesłania.");
            return View(facility);
        }

        // Validate file
        if (!_imageService.IsValidImageFormat(file.FileName))
        {
            ModelState.AddModelError("file", "Nieprawidłowy format pliku. Dozwolone formaty: JPG, PNG, GIF, WebP.");
            return View(facility);
        }

        using var stream = file.OpenReadStream();
        if (_imageService.GetFileSizeInBytes(stream) > 5 * 1024 * 1024)
        {
            ModelState.AddModelError("file", "Plik jest zbyt duży. Maksymalny rozmiar to 5MB.");
            return View(facility);
        }

        try
        {
            // Save and optimize image
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "facilities", facilityId.ToString());
            var uniqueFileName = await _imageService.SaveImageAsync(stream, file.FileName, uploadsFolder);

            // Get next display order
            var maxOrder = await _context.FacilityImages
                .Where(i => i.FacilityId == facilityId)
                .MaxAsync(i => (int?)i.DisplayOrder) ?? 0;

            // Check if this should be the main image (first image)
            var isFirstImage = !await _context.FacilityImages.AnyAsync(i => i.FacilityId == facilityId);

            // Create database record
            var image = new FacilityImage
            {
                FacilityId = facilityId,
                ImageUrl = $"/uploads/facilities/{facilityId}/{uniqueFileName}",
                Caption = caption,
                DisplayOrder = maxOrder + 1,
                IsMain = isFirstImage,
                CreatedAt = DateTime.UtcNow
            };

            _context.FacilityImages.Add(image);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Zdjęcie zostało dodane pomyślnie.";
            return RedirectToAction(nameof(Index), new { facilityId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for facility {FacilityId}", facilityId);
            ModelState.AddModelError("", "Wystąpił błąd podczas przesyłania pliku.");
            return View(facility);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Moderator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetMain(int id)
    {
        var image = await _context.FacilityImages
            .Include(i => i.Facility)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (image == null)
            return NotFound();

        // Check ownership
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!User.IsInRole("Admin") && (!image.Facility.VerifiedById.HasValue || image.Facility.VerifiedById.ToString() != userId))
            return Forbid();

        // Unset all other main images for this facility
        var otherImages = await _context.FacilityImages
            .Where(i => i.FacilityId == image.FacilityId && i.Id != id && i.IsMain)
            .ToListAsync();

        foreach (var img in otherImages)
        {
            img.IsMain = false;
        }

        // Set this image as main
        image.IsMain = true;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Zdjęcie główne zostało zmienione.";
        return RedirectToAction(nameof(Index), new { facilityId = image.FacilityId });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Moderator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var image = await _context.FacilityImages
            .Include(i => i.Facility)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (image == null)
            return NotFound();

        // Check ownership
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!User.IsInRole("Admin") && (!image.Facility.VerifiedById.HasValue || image.Facility.VerifiedById.ToString() != userId))
            return Forbid();

        try
        {
            // Delete physical file using ImageService
            var filePath = Path.Combine(_environment.WebRootPath, image.ImageUrl.TrimStart('/'));
            await _imageService.DeleteImageAsync(filePath);

            // If this was the main image, set another image as main
            if (image.IsMain)
            {
                var nextMain = await _context.FacilityImages
                    .Where(i => i.FacilityId == image.FacilityId && i.Id != id)
                    .OrderBy(i => i.DisplayOrder)
                    .FirstOrDefaultAsync();

                if (nextMain != null)
                {
                    nextMain.IsMain = true;
                }
            }

            _context.FacilityImages.Remove(image);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Zdjęcie zostało usunięte.";
            return RedirectToAction(nameof(Index), new { facilityId = image.FacilityId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image {ImageId}", id);
            TempData["Error"] = "Wystąpił błąd podczas usuwania zdjęcia.";
            return RedirectToAction(nameof(Index), new { facilityId = image.FacilityId });
        }
    }
}
