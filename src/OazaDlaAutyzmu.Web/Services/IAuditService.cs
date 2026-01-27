using OazaDlaAutyzmu.Domain.Entities;

namespace OazaDlaAutyzmu.Web.Services;

public interface IAuditService
{
    Task LogAsync(string action, string entityType, int? entityId, int? userId, string? userEmail, string? oldValues, string? newValues, string? ipAddress);
}
