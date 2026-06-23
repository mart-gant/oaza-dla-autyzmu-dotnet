using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OazaDlaAutyzmu.Application.Commands.Facilities;
using OazaDlaAutyzmu.Domain.Entities;
using OazaDlaAutyzmu.Infrastructure.Data;
using OazaDlaAutyzmu.Infrastructure.Services;
using OazaDlaAutyzmu.Web.Services;
using OazaDlaAutyzmu.Web.Middleware;
using AspNetCoreRateLimit;
using Serilog;
using reCAPTCHA.AspNetCore;
using Sentry;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog early
// Read Seq and log file path from configuration
var seqServerUrl = builder.Configuration.GetValue<string>("Serilog:SeqServerUrl");
var logFilePath = builder.Configuration.GetValue<string>("Serilog:LogFilePath") ?? "Logs/log-.txt";

Log.Logger = new Serilog.LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(logFilePath, rollingInterval: Serilog.RollingInterval.Day)
    .WriteTo.Seq(seqServerUrl ?? "http://localhost:5341")
    .CreateLogger();

builder.Host.UseSerilog();

builder.WebHost.UseSentry(options =>
{
    options.Dsn = builder.Configuration["Sentry:Dsn"];
    options.Environment = builder.Environment.EnvironmentName;
    options.TracesSampleRate = builder.Configuration.GetValue<double>("Sentry:TracesSampleRate", 0.1);
    options.SendDefaultPii = builder.Configuration.GetValue<bool>("Sentry:SendDefaultPii", false);
    options.MaxBreadcrumbs = builder.Configuration.GetValue<int>("Sentry:MaxBreadcrumbs", 50);
});

// Add services to the container
var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");
if (useInMemory)
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseInMemoryDatabase("TestDb"));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Memory cache for rate limiting
builder.Services.AddMemoryCache();

// Rate limiting configuration
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Identity configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    // Password requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    
    // Email confirmation
    options.SignIn.RequireConfirmedEmail = false; // Set to true in production
    
    // Account lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cookie configuration for session timeout
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// MediatR configuration
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(CreateFacilityCommand).Assembly));

// FluentValidation configuration
builder.Services.AddValidatorsFromAssemblyContaining<CreateFacilityCommand>();

// reCAPTCHA configuration
builder.Services.AddRecaptcha(builder.Configuration.GetSection("RecaptchaSettings"));

// Email service configuration - use Resend for modern email delivery
var emailProvider = builder.Configuration["EmailSettings:Provider"]?.ToLower() ?? "smtp";
if (emailProvider == "resend")
{
    // Register Resend client
    var resendApiKey = builder.Configuration["EmailSettings:ResendApiKey"];
    builder.Services.AddScoped<Resend.IResend>(_ => new Resend.ResendClient(resendApiKey));
    builder.Services.AddScoped<IEmailService, ResendEmailService>();
}
else
{
    // Fallback to SMTP-based email service
    builder.Services.AddScoped<IEmailService, EmailService>();
}

// Register services
builder.Services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IContentModerationService, ContentModerationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Controllers and views
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Response caching
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();

// Response compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// Swagger/OpenAPI configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed database with test data
using (var scope = app.Services.CreateScope())
{
    try
    {
        await OazaDlaAutyzmu.Infrastructure.Data.SeedData.Initialize(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Ensure specific article slug is published to avoid 404 for known slugs in development
using (var scope = app.Services.CreateScope())
{
    try
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var context = scope.ServiceProvider.GetRequiredService<OazaDlaAutyzmu.Infrastructure.Data.ApplicationDbContext>();
        var targetSlug = "swiatowy-dzien-swiadomosci-autyzmu-2";
        var article = await context.Articles.FirstOrDefaultAsync(a => a.Slug == targetSlug);
        if (article != null && article.Status != OazaDlaAutyzmu.Domain.Entities.ArticleStatus.Published)
        {
            article.Status = OazaDlaAutyzmu.Domain.Entities.ArticleStatus.Published;
            article.PublishedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            logger.LogInformation("Published article with slug '{Slug}' on startup.", targetSlug);
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while ensuring article publication.");
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Even in development, use HTTPS redirection
    app.UseDeveloperExceptionPage();
    
    // Enable Swagger only in development
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // Use an app-root relative path so the UI works consistently behind the /api/docs prefix.
        c.SwaggerEndpoint("../swagger/v1/swagger.json", "Oaza dla Autyzmu API v1");
        c.RoutePrefix = "api/docs"; // URL will be /api/docs
    });
}

// HSTS configuration (HTTP Strict Transport Security)
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
    await next();
});

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseResponseCaching();

// Security headers
app.UseSecurityHeaders();

app.UseRouting();

// Rate limiting middleware
app.UseIpRateLimiting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/docs", () => Results.Redirect("/api/docs/index.html"));
app.MapGet("/api/docs/", () => Results.Redirect("/api/docs/index.html"));

app.MapStaticAssets();

// Map attribute-routed API controllers (e.g. /api/v1/...)
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

try
{
    app.Run();
}
finally
{
    Serilog.Log.CloseAndFlush();
}

// Make Program class accessible to tests
public partial class Program { }
