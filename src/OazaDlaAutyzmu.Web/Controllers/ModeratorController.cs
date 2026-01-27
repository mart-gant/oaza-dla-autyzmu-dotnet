using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OazaDlaAutyzmu.Application.Commands.Forum;
using OazaDlaAutyzmu.Application.Commands.Reviews;
using OazaDlaAutyzmu.Application.Queries.Reviews;
using OazaDlaAutyzmu.Infrastructure.Data;
using OazaDlaAutyzmu.Infrastructure.Services;
using OazaDlaAutyzmu.Web.Services;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace OazaDlaAutyzmu.Web.Controllers;

[Authorize(Roles = "Admin,Moderator")]
public class ModeratorController : Controller
{
    private readonly IMediator _mediator;
    private readonly IAuditService _auditService;
    private readonly INotificationService _notificationService;
    private readonly ApplicationDbContext _context;

    public ModeratorController(
        IMediator mediator, 
        IAuditService auditService, 
        INotificationService notificationService,
        ApplicationDbContext context)
    {
        _mediator = mediator;
        _auditService = auditService;
        _notificationService = notificationService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View();
    }

    // Reviews Management
    [HttpGet]
    public async Task<IActionResult> PendingReviews()
    {
        var query = new GetPendingReviewsQuery();
        var reviews = await _mediator.Send(query);
        return View(reviews);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveReview(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int moderatorId))
        {
            return Unauthorized();
        }

        // Get review to find author
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono opinii.";
            return RedirectToAction(nameof(PendingReviews));
        }

        var command = new ApproveReviewCommand
        {
            ReviewId = id,
            ModeratorId = moderatorId
        };

        var success = await _mediator.Send(command);
        
        if (success)
        {
            await _auditService.LogAsync("Review_Approve", "Review", id, moderatorId, User.Identity?.Name, 
                null, $"Review {id} approved", HttpContext.Connection.RemoteIpAddress?.ToString());
            
            // Create notification for review author
            await _notificationService.CreateNotificationAsync(
                review.UserId,
                "ReviewApproved",
                "Twoja opinia została zatwierdzona",
                "Moderator zatwierdził Twoją opinię. Jest ona teraz widoczna publicznie.",
                $"/Facilities/Details/{review.FacilityId}");
            
            TempData["SuccessMessage"] = "Opinia została zatwierdzona.";
        }
        else
            TempData["ErrorMessage"] = "Nie udało się zatwierdzić opinii.";

        return RedirectToAction(nameof(PendingReviews));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectReview(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int moderatorId))
        {
            return Unauthorized();
        }

        // Get review to find author
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
        {
            TempData["ErrorMessage"] = "Nie znaleziono opinii.";
            return RedirectToAction(nameof(PendingReviews));
        }

        var reviewUserId = review.UserId;

        var command = new RejectReviewCommand { ReviewId = id };
        var success = await _mediator.Send(command);
        
        if (success)
        {
            await _auditService.LogAsync("Review_Reject", "Review", id, moderatorId, User.Identity?.Name, 
                null, $"Review {id} rejected and deleted", HttpContext.Connection.RemoteIpAddress?.ToString());
            
            // Create notification for review author
            await _notificationService.CreateNotificationAsync(
                reviewUserId,
                "ReviewRejected",
                "Twoja opinia została odrzucona",
                "Moderator odrzucił Twoją opinię. Może ona naruszać regulamin lub zawierać niewłaściwą treść.",
                null);
            
            TempData["SuccessMessage"] = "Opinia została odrzucona.";
        }
        else
            TempData["ErrorMessage"] = "Nie udało się odrzucić opinii.";

        return RedirectToAction(nameof(PendingReviews));
    }

    // Forum Topic Management
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTopicPin(int id, int categoryId)
    {
        var command = new ToggleTopicPinCommand { TopicId = id };
        var success = await _mediator.Send(command);
        
        if (success)
            TempData["SuccessMessage"] = "Status przypięcia tematu został zmieniony.";
        else
            TempData["ErrorMessage"] = "Nie udało się zmienić statusu tematu.";

        return RedirectToAction("Category", "Forum", new { id = categoryId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTopicLock(int id)
    {
        var command = new ToggleTopicLockCommand { TopicId = id };
        var success = await _mediator.Send(command);
        
        if (success)
            TempData["SuccessMessage"] = "Status blokady tematu został zmieniony.";
        else
            TempData["ErrorMessage"] = "Nie udało się zmienić statusu tematu.";

        return RedirectToAction("Topic", "Forum", new { id });
    }
}
