using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OazaDlaAutyzmu.Domain.Entities;

namespace OazaDlaAutyzmu.Infrastructure.Data;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Ensure database is created (use EnsureCreated for InMemory, Migrate for relational)
        var databaseProvider = context.Database.ProviderName;
        if (databaseProvider == "Microsoft.EntityFrameworkCore.InMemory")
        {
            await context.Database.EnsureCreatedAsync();
        }
        else
        {
            await context.Database.MigrateAsync();
        }

        // Check if data already exists
        if (await context.Users.AnyAsync())
            return; // Database already seeded

        // 1. Create test user
        var testUser = new ApplicationUser
        {
            UserName = "test@oaza.pl",
            Email = "test@oaza.pl",
            FirstName = "Jan",
            LastName = "Kowalski",
            EmailConfirmed = true,
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(testUser, "Test123!");
        if (!result.Succeeded)
            throw new Exception($"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        // 2. Create admin user
        var adminUser = new ApplicationUser
        {
            UserName = "admin@oaza.pl",
            Email = "admin@oaza.pl",
            FirstName = "Admin",
            LastName = "System",
            EmailConfirmed = true,
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        await userManager.CreateAsync(adminUser, "Admin123!");

        // 3. Create test facility
        var facility = new Facility
        {
            Name = "Centrum Terapii Integracji Sensorycznej",
            Type = FacilityType.Therapy,
            Address = "ul. Kwiatowa 15",
            PostalCode = "00-001",
            City = "Warszawa",
            PhoneNumber = "+48 22 123 45 67",
            Email = "kontakt@ctis.pl",
            Website = "https://ctis.pl",
            Description = "Specjalizujemy się w terapii integracji sensorycznej dla dzieci z autyzmem. Oferujemy indywidualne sesje terapeutyczne prowadzone przez certyfikowanych terapeutów.",
            Latitude = 52.2297m,
            Longitude = 21.0122m,
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        context.Facilities.Add(facility);
        await context.SaveChangesAsync();

        // 4. Create test review
        var review = new Review
        {
            FacilityId = facility.Id,
            UserId = testUser.Id,
            Rating = 5,
            Comment = "Świetna placówka! Mój syn uczęszcza tu od 6 miesięcy i widzimy ogromne postępy. Terapeuci są bardzo kompetentni i cierpliwy. Atmosfera jest bardzo przyjazna dla dzieci. Gorąco polecam!",
            IsApproved = true,
            ApprovedById = adminUser.Id,
            ApprovedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        context.Reviews.Add(review);

        // 5. Add another review
        var review2 = new Review
        {
            FacilityId = facility.Id,
            UserId = adminUser.Id,
            Rating = 4,
            Comment = "Profesjonalna obsługa i nowoczesne wyposażenie. Jedyny minus to długi czas oczekiwania na wizytę.",
            IsApproved = true,
            ApprovedById = adminUser.Id,
            ApprovedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };

        context.Reviews.Add(review2);
        await context.SaveChangesAsync();

        // 6. Create forum topic
        var forumCategory = await context.ForumCategories.FirstAsync(c => c.Slug == "ogolne");
        
        var topic = new ForumTopic
        {
            CategoryId = forumCategory.Id,
            AuthorId = testUser.Id,
            Title = "Witam wszystkich! Jestem nowa na forum",
            Slug = "witam-wszystkich-jestem-nowa-na-forum",
            IsPinned = false,
            IsLocked = false,
            ViewCount = 0,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        context.ForumTopics.Add(topic);
        await context.SaveChangesAsync();

        // 7. Create first post in topic
        var post = new ForumPost
        {
            TopicId = topic.Id,
            AuthorId = testUser.Id,
            Content = @"Witam serdecznie! 

Jestem mamą 5-letniego Jasia, u którego niedawno zdiagnozowano autyzm. Szukam informacji o terapiach i wsparcia od innych rodziców.

Czy ktoś może polecić dobre miejsca w Warszawie, gdzie można rozpocząć terapię? Z góry dziękuję za każdą pomoc!",
            IsApproved = true,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        context.ForumPosts.Add(post);

        // 8. Create response from admin
        var response = new ForumPost
        {
            TopicId = topic.Id,
            AuthorId = adminUser.Id,
            Content = @"Witaj na forum! 

Cieszę się, że do nas dołączyłaś. To świetne miejsce do wymiany doświadczeń i wzajemnego wsparcia.

Polecam sprawdzić naszą sekcję z placówkami - znajdziesz tam listę zweryfikowanych ośrodków terapeutycznych w Warszawie. Wiele rodziców poleca Centrum Terapii Integracji Sensorycznej na Kwiatowej.

Powodzenia i zapraszam do aktywnego uczestnictwa w dyskusjach! 😊",
            IsApproved = true,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        context.ForumPosts.Add(response);

        // Update topic stats
        topic.PostCount = 2;
        topic.LastPostAt = response.CreatedAt;
        topic.LastPostUserId = adminUser.Id;

        await context.SaveChangesAsync();

        // 9. Create second facility
        var facility2 = new Facility
        {
            Name = "Przedszkole Integracyjne \"Tęczowe Marzenia\"",
            Type = FacilityType.School,
            Address = "ul. Słoneczna 8",
            PostalCode = "02-555",
            City = "Warszawa",
            PhoneNumber = "+48 22 987 65 43",
            Email = "kontakt@teczowe-marzenia.pl",
            Website = "https://teczowe-marzenia.pl",
            Description = "Przedszkole integracyjne z grupami terapeutycznymi dla dzieci ze spektrum autyzmu. Indywidualne podejście do każdego dziecka.",
            Latitude = 52.2150m,
            Longitude = 21.0450m,
            VerificationStatus = VerificationStatus.Unverified,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        context.Facilities.Add(facility2);

        // 10. Create another topic
        var therapyCategory = await context.ForumCategories.FirstAsync(c => c.Slug == "terapie");
        
        var topic2 = new ForumTopic
        {
            CategoryId = therapyCategory.Id,
            AuthorId = adminUser.Id,
            Title = "Terapia ABA - Wasze doświadczenia?",
            Slug = "terapia-aba-wasze-doswiadczenia",
            IsPinned = true,
            IsLocked = false,
            ViewCount = 15,
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        };

        context.ForumTopics.Add(topic2);
        await context.SaveChangesAsync();

        var post2 = new ForumPost
        {
            TopicId = topic2.Id,
            AuthorId = adminUser.Id,
            Content = @"Witam!

Chciałbym otworzyć temat na temat terapii ABA (Applied Behavior Analysis). 

Co o niej sądzicie? Jakie macie doświadczenia? Czy widzicie efekty?

Podzielcie się swoimi opiniami! 🙂",
            IsApproved = true,
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        };

        context.ForumPosts.Add(post2);

        topic2.PostCount = 1;
        topic2.LastPostAt = post2.CreatedAt;
        topic2.LastPostUserId = adminUser.Id;

        await context.SaveChangesAsync();

        Console.WriteLine("✅ Seed data created successfully!");
        Console.WriteLine($"   - Test user: test@oaza.pl / Test123!");
        Console.WriteLine($"   - Admin user: admin@oaza.pl / Admin123!");
        Console.WriteLine($"   - Facilities: 2");
        Console.WriteLine($"   - Reviews: 2");
        Console.WriteLine($"   - Forum topics: 2");
        Console.WriteLine($"   - Forum posts: 3");
    }
}
