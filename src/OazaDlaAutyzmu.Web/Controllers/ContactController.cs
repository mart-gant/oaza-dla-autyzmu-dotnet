using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Domain.Entities;
using OazaDlaAutyzmu.Infrastructure.Data;
using OazaDlaAutyzmu.Infrastructure.Services;

namespace OazaDlaAutyzmu.Web.Controllers;

public class ContactController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<ContactController> _logger;

    public ContactController(
        ApplicationDbContext context,
        IEmailService emailService,
        ILogger<ContactController> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int facilityId)
    {
        var facility = await _context.Facilities.FindAsync(facilityId);
        if (facility == null)
            return NotFound();

        return View(facility);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int facilityId, string senderName, string senderEmail, string subject, string message)
    {
        var facility = await _context.Facilities.FindAsync(facilityId);
        if (facility == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(senderName) || 
            string.IsNullOrWhiteSpace(senderEmail) || 
            string.IsNullOrWhiteSpace(subject) || 
            string.IsNullOrWhiteSpace(message))
        {
            ModelState.AddModelError("", "Wszystkie pola są wymagane.");
            return View("Index", facility);
        }

        // Validate email format
        if (!IsValidEmail(senderEmail))
        {
            ModelState.AddModelError("senderEmail", "Nieprawidłowy format adresu email.");
            return View("Index", facility);
        }

        // Validate length
        if (senderName.Length > 100)
        {
            ModelState.AddModelError("senderName", "Imię i nazwisko nie może przekraczać 100 znaków.");
            return View("Index", facility);
        }

        if (subject.Length > 200)
        {
            ModelState.AddModelError("subject", "Temat nie może przekraczać 200 znaków.");
            return View("Index", facility);
        }

        if (message.Length > 2000)
        {
            ModelState.AddModelError("message", "Wiadomość nie może przekraczać 2000 znaków.");
            return View("Index", facility);
        }

        try
        {
            var contactMessage = new ContactMessage
            {
                FacilityId = facilityId,
                SenderName = senderName,
                SenderEmail = senderEmail,
                Subject = subject,
                Message = message,
                IsRead = false,
                SentAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.ContactMessages.Add(contactMessage);
            await _context.SaveChangesAsync();

            // Create notification for facility owner
            var notification = new Notification
            {
                UserId = facility.VerifiedById ?? 1, // Default to admin if no owner
                Title = "Nowa wiadomość kontaktowa",
                Message = $"Otrzymałeś nową wiadomość od {senderName} dotyczącą placówki {facility.Name}",
                Type = "ContactMessage",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Wiadomość została wysłana pomyślnie. Właściciel placówki odpowie na podany adres email.";
            return RedirectToAction("Details", "Facilities", new { id = facilityId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending contact message for facility {FacilityId}", facilityId);
            ModelState.AddModelError("", "Wystąpił błąd podczas wysyłania wiadomości. Spróbuj ponownie później.");
            return View("Index", facility);
        }
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpGet]
    public async Task<IActionResult> Messages(int? facilityId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        IQueryable<ContactMessage> query = _context.ContactMessages
            .Include(m => m.Facility);

        // If not admin, only show messages for user's facilities
        if (!User.IsInRole("Admin"))
        {
            if (userId == null)
                return Forbid();

            var userIdInt = int.Parse(userId);
            query = query.Where(m => m.Facility.VerifiedById == userIdInt);
        }

        // Filter by facility if specified
        if (facilityId.HasValue)
        {
            query = query.Where(m => m.FacilityId == facilityId.Value);
        }

        var messages = await query
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();

        return View(messages);
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var message = await _context.ContactMessages
            .Include(m => m.Facility)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (message == null)
            return NotFound();

        // Check ownership
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!User.IsInRole("Admin") && (!message.Facility.VerifiedById.HasValue || message.Facility.VerifiedById.ToString() != userId))
            return Forbid();

        message.IsRead = true;
        message.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Messages));
    }

    [Authorize(Roles = "Admin,Moderator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var message = await _context.ContactMessages
            .Include(m => m.Facility)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (message == null)
            return NotFound();

        // Check ownership
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!User.IsInRole("Admin") && (!message.Facility.VerifiedById.HasValue || message.Facility.VerifiedById.ToString() != userId))
            return Forbid();

        _context.ContactMessages.Remove(message);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Wiadomość została usunięta.";
        return RedirectToAction(nameof(Messages));
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
