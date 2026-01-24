using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OazaDlaAutyzmu.Application.Commands.Facilities;
using OazaDlaAutyzmu.Application.Queries.Facilities;
using OazaDlaAutyzmu.Application.Queries.Reviews;
using OazaDlaAutyzmu.Domain.Entities;

namespace OazaDlaAutyzmu.Web.Controllers;

public class FacilitiesController : Controller
{
    private readonly IMediator _mediator;

    public FacilitiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? city, FacilityType? type, VerificationStatus? status, string? search, int pageNumber = 1)
    {
        var query = new GetFacilitiesQuery 
        { 
            City = city, 
            Type = type,
            Status = status,
            SearchTerm = search,
            PageNumber = pageNumber,
            PageSize = 12
        };
        
        var result = await _mediator.Send(query);
        
        ViewBag.Cities = result.Items.Select(f => f.City).Distinct().OrderBy(c => c).ToList();
        ViewBag.CurrentCity = city;
        ViewBag.CurrentType = type;
        ViewBag.CurrentStatus = status;
        ViewBag.CurrentSearch = search;
        
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var query = new GetFacilityByIdQuery { Id = id };
        var facility = await _mediator.Send(query);
        
        if (facility == null)
            return NotFound();

        // Load reviews
        var reviewsQuery = new GetReviewsByFacilityQuery { FacilityId = id, OnlyApproved = true };
        var reviews = await _mediator.Send(reviewsQuery);
        ViewBag.Reviews = reviews;
            
        return View(facility);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Moderator")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Moderator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateFacilityCommand command)
    {
        if (!ModelState.IsValid)
            return View(command);

        try
        {
            var id = await _mediator.Send(command);
            TempData["Success"] = "Placówka została dodana pomyślnie!";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Błąd podczas dodawania placówki: {ex.Message}");
            return View(command);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> Edit(int id)
    {
        var query = new GetFacilityByIdQuery { Id = id };
        var facility = await _mediator.Send(query);
        
        if (facility == null)
            return NotFound();

        var command = new UpdateFacilityCommand
        {
            Id = facility.Id,
            Name = facility.Name,
            Description = facility.Description,
            Address = facility.Address,
            City = facility.City,
            PostalCode = facility.PostalCode,
            PhoneNumber = facility.PhoneNumber,
            Email = facility.Email,
            Website = facility.Website,
            Type = Enum.Parse<FacilityType>(facility.Type),
            Latitude = facility.Latitude,
            Longitude = facility.Longitude
        };
            
        return View(command);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Moderator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateFacilityCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(command);

        try
        {
            await _mediator.Send(command);
            TempData["Success"] = "Placówka została zaktualizowana!";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Błąd podczas aktualizacji: {ex.Message}");
            return View(command);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var command = new DeleteFacilityCommand { Id = id };
            await _mediator.Send(command);
            TempData["Success"] = "Placówka została usunięta!";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Błąd podczas usuwania: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
