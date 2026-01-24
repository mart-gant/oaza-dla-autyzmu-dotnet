using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OazaDlaAutyzmu.Application.Commands.Reviews;
using OazaDlaAutyzmu.Application.Queries.Reviews;
using System.Security.Claims;

namespace OazaDlaAutyzmu.Web.Controllers;

public class ReviewsController : Controller
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateReviewCommand> _validator;

    public ReviewsController(IMediator mediator, IValidator<CreateReviewCommand> validator)
    {
        _mediator = mediator;
        _validator = validator;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(int facilityId, int rating, string? comment)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }

        var command = new CreateReviewCommand
        {
            FacilityId = facilityId,
            UserId = userId,
            Rating = rating,
            Comment = comment
        };

        var validationResult = await _validator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            TempData["ErrorMessage"] = string.Join(" ", validationResult.Errors.Select(e => e.ErrorMessage));
            return RedirectToAction("Details", "Facilities", new { id = facilityId });
        }

        await _mediator.Send(command);

        TempData["SuccessMessage"] = "Dziękujemy za dodanie opinii! Zostanie ona opublikowana po weryfikacji przez moderatora.";
        return RedirectToAction("Details", "Facilities", new { id = facilityId });
    }
}
