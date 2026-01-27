using MediatR;
using Microsoft.AspNetCore.Mvc;
using OazaDlaAutyzmu.Application.Queries.Facilities;

namespace OazaDlaAutyzmu.Web.Controllers.Api;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class FacilitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FacilitiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get list of all facilities with optional filtering
    /// </summary>
    /// <param name="search">Search term for name or city</param>
    /// <param name="city">Filter by city</param>
    /// <param name="type">Filter by facility type</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 12, max: 100)</param>
    [HttpGet]
    public async Task<IActionResult> GetFacilities(
        [FromQuery] string? search = null,
        [FromQuery] string? city = null,
        [FromQuery] string? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var query = new GetFacilitiesQuery
        {
            SearchTerm = search,
            City = city,
            PageNumber = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        
        return Ok(new
        {
            data = result.Items,
            pagination = new
            {
                currentPage = result.PageNumber,
                pageSize = result.PageSize,
                totalPages = result.TotalPages,
                totalItems = result.TotalCount,
                hasNextPage = result.HasNextPage,
                hasPreviousPage = result.HasPreviousPage
            }
        });
    }

    /// <summary>
    /// Get details of a specific facility
    /// </summary>
    /// <param name="id">Facility ID</param>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFacility(int id)
    {
        var query = new GetFacilityByIdQuery { Id = id };
        var facility = await _mediator.Send(query);

        if (facility == null)
        {
            return NotFound(new { message = $"Facility with ID {id} not found" });
        }

        return Ok(new { data = facility });
    }

    /// <summary>
    /// Get reviews for a specific facility
    /// </summary>
    /// <param name="id">Facility ID</param>
    [HttpGet("{id}/reviews")]
    public async Task<IActionResult> GetFacilityReviews(int id)
    {
        var facilityQuery = new GetFacilityByIdQuery { Id = id };
        var facility = await _mediator.Send(facilityQuery);

        if (facility == null)
        {
            return NotFound(new { message = $"Facility with ID {id} not found" });
        }

        // Get reviews separately using GetReviewsByFacilityQuery
        var reviewsQuery = new OazaDlaAutyzmu.Application.Queries.Reviews.GetReviewsByFacilityQuery { FacilityId = id };
        var reviews = await _mediator.Send(reviewsQuery);

        return Ok(new { data = reviews });
    }
}
