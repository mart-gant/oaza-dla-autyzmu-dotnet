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
using reCAPTCHA.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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

// Register services
builder.Services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IEmailService, EmailService>();
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Oaza dla Autyzmu API v1");
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

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();

// Make Program class accessible to tests
public partial class Program { }
