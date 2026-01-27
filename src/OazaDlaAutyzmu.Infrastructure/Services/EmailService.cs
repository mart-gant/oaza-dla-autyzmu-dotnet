using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace OazaDlaAutyzmu.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly SmtpClient _smtpClient;
    private readonly string _senderEmail;
    private readonly string _senderName;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "noreply@oaza.pl";
        _senderName = _configuration["EmailSettings:SenderName"] ?? "Oaza dla Autyzmu";

        var smtpServer = _configuration["EmailSettings:SmtpServer"];
        var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        var smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? _senderEmail;
        var smtpPassword = _configuration["EmailSettings:SmtpPassword"];

        _smtpClient = new SmtpClient(smtpServer)
        {
            Port = smtpPort,
            Credentials = new NetworkCredential(smtpUsername, smtpPassword),
            EnableSsl = true
        };
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        // Skip sending if SMTP not configured
        if (string.IsNullOrEmpty(_configuration["EmailSettings:SmtpServer"]))
        {
            Console.WriteLine($"[EMAIL SKIPPED] To: {to}, Subject: {subject}");
            return;
        }

        try
        {
            var message = new MailMessage
            {
                From = new MailAddress(_senderEmail, _senderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await _smtpClient.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - email failure shouldn't break the application
            Console.WriteLine($"[EMAIL ERROR] Failed to send to {to}: {ex.Message}");
        }
    }

    public async Task SendContactResponseAsync(string recipientEmail, string recipientName, string facilityName, string message)
    {
        var subject = $"Odpowiedź z {facilityName} - Oaza dla Autyzmu";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #3b82f6; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9fafb; }}
        .message {{ background-color: white; padding: 15px; border-left: 4px solid #3b82f6; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #6b7280; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🧩 Oaza dla Autyzmu</h1>
        </div>
        <div class='content'>
            <p>Witaj {recipientName},</p>
            <p>Otrzymałeś/aś odpowiedź z placówki <strong>{facilityName}</strong>:</p>
            <div class='message'>
                {message.Replace("\n", "<br>")}
            </div>
            <p>Jeśli chcesz kontynuować rozmowę, możesz odpowiedzieć bezpośrednio na ten email.</p>
        </div>
        <div class='footer'>
            <p>Oaza dla Autyzmu - Platforma wsparcia dla osób z autyzmem</p>
            <p><a href='http://localhost:5050'>Odwiedź naszą stronę</a></p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(recipientEmail, subject, htmlBody);
    }

    public async Task SendReviewApprovedNotificationAsync(string recipientEmail, string facilityName)
    {
        var subject = "Twoja opinia została zatwierdzona - Oaza dla Autyzmu";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #10b981; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9fafb; }}
        .success {{ background-color: #d1fae5; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .footer {{ text-align: center; padding: 20px; color: #6b7280; font-size: 12px; }}
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
            <p>Dziękujemy za wkład w rozwój społeczności Oazy dla Autyzmu.</p>
            <p><a href='http://localhost:5050/Facilities' style='display: inline-block; background-color: #3b82f6; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Zobacz swoją opinię</a></p>
        </div>
        <div class='footer'>
            <p>Oaza dla Autyzmu - Platforma wsparcia dla osób z autyzmem</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(recipientEmail, subject, htmlBody);
    }
}
