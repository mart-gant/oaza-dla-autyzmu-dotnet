using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OazaDlaAutyzmu.Domain.Entities;
using OazaDlaAutyzmu.Web.Services;
using System.Security.Claims;

namespace OazaDlaAutyzmu.Web.Controllers.Api;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class IdentityApiController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions;
    private readonly IAuditService _auditService;

    public IdentityApiController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
        IAuditService auditService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _bearerTokenOptions = bearerTokenOptions;
        _auditService = auditService;
    }

    /// <summary>
    /// Rejestracja nowego użytkownika dla aplikacji mobilnej
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email i hasło są wymagane." });
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Użytkownik o podanym adresie email już istnieje." });
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            await _auditService.LogAsync("User_Register_Api", "ApplicationUser", user.Id, user.Id, user.Email, 
                null, $"New mobile API user registered: {user.Email}", HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Rejestracja zakończona pomyślnie. Możesz się teraz zalogować." });
        }

        var errors = result.Errors.Select(e => e.Description).ToList();
        return BadRequest(new { message = "Błąd rejestracji", errors });
    }

    /// <summary>
    /// Logowanie użytkownika i generowanie tokenu Bearer
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email i hasło są wymagane." });
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Nieprawidłowy email lub hasło." });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            await _auditService.LogAsync("User_Login_Api", "ApplicationUser", user.Id, user.Id, user.Email, 
                null, "Successful mobile API login", HttpContext.Connection.RemoteIpAddress?.ToString());

            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            
            // Logowanie za pomocą schematu Bearer
            await HttpContext.SignInAsync(IdentityConstants.BearerScheme, principal);
            
            // W tym momencie HttpContext.SignInAsync dla schematu Bearer automatycznie
            // generuje tokeny (access token + refresh token) i zapisuje je w formacie JSON
            // do strumienia odpowiedzi, zwracając status 200 OK.
            return new EmptyResult();
        }

        if (result.IsLockedOut)
        {
            await _auditService.LogAsync("User_Login_Api_LockedOut", "ApplicationUser", null, null, request.Email, 
                null, "API Account locked out", HttpContext.Connection.RemoteIpAddress?.ToString());
            
            return StatusCode(StatusCodes.Status423Locked, new { message = "Konto zostało zablokowane z powodu zbyt wielu nieudanych prób logowania. Spróbuj ponownie później." });
        }

        return Unauthorized(new { message = "Nieprawidłowy email lub hasło." });
    }

    /// <summary>
    /// Odświeżanie tokenu dostępu za pomocą tokenu odświeżania
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { message = "Token odświeżania jest wymagany." });
        }

        var options = _bearerTokenOptions.Get(IdentityConstants.BearerScheme);
        var ticket = options.RefreshTokenProtector.Unprotect(request.RefreshToken);
        
        if (ticket == null || ticket.Properties?.ExpiresUtc == null || ticket.Properties.ExpiresUtc < DateTimeOffset.UtcNow)
        {
            return Unauthorized(new { message = "Niepoprawny lub wygasły token odświeżania." });
        }

        var principal = ticket.Principal;
        var user = await _userManager.GetUserAsync(principal);
        if (user == null)
        {
            return Unauthorized(new { message = "Użytkownik powiązany z tokenem nie istnieje." });
        }

        var newPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
        await HttpContext.SignInAsync(IdentityConstants.BearerScheme, newPrincipal);
        
        return new EmptyResult();
    }

    /// <summary>
    /// Pobranie informacji o zalogowanym użytkowniku
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Brak autoryzacji" });
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return NotFound(new { message = "Użytkownik nie istnieje" });
        }

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            role = user.Role.ToString(),
            createdAt = user.CreatedAt
        });
    }

    /// <summary>
    /// Inicjalizacja logowania zewnętrznego dla aplikacji mobilnej (WebAuthenticator)
    /// </summary>
    [HttpGet("external-login")]
    public IActionResult ExternalLogin([FromQuery] string provider, [FromQuery] string redirectUri)
    {
        if (string.IsNullOrEmpty(provider) || string.IsNullOrEmpty(redirectUri))
        {
            return BadRequest(new { message = "Provider i redirectUri są wymagane." });
        }

        var callbackUrl = Url.Action("ExternalCallback", "IdentityApi");
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, callbackUrl);
        properties.Items["mobileRedirectUri"] = redirectUri;
        
        return Challenge(properties, provider);
    }

    /// <summary>
    /// Callback logowania zewnętrznego dla aplikacji mobilnej
    /// </summary>
    [HttpGet("external-callback")]
    public async Task<IActionResult> ExternalCallback()
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return BadRequest(new { message = "Nie można pobrać informacji o logowaniu zewnętrznym." });
        }

        var authenticateResult = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
        if (!authenticateResult.Succeeded || authenticateResult.Properties == null || 
            !authenticateResult.Properties.Items.TryGetValue("mobileRedirectUri", out var mobileRedirectUri) || 
            string.IsNullOrEmpty(mobileRedirectUri))
        {
            return BadRequest(new { message = "Brak adresu przekierowania dla aplikacji mobilnej." });
        }

        var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        if (user == null)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Nie otrzymano adresu email od dostawcy zewnętrznego." });
            }

            user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);
                var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? name ?? "Użytkownik";
                var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "";

                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Role = UserRole.User,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return BadRequest(new { message = "Nie udało się utworzyć użytkownika.", errors = createResult.Errors.Select(e => e.Description) });
                }
            }

            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                return BadRequest(new { message = "Nie udało się przypisać logowania zewnętrznego." });
            }
        }

        await _auditService.LogAsync("User_ExternalLogin_Api_Success", "ApplicationUser", user.Id, user.Id, user.Email, 
            null, $"Successful mobile API external login via {info.LoginProvider}", HttpContext.Connection.RemoteIpAddress?.ToString());

        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        var options = _bearerTokenOptions.Get(IdentityConstants.BearerScheme);
        var utcNow = DateTimeOffset.UtcNow;
        
        var accessTokenProperties = new AuthenticationProperties
        {
            ExpiresUtc = utcNow.Add(options.BearerTokenExpiration)
        };
        var accessTokenTicket = new AuthenticationTicket(principal, accessTokenProperties, IdentityConstants.BearerScheme);
        var accessToken = options.BearerTokenProtector.Protect(accessTokenTicket);
        
        var refreshTokenProperties = new AuthenticationProperties
        {
            ExpiresUtc = utcNow.Add(options.RefreshTokenExpiration)
        };
        var refreshTokenTicket = new AuthenticationTicket(principal, refreshTokenProperties, IdentityConstants.BearerScheme);
        var refreshToken = options.RefreshTokenProtector.Protect(refreshTokenTicket);

        var redirectUrlWithTokens = $"{mobileRedirectUri}?accessToken={Uri.EscapeDataString(accessToken)}&refreshToken={Uri.EscapeDataString(refreshToken)}";
        return Redirect(redirectUrlWithTokens);
    }
}

public class RegisterApiRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class LoginApiRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshApiRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
