using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OazaDlaAutyzmu.Application.Queries.Admin;

namespace OazaDlaAutyzmu.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var query = new GetDashboardStatisticsQuery();
        var statistics = await _mediator.Send(query);
        return View(statistics);
    }

    [HttpGet]
    public IActionResult Users()
    {
        return View();
    }

    [HttpGet]
    public IActionResult AuditLogs()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Settings()
    {
        return View();
    }
}
