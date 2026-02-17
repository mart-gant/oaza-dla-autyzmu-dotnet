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
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

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

        // Create roles if they don't exist
        var roles = new[] { "Admin", "Moderator", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
            }
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
        
        await userManager.AddToRoleAsync(testUser, "User");

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

        var adminResult = await userManager.CreateAsync(adminUser, "Admin123!");
        if (!adminResult.Succeeded)
            throw new Exception($"Failed to create admin user: {string.Join(", ", adminResult.Errors.Select(e => e.Description))}");
        
        await userManager.AddToRoleAsync(adminUser, "Admin");

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

        // 11. Get article categories
        var educationCategory = await context.ArticleCategories.FirstAsync(c => c.Slug == "edukacja");
        var therapyCategory2 = await context.ArticleCategories.FirstAsync(c => c.Slug == "terapie");
        var supportCategory = await context.ArticleCategories.FirstAsync(c => c.Slug == "wsparcie-rodzin");

        // 12. Create example articles about autism spectrum
        var article1 = new Article
        {
            Title = "Spektrum autyzmu - co to właściwie oznacza?",
            Slug = "spektrum-autyzmu-co-to-oznacza",
            Content = @"<h2>Czym jest spektrum autyzmu?</h2>

<p>Spektrum autyzmu to pojęcie, które opisuje zakres zachowań i umiejętności związanych z autyzmem. Słowo ""spektrum"" jest kluczowe - oznacza to, że autyzm przybywa w różnych formach i stopniach natężenia.</p>

<h3>Główne cechy spektrum autyzmu:</h3>

<ul>
<li><strong>Trudności w komunikacji społecznej</strong> - Issues with verbal and non-verbal communication</li>
<li><strong>Specjalne zainteresowania</strong> - Intensywne, często bardzo szczegółowe zainteresowania</li>
<li><strong>Powtarzalne zachowania</strong> - Rutyny i powtarzające się czynności</li>
<li><strong>Wrażliwość sensoryczna</strong> - Wzmożona lub osłabiona wrażliwość zmysłów</li>
</ul>

<h3>Ważne do zrozumienia:</h3>

<p>Każda osoba z autyzmem jest inna. Spektrum oznacza, że dwie osoby z diagnozą autyzmu mogą mieć zupełnie różne doświadczenia i potrzeby wsparcia.</p>

<p>Autyzm to nie choroba - to neurobiologiczna różnica w sposobie, w jaki mózg przetwaria informacje.</p>",
            Excerpt = "Poznaj podstawowe informacje o spektrum autyzmu, główne cechy i dlaczego jest to ważne pojęcie.",
            CategoryId = educationCategory.Id,
            AuthorId = adminUser.Id,
            Status = ArticleStatus.Published,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var article2 = new Article
        {
            Title = "Jak wspierać dziecko ze spektrum autyzmu w domu?",
            Slug = "wspieranie-dziecka-spektrum-autyzmu-w-domu",
            Content = @"<h2>Poradnik dla rodziców</h2>

<p>Wspieranie dziecka ze spektrum autyzmu w domu to ważna część jego rozwoju. Oto kilka praktycznych porad.</p>

<h3>1. Twórz rutyny i strukturę</h3>

<p>Dzieci ze spektrum autyzmu często czują się bezpieczniej w przewidywalnym otoczeniu. Regularny harmonogram dnia pomaga zmniejszyć niepokój.</p>

<h3>2. Dostosuj otoczenie sensoryczne</h3>

<ul>
<li>Ogranicz hałas i zbędne bodźce</li>
<li>Wybierz odpowiednie oświetlenie</li>
<li>Zapewni spokojny kąt do odpoczynku</li>
</ul>

<h3>3. Nawiąż komunikację </h3>

<p>Bądź jasny i konkretny w instrukcjach. Użyj prostych słów i wizualnych pomocí komunikacyjnych.</p>

<h3>4. Doceniaj zainteresowania dziecka</h3>

<p>Zamiast walczyć ze specjalistycznym zainteresowaniem twojego dziecka, spróbuj je wykorzystać w nauce i zabawie.</p>

<h3>5. Szukaj profesjonalnego wsparcia</h3>

<p>Terapeuci, psycholodzy i specjaliści mogą zaproponować strategie dostosowane do indywidualnych potrzeb twojego dziecka.</p>",
            Excerpt = "Praktyczne porady dla rodziców na temat wspierania dziecka ze spektrum autyzmu w domu.",
            CategoryId = supportCategory.Id,
            AuthorId = adminUser.Id,
            Status = ArticleStatus.Published,
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        };

        var article3 = new Article
        {
            Title = "Terapia logopedyczna dla osób z autyzmem",
            Slug = "terapia-logopedyczna-dla-osob-z-autyzmem",
            Content = @"<h2>Znaczenie terapii logopedycznej</h2>

<p>Wiele osób ze spektrum autyzmu ma trudności z komunikacją. Terapia logopedyczna może być bardzo pomocna.</p>

<h3>Jakie problemy logopedyczne są częste?</h3>

<ul>
<li>Opóźnienie w rozwoju mowy</li>
<li>Trudności z artykułacją</li>
<li>Problemy z zrozumieniem i użyciem języka</li>
<li>Problemy z pragmatyką komunikacyjną (społeczne aspekty komunikacji)</li>
</ul>

<h3>Jak logopeda może pomóc?</h3>

<p>Logopedzi opracowują indywidualne plany terapii dostosowane do potrzeb każdej osoby. Mogą pracować nad:</p>

<ul>
<li>Wyraźnością mowy</li>
<li>Rozbudowaniem słownika</li>
<li>Umiejętnościami społeczno-komunikacyjnymi</li>
<li>Zapoznawaniem się z alternatywnymi metodami komunikacji (AAC)</li>
</ul>

<h3>Kiedy szukać pomocy?</h3>

<p>Jeśli dostrzeżesz trudności w komunikacji u swojego dziecka, warto zasięgnąć porady logopedy. Im wcześniej zostanie podjęta interwencja, tym lepsze mogą być rezultaty.</p>",
            Excerpt = "Informacja o roli i znaczeniu terapii logopedycznej dla osób ze spektrum autyzmu.",
            CategoryId = therapyCategory2.Id,
            AuthorId = adminUser.Id,
            Status = ArticleStatus.Published,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var article4 = new Article
        {
            Title = "Integracja sensoryczna - co to i jak działa?",
            Slug = "integracja-sensoryczna-jak-dziala",
            Content = @"<h2>Zrozumienie integracji sensorycznej</h2>

<p>Integracja sensoryczna (SI) to termin opisujący, jak nasze mózgi odbierają i przetwarzają informacje ze zmysłów.</p>

<h3>Problemy z integracją sensoryczną w autyzmie</h3>

<p>Osoby ze spektrum autyzmu często mają trudności z przetwarzaniem informacji sensorycznych. Mogą być:</p>

<ul>
<li><strong>Nadwrażliwe (hipersensytywne)</strong> - Zbyt czuli na bodźce</li>
<li><strong>Niedowrażliwe (hiposensytywne)</strong> - Mniej czuli na bodźce</li>
<li><strong>Niechętni wobec zmian sensorycznych</strong> - Wymagają czasu na adaptację</li>
</ul>

<h3>Terapia integracji sensorycznej</h3>

<p>Terapeuta terapii SI pracuje z pacjentem, aby pomóc mózgowi lepiej przetwarzać bodźce sensoryczne. Sesje mogą obejmować:</p>

<ul>
<li>Zabawy i ćwiczenia w kontrolowanym otoczeniu</li>
<li>Bodźce proprioceptywne i wibrantne</li>
<li>Aktywności dostosowane do indywidualnych potrzeb</li>
</ul>

<h3>Korzyści</h3>

<p>Prawidłowa terapia SI może poprawiać równowagę, koordynację motoryczną i emocjonalne samopoczucie.</p>",
            Excerpt = "Wyjaśnienie procesu integracji sensorycznej i roli terapii SI dla osób ze spektrum autyzmu.",
            CategoryId = therapyCategory2.Id,
            AuthorId = adminUser.Id,
            Status = ArticleStatus.Published,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        context.Articles.AddRange(article1, article2, article3, article4);
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
