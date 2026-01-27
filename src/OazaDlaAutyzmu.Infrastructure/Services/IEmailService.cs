namespace OazaDlaAutyzmu.Infrastructure.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
    Task SendContactResponseAsync(string recipientEmail, string recipientName, string facilityName, string message);
    Task SendReviewApprovedNotificationAsync(string recipientEmail, string facilityName);
}
