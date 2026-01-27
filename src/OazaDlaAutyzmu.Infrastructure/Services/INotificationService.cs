using OazaDlaAutyzmu.Domain.Entities;

namespace OazaDlaAutyzmu.Infrastructure.Services;

public interface INotificationService
{
    Task CreateNotificationAsync(int userId, string type, string title, string message, string? url = null, CancellationToken cancellationToken = default);
    Task<List<Notification>> GetUserNotificationsAsync(int userId, bool unreadOnly = false, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);
    Task DeleteNotificationAsync(int notificationId, CancellationToken cancellationToken = default);
}
