using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OazaDlaAutyzmu.Domain.Entities;
using OazaDlaAutyzmu.Web.Services;
using System.Security.Claims;

namespace OazaDlaAutyzmu.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailSender _emailSender;
    private readonly IAuditService _auditService;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailSender emailSender,
        IAuditService auditService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _auditService = auditService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string email, string password, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Email i hasło są wymagane.");
            return View();
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            // Audit log for new user registration
            await _auditService.LogAsync("User_Register", "ApplicationUser", user.Id, user.Id, user.Email, 
                null, $"New user: {user.Email}", HttpContext.Connection.RemoteIpAddress?.ToString());

            // Generate email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Account", 
                new { userId = user.Id, token }, Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email!, "Potwierdź swoje konto - Oaza dla Autyzmu", 
                $"Kliknij w link, aby potwierdzić swoje konto: <a href='{confirmationLink}'>Potwierdź email</a>");

            TempData["Message"] = "Rejestracja zakończona pomyślnie! Sprawdź swoją skrzynkę email, aby potwierdzić konto.";
            return RedirectToAction("Login");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View();
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError("", "Email i hasło są wymagane.");
            return View();
        }

        var result = await _signInManager.PasswordSignInAsync(email, password, isPersistent: true, lockoutOnFailure: true);

        if (result.RequiresTwoFactor)
        {
            return RedirectToAction("LoginWith2FA", new { returnUrl });
        }

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                await _auditService.LogAsync("User_Login", "ApplicationUser", user.Id, user.Id, user.Email, 
                    null, "Successful login", HttpContext.Connection.RemoteIpAddress?.ToString());
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            await _auditService.LogAsync("User_Login_LockedOut", "ApplicationUser", null, null, email, 
                null, "Account locked out", HttpContext.Connection.RemoteIpAddress?.ToString());
            
            ModelState.AddModelError("", "Konto zostało zablokowane z powodu zbyt wielu nieudanych prób logowania. Spróbuj ponownie później.");
            return View();
        }

        ModelState.AddModelError("", "Nieprawidłowy email lub hasło.");
        return View();
    }

    [HttpGet]
    public IActionResult LoginWith2FA(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginWith2FA(string code, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            ModelState.AddModelError("", "Kod weryfikacyjny jest wymagany.");
            return View();
        }

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(code, isPersistent: true, rememberClient: false);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError("", "Konto zostało zablokowane.");
            return View();
        }

        ModelState.AddModelError("", "Nieprawidłowy kod weryfikacyjny.");
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            await _auditService.LogAsync("User_Logout", "ApplicationUser", user.Id, user.Id, user.Email, 
                null, "User logged out", HttpContext.Connection.RemoteIpAddress?.ToString());
        }

        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(int userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            ViewBag.ErrorMessage = "Nie znaleziono użytkownika.";
            return View("Error");
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            ViewBag.SuccessMessage = "Email został potwierdzony pomyślnie! Możesz się teraz zalogować.";
            return View();
        }

        ViewBag.ErrorMessage = "Nie udało się potwierdzić emaila.";
        return View();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Enable2FA()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        await _userManager.ResetAuthenticatorKeyAsync(user);
        var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);

        ViewBag.AuthenticatorKey = FormatKey(authenticatorKey!);
        ViewBag.QRCodeUrl = GenerateQrCodeUri(user.Email!, authenticatorKey!);

        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enable2FA(string code)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);

        if (!is2faTokenValid)
        {
            ModelState.AddModelError("", "Nieprawidłowy kod weryfikacyjny.");
            
            var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
            ViewBag.AuthenticatorKey = FormatKey(authenticatorKey!);
            ViewBag.QRCodeUrl = GenerateQrCodeUri(user.Email!, authenticatorKey!);
            
            return View();
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        TempData["Message"] = "Uwierzytelnianie dwuskładnikowe zostało włączone pomyślnie!";
        
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable2FA()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _userManager.ResetAuthenticatorKeyAsync(user);
        
        TempData["Message"] = "Uwierzytelnianie dwuskładnikowe zostało wyłączone.";
        return RedirectToAction("Index", "Home");
    }

    private string FormatKey(string key)
    {
        const int chunkSize = 4;
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < key.Length; i += chunkSize)
        {
            if (i > 0) result.Append(' ');
            result.Append(key.Substring(i, Math.Min(chunkSize, key.Length - i)));
        }
        return result.ToString();
    }

    private string GenerateQrCodeUri(string email, string key)
    {
        return $"otpauth://totp/Oaza dla Autyzmu:{email}?secret={key}&issuer=Oaza dla Autyzmu";
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError("", "Email jest wymagany.");
            return View();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            TempData["Message"] = "Jeśli podany email istnieje w systemie, wysłaliśmy link do resetowania hasła.";
            return RedirectToAction("Login");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = Url.Action("ResetPassword", "Account", 
            new { userId = user.Id, token }, Request.Scheme);

        await _emailSender.SendEmailAsync(user.Email!, "Resetowanie hasła - Oaza dla Autyzmu", 
            $"Kliknij w link, aby zresetować hasło: <a href='{resetLink}'>Resetuj hasło</a><br><br>Link wygasa za 1 godzinę.");

        await _auditService.LogAsync("User_PasswordResetRequested", "ApplicationUser", user.Id, user.Id, user.Email, 
            null, "Password reset requested", HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Message"] = "Jeśli podany email istnieje w systemie, wysłaliśmy link do resetowania hasła.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult ResetPassword(int userId, string token)
    {
        ViewBag.UserId = userId;
        ViewBag.Token = token;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int userId, string token, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || password != confirmPassword)
        {
            ModelState.AddModelError("", "Hasła muszą być identyczne i nie mogą być puste.");
            ViewBag.UserId = userId;
            ViewBag.Token = token;
            return View();
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            ViewBag.ErrorMessage = "Nie znaleziono użytkownika.";
            return View("Error");
        }

        var result = await _userManager.ResetPasswordAsync(user, token, password);
        if (result.Succeeded)
        {
            await _auditService.LogAsync("User_PasswordReset", "ApplicationUser", user.Id, user.Id, user.Email, 
                null, "Password reset successfully", HttpContext.Connection.RemoteIpAddress?.ToString());

            TempData["Message"] = "Hasło zostało zmienione pomyślnie! Możesz się teraz zalogować.";
            return RedirectToAction("Login");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        ViewBag.UserId = userId;
        ViewBag.Token = token;
        return View();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> DownloadMyData()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var userData = new
        {
            PersonalInformation = new
            {
                user.Email,
                user.FirstName,
                user.LastName,
                user.CreatedAt,
                user.PhoneNumber
            },
            AccountSettings = new
            {
                TwoFactorEnabled = user.TwoFactorEnabled,
                EmailConfirmed = user.EmailConfirmed
            },
            ExportDate = DateTime.UtcNow
        };

        var json = System.Text.Json.JsonSerializer.Serialize(userData, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        await _auditService.LogAsync("User_DataExport", "ApplicationUser", user.Id, user.Id, user.Email, 
            null, "User data exported (GDPR)", HttpContext.Connection.RemoteIpAddress?.ToString());

        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"my-data-{DateTime.UtcNow:yyyy-MM-dd}.json");
    }

    [HttpGet]
    [Authorize]
    public IActionResult DeleteMyAccount()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMyAccountConfirmed()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        await _auditService.LogAsync("User_AccountDeleted", "ApplicationUser", user.Id, user.Id, user.Email, 
            $"User: {user.Email}", "Account permanently deleted (GDPR)", HttpContext.Connection.RemoteIpAddress?.ToString());

        await _signInManager.SignOutAsync();
        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            TempData["Message"] = "Twoje konto zostało trwale usunięte.";
            return RedirectToAction("Index", "Home");
        }

        TempData["ErrorMessage"] = "Nie udało się usunąć konta.";
        return RedirectToAction("DeleteMyAccount");
    }
}
