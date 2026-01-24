using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OazaDlaAutyzmu.Application.Commands.Forum;
using OazaDlaAutyzmu.Application.Commands.Reviews;
using OazaDlaAutyzmu.Application.Queries.Reviews;
using System.Security.Claims;

namespace OazaDlaAutyzmu.Web.Controllers;

[Authorize(Roles = "Admin,Moderator")]
public class ModeratorController : Controller
{
    private readonly IMediator _mediator;

    public ModeratorController(IMediator mediator)
    {
        _mediator = mediator;
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
    public async Task<IActionResult> ApproveReview(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int moderatorId))
        {
            return Unauthorized();
        }

        var command = new ApproveReviewCommand
        {
            ReviewId = id,
            ModeratorId = moderatorId
        };

        var success = await _mediator.Send(command);
        
        if (success)
            TempData["SuccessMessage"] = "Opinia została zatwierdzona.";
        else
            TempData["ErrorMessage"] = "Nie udało się zatwierdzić opinii.";

        return RedirectToAction(nameof(PendingReviews));
    }

    [HttpPost]
    public async Task<IActionResult> RejectReview(int id)
    {
        var command = new RejectReviewCommand { ReviewId = id };
        var success = await _mediator.Send(command);
        
        if (success)
            TempData["SuccessMessage"] = "Opinia została odrzucona.";
        else
            TempData["ErrorMessage"] = "Nie udało się odrzucić opinii.";

        return RedirectToAction(nameof(PendingReviews));
    }

    // Forum Topic Management
    [HttpPost]
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
