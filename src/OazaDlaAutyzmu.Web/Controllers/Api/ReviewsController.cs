using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OazaDlaAutyzmu.Application.Commands.Reviews;
using OazaDlaAutyzmu.Application.Queries.Reviews;
using System.Security.Claims;

namespace OazaDlaAutyzmu.Web.Controllers.Api;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get list of all approved reviews
    /// </summary>
    /// <param name="facilityId">Optional filter by facility ID</param>
    [HttpGet]
    public async Task<IActionResult> GetReviews([FromQuery] int? facilityId = null)
    {
        if (facilityId.HasValue)
        {
            var query = new GetReviewsByFacilityQuery { FacilityId = facilityId.Value };
            var reviews = await _mediator.Send(query);
            return Ok(new { data = reviews });
        }

        // Get all approved reviews (Note: would need a query for this)
        return Ok(new { data = new List<object>(), message = "Use facilityId parameter to filter reviews" });
    }

    /// <summary>
    /// Create a new review (requires authentication)
    /// </summary>
    /// <param name="request">Review data</param>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var command = new CreateReviewCommand
        {
            FacilityId = request.FacilityId,
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        try
        {
            var reviewId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetReviews), new { facilityId = request.FacilityId }, 
                new { data = new { id = reviewId, message = "Review created successfully and is pending approval" } });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class CreateReviewRequest
{
    public int FacilityId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
