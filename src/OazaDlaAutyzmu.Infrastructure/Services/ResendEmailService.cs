using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;

namespace OazaDlaAutyzmu.Infrastructure.Services;

/// <summary>
/// Email service implementation using Resend API
/// https://resend.com/docs/send-with-dotnet
/// </summary>
public class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ResendEmailService> _logger;
    private readonly string _senderEmail;
    private readonly string _senderName;
    private readonly bool _isConfigured;

    public ResendEmailService(
        IResend resend,
        IConfiguration configuration,
        ILogger<ResendEmailService> logger)
    {
        _resend = resend;
        _configuration = configuration;
        _logger = logger;
        
        _senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "noreply@oaza.pl";
        _senderName = _configuration["EmailSettings:SenderName"] ?? "Oaza dla Autyzmu";
        
        var apiKey = _configuration["EmailSettings:ResendApiKey"];
        _isConfigured = !string.IsNullOrEmpty(apiKey);
        
        if (!_isConfigured)
        {
            _logger.LogWarning("Resend API key not configured. Emails will not be sent.");
        }
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        if (!_isConfigured)
        {
            _logger.LogWarning("Email not sent to {Email}. Resend not configured.", to);
            return;
        }

        try
        {
            var message = new EmailMessage
            {
                From = $"{_senderName} <{_senderEmail}>",
                To = to,
                Subject = subject,
                HtmlBody = htmlBody
            };

            var response = await _resend.EmailSendAsync(message);
            
            _logger.LogInformation("Email sent successfully to {Email}. MessageId: {MessageId}", 
                to, response.Data?.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} via Resend", to);
            // Don't throw - email failure shouldn't break the application
        }
    }

    public async Task SendContactResponseAsync(string recipientEmail, string recipientName, string facilityName, string message)
    {
        var subject = $"Odpowiedź z {facilityName} - Oaza dla Autyzmu";
        var htmlBody = GenerateContactResponseHtml(recipientName, facilityName, message);
        await SendEmailAsync(recipientEmail, subject, htmlBody);
    }

    public async Task SendReviewApprovedNotificationAsync(string recipientEmail, string facilityName)
    {
        var subject = "Twoja opinia została zatwierdzona - Oaza dla Autyzmu";
        var htmlBody = GenerateReviewApprovedHtml(facilityName);
        await SendEmailAsync(recipientEmail, subject, htmlBody);
    }

    private string GenerateContactResponseHtml(string recipientName, string facilityName, string message)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #3b82f6; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ padding: 20px; background-color: #f9fafb; }}
        .message {{ background-color: white; padding: 15px; border-left: 4px solid #3b82f6; margin: 15px 0; border-radius: 4px; }}
        .footer {{ text-align: center; padding: 20px; color: #6b7280; font-size: 12px; }}
        .button {{ display: inline-block; background-color: #3b82f6; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🧩 Oaza dla Autyzmu</h1>
        </div>
        <div class='content'>
            <p>Witaj <strong>{recipientName}</strong>,</p>
            <p>Otrzymałeś/aś odpowiedź z placówki <strong>{facilityName}</strong>:</p>
            <div class='message'>
                {message.Replace("\n", "<br>")}
            </div>
            <p>Jeśli chcesz kontynuować rozmowę, możesz odpowiedzieć bezpośrednio na ten email.</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} Oaza dla Autyzmu - Platforma wsparcia dla osób z autyzmem</p>
            <p style='font-size: 11px; color: #9ca3af;'>Ten email został wysłany automatycznie. Prosimy nie odpowiadać.</p>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateReviewApprovedHtml(string facilityName)
    {
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:5050";
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #10b981; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ padding: 20px; background-color: #f9fafb; }}
        .success {{ background-color: #d1fae5; padding: 15px; border-radius: 8px; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #6b7280; font-size: 12px; }}
        .button {{ display: inline-block; background-color: #3b82f6; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Opinia zatwierdzona!</h1>
        </div>
        <div class='content'>
            <div class='success'>
                <p><strong>Gratulacje!</strong></p>
                <p>Twoja opinia placówki <strong>{facilityName}</strong> została zaakceptowana przez moderatora i jest teraz widoczna publicznie.</p>
            </div>
            <p>Dziękujemy za wkład w rozwój społeczności Oazy dla Autyzmu. Twoja opinia pomoże innym rodzinom w podjęciu najlepszych decyzji.</p>
            <p style='text-align: center;'>
                <a href='{baseUrl}/Facilities' class='button'>Zobacz swoją opinię</a>
            </p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} Oaza dla Autyzmu - Platforma wsparcia dla osób z autyzmem</p>
            <p style='font-size: 11px; color: #9ca3af;'>Ten email został wysłany automatycznie. Prosimy nie odpowiadać.</p>
        </div>
    </div>
</body>
</html>";
    }
}
