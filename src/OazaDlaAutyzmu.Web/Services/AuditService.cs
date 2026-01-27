using OazaDlaAutyzmu.Domain.Entities;
using OazaDlaAutyzmu.Infrastructure.Data;

namespace OazaDlaAutyzmu.Web.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(string action, string entityType, int? entityId, int? userId, string? userEmail, string? oldValues, string? newValues, string? ipAddress)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                UserId = userId,
                UserEmail = userEmail,
                OldValues = oldValues,
                NewValues = newValues,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Audit log created: {Action} on {EntityType} (ID: {EntityId}) by {UserEmail}", 
                action, entityType, entityId, userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create audit log for action: {Action}", action);
        }
    }
}
