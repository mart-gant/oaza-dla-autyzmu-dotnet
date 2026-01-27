using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Infrastructure.Data;
using OazaDlaAutyzmu.Web.Models;

namespace OazaDlaAutyzmu.Web.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var stats = new
        {
            TotalFacilities = await _context.Facilities.CountAsync(),
            VerifiedFacilities = await _context.Facilities
                .CountAsync(f => f.VerificationStatus == OazaDlaAutyzmu.Domain.Entities.VerificationStatus.Verified || 
                                 f.VerificationStatus == OazaDlaAutyzmu.Domain.Entities.VerificationStatus.Certified),
            TotalReviews = await _context.Reviews.CountAsync(),
            ForumTopics = await _context.ForumTopics.CountAsync()
        };

        var popularFacilities = await _context.Facilities
            .Include(f => f.Reviews)
            .Where(f => f.VerificationStatus == OazaDlaAutyzmu.Domain.Entities.VerificationStatus.Verified || 
                        f.VerificationStatus == OazaDlaAutyzmu.Domain.Entities.VerificationStatus.Certified)
            .OrderByDescending(f => f.Reviews.Count)
            .Take(6)
            .Select(f => new
            {
                f.Id,
                f.Name,
                f.City,
                Type = f.Type.ToString(),
                f.Description,
                ReviewCount = f.Reviews.Count,
                AverageRating = f.Reviews.Any() ? f.Reviews.Average(r => r.Rating) : 0
            })
            .ToListAsync();

        ViewBag.Stats = stats;
        ViewBag.PopularFacilities = popularFacilities;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
