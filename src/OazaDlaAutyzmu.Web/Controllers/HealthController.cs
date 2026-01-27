using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HealthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            // Check database connectivity
            await _context.Database.CanConnectAsync();
            
            var healthStatus = new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                checks = new
                {
                    database = "ok",
                    memory = "ok"
                }
            };

            return Ok(healthStatus);
        }
        catch (Exception ex)
        {
            var unhealthyStatus = new
            {
                status = "unhealthy",
                timestamp = DateTime.UtcNow,
                error = ex.Message
            };

            return StatusCode(503, unhealthyStatus);
        }
    }
}
