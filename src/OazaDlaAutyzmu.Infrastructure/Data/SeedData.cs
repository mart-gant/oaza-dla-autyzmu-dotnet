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

        // 3. Create recommended therapists
        var f1 = new Facility
        {
            Name = "Katarzyna Andrys",
            Type = FacilityType.Therapy,
            Address = "Dialogownia",
            City = "Poznań",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: systemowe, SWR.",
            Website = "https://dialogownia.com/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f1);

        var f2 = new Facility
        {
            Name = "Joanna Barbarewicz",
            Type = FacilityType.Therapy,
            Address = "Gdynia, Warszawa",
            City = "Gdynia, Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: integratywne.",
            Website = "https://przystanekempatia.pl/portfolio-item/joanna-barbarewicz-psycholog-psychoterapeuta/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f2);

        var f3 = new Facility
        {
            Name = "Marta Baron",
            Type = FacilityType.Therapy,
            Address = "Ko-relacje",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: Gestalt.",
            Website = "http://www.ko-relacje.pl/zespol/marta-baron",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f3);

        var f4 = new Facility
        {
            Name = "Beata Błaszyk",
            Type = FacilityType.Therapy,
            Address = "Bydgoszcz",
            City = "Bydgoszcz",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: ericksonowskie.",
            Website = "https://psychoterapia-za.pl/index.php/o-mnie/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f4);

        var f5 = new Facility
        {
            Name = "Alicja Bogaczyk",
            Type = FacilityType.Therapy,
            Address = "Centrum Wspierania Relacji",
            City = "Poznań",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: TPP.",
            Website = "http://wspieranierelacji.pl/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f5);

        var f6 = new Facility
        {
            Name = "Kinga Bogdańska",
            Type = FacilityType.Therapy,
            Address = "Ginamedica",
            City = "Wrocław",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://ginemedica.pl/lekarz/kinga-bogdanska/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f6);

        var f7 = new Facility
        {
            Name = "Katarzyna Borowiec-Mańkowska",
            Type = FacilityType.Therapy,
            Address = "TIPI",
            City = "Bytom",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: systemowe.",
            Website = "https://www.znanylekarz.pl/katarzyna-borowiec-mankowska/psycholog-psychoterapeuta/zabrze",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f7);

        var f8 = new Facility
        {
            Name = "Aleksandra Brzezińska",
            Type = FacilityType.Therapy,
            Address = "Centrum Psychoterapii i Diagnozy",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://www.centrumpid.com/aleksandra-brzezinska",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f8);

        var f9 = new Facility
        {
            Name = "Joanna Burgiełł",
            Type = FacilityType.Therapy,
            Address = "Warszawa",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://joannaburgiell.wixsite.com/psychoterapia",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f9);

        var f10 = new Facility
        {
            Name = "Monika Całka",
            Type = FacilityType.Therapy,
            Address = "Integra",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: integratywne.",
            Website = "https://psychopracownia-integra.pl/zespol/#monikaa-calka",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f10);

        var f11 = new Facility
        {
            Name = "Maja Chaudhuri",
            Type = FacilityType.Therapy,
            Address = "Warszawa",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT, schematów.",
            Website = "https://www.znanylekarz.pl/maja-chaudhuri/psycholog-seksuolog-psychoterapeuta/warszawa?fbclid=IwAR3-VDtwgK1Z91q1OxkoJTmSEVd6TOZhW6xC4sdGqsSNgDZ-U_InZilmsNc",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f11);

        var f12 = new Facility
        {
            Name = "Paula Chibowska",
            Type = FacilityType.Therapy,
            Address = "Gdańsk",
            City = "Gdańsk",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://www.znanylekarz.pl/paula-chibowska/psychoterapeuta-psycholog/gdansk",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f12);

        var f13 = new Facility
        {
            Name = "Patrycja Cichanowska-Hołojda",
            Type = FacilityType.Therapy,
            Address = "Wałbrzych",
            City = "Wałbrzych",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: integratywne, systemowe.",
            Website = "https://pleso.me/pl/therapists?psychologist_id=6ad2f516-f039-3d9f-8e6c-e349f59cf734&booking-widget-step=service_and_calendar_selection",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f13);

        var f14 = new Facility
        {
            Name = "Maria Czarnecka",
            Type = FacilityType.Therapy,
            Address = "Bliskie miejsce",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: psychodynamiczne, systemowe.",
            Website = "http://bliskiemiejsce.pl/maria-czarnecka/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f14);

        var f15 = new Facility
        {
            Name = "Emilia Czartoryska",
            Type = FacilityType.Therapy,
            Address = "HarmonJa",
            City = "Gdańsk",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: DBT, Gestalt.",
            Website = "https://www.harmonja.pl/zespol/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f15);

        var f16 = new Facility
        {
            Name = "Monika Czerepak",
            Type = FacilityType.Therapy,
            Address = "MedTerapeutica",
            City = "Wrocław",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: psychodynamiczne, systemowe.",
            Website = "https://medterapeutica.pl/mc/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f16);

        var f17 = new Facility
        {
            Name = "Eliza Czerka-Fortuna",
            Type = FacilityType.Therapy,
            Address = "Gdańsk, Gdynia",
            City = "Gdańsk, Gdynia",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: systemowe, CBT.",
            Website = "https://www.znanylekarz.pl/eliza-czerka-fortuna/psycholog-psychoterapeuta/gdansk",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f17);

        var f18 = new Facility
        {
            Name = "Malik Czernek",
            Type = FacilityType.Therapy,
            Address = "Bielsko-Biała",
            City = "Bielsko-Biała",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "http://psychologbielsko.eu/kadra/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f18);

        var f19 = new Facility
        {
            Name = "Urszula Czyżowicz",
            Type = FacilityType.Therapy,
            Address = "Bydgoszcz",
            City = "Bydgoszcz",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT, schematów.",
            Website = "https://www.znanylekarz.pl/urszula-czyzowicz/psycholog-psychoterapeuta/bydgoszcz",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f19);

        var f20 = new Facility
        {
            Name = "Małgorzata Dębska",
            Type = FacilityType.Therapy,
            Address = "OdNowa",
            City = "Wrocław",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "http://www.psychologodnowa.pl/pl/o-nas/2-uncategorised/64-malgorzata-debska",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f20);

        var f21 = new Facility
        {
            Name = "Ewa Dobiała",
            Type = FacilityType.Therapy,
            Address = "Leszno",
            City = "Leszno",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: TPP, ericksonowskie.",
            Website = "http://edobiala.igabinet.pl/g1336221324pol-O-mnie.html",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f21);

        var f22 = new Facility
        {
            Name = "Dorota Dolecka-Semeniuk",
            Type = FacilityType.Therapy,
            Address = "Lublin",
            City = "Lublin",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: psychodynamiczne.",
            Website = "https://www.znanylekarz.pl/dorota-dolecka/psycholog-psychoterapeuta/lublin",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f22);

        var f23 = new Facility
        {
            Name = "Marta Dora",
            Type = FacilityType.Therapy,
            Address = "Warszawa",
            City = "Pracownia Zasoby, Centrum Terapii Synteza",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: systemowo-psychodynamiczne.",
            Website = "http://pracowniazasoby.pl/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f23);

        var f24 = new Facility
        {
            Name = "Joanna Drążkiewicz",
            Type = FacilityType.Therapy,
            Address = "Dialog",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: integratywne.",
            Website = "https://www.psychiatrzy.warszawa.pl/specjalisci/joanna-drazkiewicz",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f24);

        var f25 = new Facility
        {
            Name = "Katarzyna Dułak",
            Type = FacilityType.Therapy,
            Address = "Mental Path",
            City = "Gdańsk, Sopot",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: Gestalt.",
            Website = "http://www.katarzynadulak.pl/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f25);

        var f26 = new Facility
        {
            Name = "Karolina Dyrda",
            Type = FacilityType.Therapy,
            Address = "Psychomedic",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://psychomedic.pl/dt_portfolios/mgr-karolina-dyrda-psycholog-psychoterapeuta-dzieci-i-doroslych-cbt/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f26);

        var f27 = new Facility
        {
            Name = "Katarzyna Dzwonnik",
            Type = FacilityType.Therapy,
            Address = "Wrocław",
            City = "Wrocław",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: Gestalt, systemowe, par, rodzin.",
            Website = "https://www.psychologowie-dla-spoleczenstwa.pl/zespol/kasia-dzwonnik",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f27);

        var f28 = new Facility
        {
            Name = "Magdalena Flaga-Łuczkiewicz",
            Type = FacilityType.Therapy,
            Address = "Dialog",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: integratywne.",
            Website = "https://www.psychiatrzy.warszawa.pl/specjalisci/magdalena-flaga-luczkiewicz/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f28);

        var f29 = new Facility
        {
            Name = "Tomasz Folusz",
            Type = FacilityType.Therapy,
            Address = "Warszawa",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "http://psycho-terapeuta.warszawa.pl/poznawczo-behawioralny/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f29);

        var f30 = new Facility
        {
            Name = "Izabela Fornalik",
            Type = FacilityType.Therapy,
            Address = "Klinika Terapii Poznawczo-Behawioralnej Uniwersytetu SWPS",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://klinika1.swps.pl/index.php",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f30);

        var f31 = new Facility
        {
            Name = "Patrycja Frania",
            Type = FacilityType.Therapy,
            Address = "SISU",
            City = "Wrocław",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: TSR, TPP.",
            Website = "https://patrycjafrania.pl/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f31);

        var f32 = new Facility
        {
            Name = "Maria Gaj",
            Type = FacilityType.Therapy,
            Address = "Psychomedic",
            City = "Łódź",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://psychomedic.pl/dt_portfolios/mgr-maria-gaj-psycholog-psychoterapeuta-lodz/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f32);

        var f33 = new Facility
        {
            Name = "Zuzanna Gawor-Kotkowska",
            Type = FacilityType.Therapy,
            Address = "Neuroverse",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: różne.",
            Website = "https://www.neuroverse.com.pl/czlonkowie-zespolu/zuzanna",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f33);

        var f34 = new Facility
        {
            Name = "Joanna Gieldarska",
            Type = FacilityType.Therapy,
            Address = "Warszawa",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "http://joannagieldarska.pl/pl/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f34);

        var f35 = new Facility
        {
            Name = "Małgorzata Gołębiewska",
            Type = FacilityType.Therapy,
            Address = "Gdynia",
            City = "Poradnia CBT",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://www.znanylekarz.pl/malgorzata-golebiewska-2/psychoterapeuta/gdynia",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f35);

        var f36 = new Facility
        {
            Name = "Zuzanna Gucwa-Mozolewska",
            Type = FacilityType.Therapy,
            Address = "Vadimed",
            City = "Kraków",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT, psychodynamiczne.",
            Website = "https://www.vadimed.com.pl/dla-pacjenta/godziny-przyjec",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f36);

        var f37 = new Facility
        {
            Name = "Michał T. Handzel",
            Type = FacilityType.Therapy,
            Address = "Indywidualne Wsparcie Terapeutyczne",
            City = "Online",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: różne.",
            Website = "https://www.facebook.com/IWTspektrumautyzmu",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f37);

        var f38 = new Facility
        {
            Name = "Monika Hołub",
            Type = FacilityType.Therapy,
            Address = "Warszawa, Żyrardów",
            City = "Warszawa, Żyrardów",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: integratywne.",
            Website = "http://dobry-psycholog.com/o-mnie",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f38);

        var f39 = new Facility
        {
            Name = "Marek Husak",
            Type = FacilityType.Therapy,
            Address = "Cogito",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT, schematów.",
            Website = "https://savant.org.pl/o-nas/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f39);

        var f40 = new Facility
        {
            Name = "Katarzyna Kalinowska",
            Type = FacilityType.Therapy,
            Address = "Białystok",
            City = "Mindhealth",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: systemowe, psychodynamiczne.",
            Website = "https://mindhealth.pl/specjalisci/mgr-katarzyna-kalinowska",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f40);

        var f41 = new Facility
        {
            Name = "Patrycja Kloza",
            Type = FacilityType.Therapy,
            Address = "Katowice",
            City = "Katowice",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: integratywne.",
            Website = "https://www.pracowniapsychoedukacji.com.pl/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f41);

        var f42 = new Facility
        {
            Name = "Elwira Korycka",
            Type = FacilityType.Therapy,
            Address = "AdAlta",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: schematów, CBT.",
            Website = "https://www.adalta.com.pl/elwira-korycka",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f42);

        var f43 = new Facility
        {
            Name = "Kornelia Krajowska-Kukieł",
            Type = FacilityType.Therapy,
            Address = "Warszawa",
            City = "Synapsis, Centrum Diagnozy i Pomocy Psych.",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: psychodynamiczne, systemowe.",
            Website = "https://psychologursus.com.pl/o-nas/nasi-specjalisci/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f43);

        var f44 = new Facility
        {
            Name = "Anita Krakowiak",
            Type = FacilityType.Therapy,
            Address = "Online",
            City = "Online",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: systemowe, psychodynamiczne.",
            Website = "https://epsycholodzy.pl/blog/psychoterapia-online/psycholog/anita-krakowiak/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f44);

        var f45 = new Facility
        {
            Name = "Aleksandra Kraszczyńska",
            Type = FacilityType.Therapy,
            Address = "Online",
            City = "Online",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: EMDR, CBT, systemowe.",
            Website = "https://www.terapiatraumy.online/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f45);

        var f46 = new Facility
        {
            Name = "Magdalena Krukowska-Sidorczuk",
            Type = FacilityType.Therapy,
            Address = "Luxmed",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://harmonia.luxmed.pl/specjalista/krukowska-sidorczuk-magdalena/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f46);

        var f47 = new Facility
        {
            Name = "Magdalena Krzyżosiak",
            Type = FacilityType.Therapy,
            Address = "Preludium",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://poradniapreludium.pl/nasz-zespol/magdalena-krzyzosiak/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f47);

        var f48 = new Facility
        {
            Name = "Justyna Kula-Lic",
            Type = FacilityType.Therapy,
            Address = "Leżajsk",
            City = "Leżajsk",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę.",
            Website = "https://www.spzoz-lezajsk.pl/?c=mdTresc-cmPokaz-269",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f48);

        var f49 = new Facility
        {
            Name = "Małgorzata Kumięga",
            Type = FacilityType.Therapy,
            Address = "Ginemedica",
            City = "Wrocław",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: systemowe.",
            Website = "https://ginemedica.pl/lekarz/malgorzata-kumiega/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f49);

        var f50 = new Facility
        {
            Name = "Karolina Kuźnicka-Przerywacz",
            Type = FacilityType.Therapy,
            Address = "Przyjazna terapia",
            City = "Kraków",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: psychodynamiczne, systemowe, CBT.",
            Website = "https://www.przyjaznaterapia.pl/karolina_ku%C5%BAnicka_przerywacz-106982-blog.html",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f50);

        var f51 = new Facility
        {
            Name = "Anna Kwiatkowska-Ożegowska",
            Type = FacilityType.Therapy,
            Address = "Swarzędz, Poznań",
            City = "Swarzędz, Poznań",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę.",
            Website = "http://kwiatkowska-psycholog.pl/omnie.html",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f51);

        var f52 = new Facility
        {
            Name = "Maja Lasota",
            Type = FacilityType.Therapy,
            Address = "Wrocław",
            City = "Ginemedica",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://ginemedica.pl/lekarz/maja-lasota/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f52);

        var f53 = new Facility
        {
            Name = "Anna Lenek",
            Type = FacilityType.Therapy,
            Address = "Wellbee, SELF, PTIMA",
            City = "Gliwice",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: integratywne.",
            Website = "https://wellbee.pl/nasi-specjalisci/anna-lenek",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f53);

        var f54 = new Facility
        {
            Name = "Kamil Dante Lucci",
            Type = FacilityType.Therapy,
            Address = "Dialog",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://www.psychiatrzy.warszawa.pl/specjalisci/kamil-dante-lucci/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f54);

        var f55 = new Facility
        {
            Name = "Aleksandra Magda",
            Type = FacilityType.Therapy,
            Address = "Mind Works",
            City = "Kraków",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT, schematów.",
            Website = "https://terapia-cbt.pl/o-nas/aleksandra-magda/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f55);

        var f56 = new Facility
        {
            Name = "Karolina Magierek",
            Type = FacilityType.Therapy,
            Address = "Wejherowo, Gdynia",
            City = "Wejherowo, Gdynia",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: systemowe, EMDR.",
            Website = "https://www.znanylekarz.pl/karolina-magierek/psycholog-psychoterapeuta-seksuolog/wejherowo",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f56);

        var f57 = new Facility
        {
            Name = "Małgorzata Mańko-Marcysiak",
            Type = FacilityType.Therapy,
            Address = "Bydgoszcz",
            City = "Bydgoszcz",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: psychodynamiczne.",
            Website = "http://www.centrum-pomoc.bydgoszcz.pl/kadra/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f57);

        var f58 = new Facility
        {
            Name = "Cezary Markowski",
            Type = FacilityType.Therapy,
            Address = "Warszawa",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: psychodynamiczne.",
            Website = "https://cpp.pl/team/cezary-markowski/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f58);

        var f59 = new Facility
        {
            Name = "Agnieszka Mateja",
            Type = FacilityType.Therapy,
            Address = "Tychy",
            City = "Tychy",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę.",
            Website = "https://pracowniapsychologicznamateja.pl/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f59);

        var f60 = new Facility
        {
            Name = "Przemysław Matuszyński",
            Type = FacilityType.Therapy,
            Address = "Matiam PM",
            City = "Gniezno",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://www.matiam.pl/o-nas",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f60);

        var f61 = new Facility
        {
            Name = "Magdalena Mętkowska-Walewska",
            Type = FacilityType.Therapy,
            Address = "Gdańsk",
            City = "Gdańsk",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT, schematów.",
            Website = "https://metkowska-walewska.pl/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f61);

        var f62 = new Facility
        {
            Name = "Joanna Mikołajczyk",
            Type = FacilityType.Therapy,
            Address = "Solutio",
            City = "Poznań",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://psychologia-solutio.pl/staff/joanna-mikolajczyk-2/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f62);

        var f63 = new Facility
        {
            Name = "Łukasz Miścicki",
            Type = FacilityType.Therapy,
            Address = "Dialog",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: humanistyczne.",
            Website = "https://www.psychiatrzy.warszawa.pl/specjalisci/lukasz-miscicki/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f63);

        var f64 = new Facility
        {
            Name = "Marta Moszoro",
            Type = FacilityType.Therapy,
            Address = "Centrum Psychoterapii i Diagnozy",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: integratywne.",
            Website = "https://www.centrumpid.com/marta-moszoro",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f64);

        var f65 = new Facility
        {
            Name = "Ewa Mroczek",
            Type = FacilityType.Therapy,
            Address = "Online",
            City = "Online",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://ewamroczek.pl/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f65);

        var f66 = new Facility
        {
            Name = "Ilona Mroczkowska",
            Type = FacilityType.Therapy,
            Address = "Siedlce",
            City = "Siedlce",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: psychodynamiczne, systemowe.",
            Website = "https://www.facebook.com/p/Gabinet-Psychoterapii-Tu-i-Teraz-Ilona-Mroczkowska-100057571056343/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f66);

        var f67 = new Facility
        {
            Name = "Patrycja Nalazek",
            Type = FacilityType.Therapy,
            Address = "Relacje",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT, psychodynamiczne.",
            Website = "https://osrodekrelacje.com/zespol-psycholodzy-i-terapeuci/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f67);

        var f68 = new Facility
        {
            Name = "Marzena Okapiec",
            Type = FacilityType.Therapy,
            Address = "Opole",
            City = "Opole",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: Gestalt.",
            Website = "https://gestaltpsychoterapia.com/o-mnie/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f68);

        var f69 = new Facility
        {
            Name = "Małgorzata Ostrowska",
            Type = FacilityType.Therapy,
            Address = "HarmonJa",
            City = "Gdańsk",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: DBT, CBT, psychodynamiczne.",
            Website = "https://www.harmonja.pl/profile/malgorzata-ostrowska/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f69);

        var f70 = new Facility
        {
            Name = "Małgorzata Palacz",
            Type = FacilityType.Therapy,
            Address = "Warszawa",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://www.znanylekarz.pl/malgorzata-palacz-2/psycholog-psychoterapeuta/warszawa",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f70);

        var f71 = new Facility
        {
            Name = "Aneta Pietrzak",
            Type = FacilityType.Therapy,
            Address = "Wrocław",
            City = "Prodeste",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: TPP.",
            Website = "https://prodeste.pl/fundacja/ludzie/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f71);

        var f72 = new Facility
        {
            Name = "Magda Piórkowska",
            Type = FacilityType.Therapy,
            Address = "Łódź",
            City = "Ostoja",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://www.magdapiorkowska.com/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f72);

        var f73 = new Facility
        {
            Name = "Anna Pyla-Mazur",
            Type = FacilityType.Therapy,
            Address = "Kraków",
            City = "Kraków",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: psychodynamiczne.",
            Website = "https://annapylamazur.pl/O_MNIE",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f73);

        var f74 = new Facility
        {
            Name = "Urszula Ratajczak",
            Type = FacilityType.Therapy,
            Address = "Online (związana z Prodeste)",
            City = "Online",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: TPP.",
            Website = "https://porozmawiajmyonline.pl/psycholog/mgr-urszula-ratajczak/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f74);

        var f75 = new Facility
        {
            Name = "Grażyna Rembelska",
            Type = FacilityType.Therapy,
            Address = "Intra",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: integratywne.",
            Website = "https://osrodekintra.pl/team-view/grazyna-rembelska/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f75);

        var f76 = new Facility
        {
            Name = "Paulina Roszkowska",
            Type = FacilityType.Therapy,
            Address = "Online",
            City = "Online",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: TPP.",
            Website = "https://www.audhdroszkowska.pl/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f76);

        var f77 = new Facility
        {
            Name = "Agata Sawicka-Bocian",
            Type = FacilityType.Therapy,
            Address = "Wrocław",
            City = "Psychomedic",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://psychomedic.pl/dt_portfolios/dr-agata-sawicka-bocian-psycholog-diagnosta-wroclaw/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f77);

        var f78 = new Facility
        {
            Name = "Renata Sikorska",
            Type = FacilityType.Therapy,
            Address = "Warszawa",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę.",
            Website = "https://psychomedic.pl/dt_portfolios/mgr-renata-sikorska-diagnosta-calosciowych-zaburzen-rozwojowych-warszawa/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f78);

        var f79 = new Facility
        {
            Name = "Aleksandra Sileńska",
            Type = FacilityType.Therapy,
            Address = "Szczecin",
            City = "Szczecin",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: systemowe.",
            Website = "http://mojpsycholog.szczecin.pl/o-mnie/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f79);

        var f80 = new Facility
        {
            Name = "Magdalena Siwek",
            Type = FacilityType.Therapy,
            Address = "Kraków",
            City = "Centrum Dobrej Terapii",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://www.centrumdobrejterapii.pl/nasi-specjalisci/magdalena-siwek/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f80);

        var f81 = new Facility
        {
            Name = "Renata Składanek",
            Type = FacilityType.Therapy,
            Address = "Warszawa",
            City = "Dialog",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: integratywne.",
            Website = "https://www.psychiatrzy.warszawa.pl/specjalisci/renata-skladanek/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f81);

        var f82 = new Facility
        {
            Name = "Dorota Stanisławska",
            Type = FacilityType.Therapy,
            Address = "NFZ",
            City = "Multimed lub Centrum Psychoterapii \"Terapiatuli\", Kalisz",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://terapiatuli.pl/zespol/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f82);

        var f83 = new Facility
        {
            Name = "Adrianna Stawarz",
            Type = FacilityType.Therapy,
            Address = "Wrocław",
            City = "Wrocław",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: integratywne.",
            Website = "https://www.znanylekarz.pl/adrianna-stawarz/psycholog-seksuolog-psychoterapeuta/wroclaw?fbclid=IwAR2esWqvtjsFKKM-80qpCKSe_Lj5cHzE78swPLnRI1WXbWaoKQxQoH4FkOU",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f83);

        var f84 = new Facility
        {
            Name = "Martyna Stefańska",
            Type = FacilityType.Therapy,
            Address = "Warszawa",
            City = "Bezpieczne Miejsce",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: DBT.",
            Website = "https://www.terapiadbt.pl/team/martyna-stefanska/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f84);

        var f85 = new Facility
        {
            Name = "Martyna Stochel-Morek",
            Type = FacilityType.Therapy,
            Address = "Persevere",
            City = "Katowice",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: psychodynamiczne.",
            Website = "https://persevere.org.pl/martyna-stochel-morek/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f85);

        var f86 = new Facility
        {
            Name = "Justyna Sulej",
            Type = FacilityType.Therapy,
            Address = "Schematy",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT, schematów.",
            Website = "https://schema-ty.pl/justyna-sulej/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f86);

        var f87 = new Facility
        {
            Name = "Agnieszka Szadurska-Prokopiuk",
            Type = FacilityType.Therapy,
            Address = "Online",
            City = "Online",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: systemowe, ericksonowskie i TPP.",
            Website = "https://szadurska-prokopiuk.pl/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f87);

        var f88 = new Facility
        {
            Name = "Zuzanna Szal",
            Type = FacilityType.Therapy,
            Address = "Wrocław",
            City = "Wrocław",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: Gestalt.",
            Website = "https://www.znanylekarz.pl/zuzanna-szal/psycholog-psycholog-dzieciecy-psychoterapeuta/wroclaw",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f88);

        var f89 = new Facility
        {
            Name = "Agnieszka Szczepocka",
            Type = FacilityType.Therapy,
            Address = "Warszawa",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://www.znanylekarz.pl/agnieszka-szczepocka/psychoterapeuta-psycholog/warszawa?utm_medium=ppc&utm_source=google&utm_term=%2Bagnieszka+%2Bszczepocka&utm_campaign=SN-Docs-Name-1&hsa_acc=4342853338&hsa_cam=1590756134&hsa_grp=90781596852&hsa_ad=444376752902&hsa_src=g&hsa_tgt=kwd-843648996694&hsa_kw=%2Bagnieszka+%2Bszczepocka&hsa_mt=b&hsa_net=adwords&hsa_ver=3&gclid=Cj0KCQjwk5ibBhDqARIsACzmgLTv9NMGjEx-CZpMopKTqt7OhTe34ik74mfdkfSMp9FH1Ihs7W1ihdEaAhFfEALw_wcB",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f89);

        var f90 = new Facility
        {
            Name = "Maria Szczuka",
            Type = FacilityType.Therapy,
            Address = "Terapeutyczna PoraDnia",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: systemowe.",
            Website = "https://poraterapii.pl/zespol/maria-szczuka",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f90);

        var f91 = new Facility
        {
            Name = "Julia Szymanowska",
            Type = FacilityType.Therapy,
            Address = "Inter Ego",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT, schematów.",
            Website = "https://interego.com.pl/zespol/julia-szymanowska/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f91);

        var f92 = new Facility
        {
            Name = "Beata Światłowska",
            Type = FacilityType.Therapy,
            Address = "COtam?",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://psychoterapiacotam.pl/zespol-specjalistow/beata-swiatlowska/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f92);

        var f93 = new Facility
        {
            Name = "Oliwia Tran",
            Type = FacilityType.Therapy,
            Address = "Poznań",
            City = "Poznań",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: EMDR, CBT.",
            Website = "https://oliwiatran.pl/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f93);

        var f94 = new Facility
        {
            Name = "Wojciech Trzaskowski",
            Type = FacilityType.Therapy,
            Address = "Inzpiro",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: Gestalt.",
            Website = "https://www.psychoterapeuci.pl/psycholodzy-i-psychoterapeuci/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f94);

        var f95 = new Facility
        {
            Name = "Kinga Tucholska",
            Type = FacilityType.Therapy,
            Address = "Integralnie.pl",
            City = "Kraków",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "http://integralnie.pl/o-nas",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f95);

        var f96 = new Facility
        {
            Name = "Beata Tylus",
            Type = FacilityType.Therapy,
            Address = "Przystań, Invimed",
            City = "Poznań",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://www.znanylekarz.pl/beata-tylus/psychoterapeuta-seksuolog/poznan",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f96);

        var f97 = new Facility
        {
            Name = "Monika Ulatowska",
            Type = FacilityType.Therapy,
            Address = "Gdańsk, Warszawa",
            City = "Gdańsk, Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://ppp4.edu.pl/zespol/monika-ulatowska-radzka/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f97);

        var f98 = new Facility
        {
            Name = "Monika Urbaniak",
            Type = FacilityType.Therapy,
            Address = "Krotoszyn, Poznań",
            City = "Krotoszyn, Poznań",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: psychodynamiczne.",
            Website = "https://www.znanylekarz.pl/monika-urbaniak-2/psycholog-psychoterapeuta/poznan-krotoszyn?fbclid=IwAR3YSxCQp4F-SQ7A6RJtVwb_GqZ7JnNX6ZHgnLOd7M2lCo98xb9Wo4Xe2JA",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f98);

        var f99 = new Facility
        {
            Name = "Michalina Voss",
            Type = FacilityType.Therapy,
            Address = "Poznań",
            City = "OTPB",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT, ACT.",
            Website = "https://otpb.pl/tarapeuci-behawioralni/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f99);

        var f100 = new Facility
        {
            Name = "Małgorzata Walęcka",
            Type = FacilityType.Therapy,
            Address = "Cogito",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT, schematów.",
            Website = "https://cogitoterapia.pl/?page_id=228&fbclid=IwAR1c_qFihCvAQTH6YZmNsqLvxSD-jSm7cGhOswfh4B6vRDLy4SAmuM8M2C4",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f100);

        var f101 = new Facility
        {
            Name = "Agnieszka Warszawa",
            Type = FacilityType.Therapy,
            Address = "Online",
            City = "Online",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: TPP, EDMR.",
            Website = "https://www.facebook.com/IWTspektrumautyzmu",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f101);

        var f102 = new Facility
        {
            Name = "Aleksandra Wierzbicka",
            Type = FacilityType.Therapy,
            Address = "InterEgo",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT, schematów.",
            Website = "https://interego.com.pl/zespol/aleksandra-wierzbicka/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f102);

        var f103 = new Facility
        {
            Name = "Dorota Wiszejko-Wierzbicka",
            Type = FacilityType.Therapy,
            Address = "Warszawa",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: analiza grupowa.",
            Website = "https://psycholog-seksuolog.waw.pl/dorota-wiszejko-wierzbicka/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f103);

        var f104 = new Facility
        {
            Name = "Anna Maria Wołyniak",
            Type = FacilityType.Therapy,
            Address = "Kobiecy aspekt",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT, EDMR.",
            Website = "https://kobiecyaspekt.pl/o-mnie,3,pl.html?fbclid=IwAR3wvg7MYbsSbaDTO-8iSqpu4TpkingQ2Xb9yTsUYyhf_ICC1qbR9uQvD0o",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f104);

        var f105 = new Facility
        {
            Name = "Karolina Woźniak",
            Type = FacilityType.Therapy,
            Address = "Gdańsk, Gdynia",
            City = "Gdańsk, Gdynia",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://www.znanylekarz.pl/karolina-wozniak-3/psychoterapeuta-psycholog/gdansk",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f105);

        var f106 = new Facility
        {
            Name = "Joanna Zawadzka-Iwanek",
            Type = FacilityType.Therapy,
            Address = "Psychopercepcje",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: CBT.",
            Website = "https://psychopercepcje.pl/Joanna_Zawadzka.html",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f106);

        var f107 = new Facility
        {
            Name = "Paulina Zawadzka",
            Type = FacilityType.Therapy,
            Address = "Artonomia",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: TPP.",
            Website = "https://artonomia.org/o-nas/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f107);

        var f108 = new Facility
        {
            Name = "Ewa Zawisza-Wilk",
            Type = FacilityType.Therapy,
            Address = "Kraków",
            City = "Centrum Metody Krakowskiej",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę.",
            Website = "https://centrummetodykrakowskiej.pl/blog/zespol-aspergera-wywiad-z-dr-ewa-zawisza-wilk/",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f108);

        var f109 = new Facility
        {
            Name = "Dagmara Zbroch",
            Type = FacilityType.Therapy,
            Address = "Psychomedic",
            City = "Warszawa",
            Description = "Terapeut(k)a polecan(a) przez Idę Tyminę. Podejście: psychodynamiczne.",
            Website = "https://psychomedic.pl/dt_portfolios/mgr-dagmara-zbroch-psycholog-psychoterapeuta-dzieci-i-mlodziezy/#naglowekart",
            Source = "Ida Tymina",
            VerificationStatus = VerificationStatus.Verified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(f109);

        // 3a. Create certified facilities (Fundacja Dziewczyny w Spektrum)
        var cert1 = new Facility
        {
            Name = "Barbara Molęda-Kusidło (Gabinet Psychoterapii EXPECTA)",
            Type = FacilityType.Therapy,
            Address = "Racibórz",
            City = "Racibórz",
            Description = "Placówka certyfikowana przez Fundację Dziewczyny w Spektrum (Certyfikat Spektrum Inkluzywności). Zakres: diagnostyka spektrum autyzmu u osób dorosłych, psychoterapia dorosłych osób w spektrum autyzmu.",
            Website = "https://dziewczynywspektrum.pl/certyfikacja/",
            Source = "Fundacja Dziewczyny w Spektrum",
            VerificationStatus = VerificationStatus.Certified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(cert1);

        var cert2 = new Facility
        {
            Name = "Gabinet Psychoterapii Wzmocnienie",
            Type = FacilityType.Therapy,
            Address = "Kraków",
            City = "Kraków",
            Description = "Placówka certyfikowana przez Fundację Dziewczyny w Spektrum (Certyfikat Spektrum Inkluzywności). Zakres: diagnostyka spektrum autyzmu u osób dorosłych, psychoterapia dorosłych osób w spektrum autyzmu. Wiele osób poleca proces diagnostyczny prowadzony przez p. Aleksandrę.",
            Website = "https://dziewczynywspektrum.pl/certyfikacja/",
            Source = "Fundacja Dziewczyny w Spektrum",
            VerificationStatus = VerificationStatus.Certified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(cert2);

        var cert3 = new Facility
        {
            Name = "Centrum Diagnozy i Terapii TUiTAM",
            Type = FacilityType.Therapy,
            Address = "Łódź",
            City = "Łódź",
            Description = "Placówka certyfikowana przez Fundację Dziewczyny w Spektrum (Certyfikat Spektrum Inkluzywności). Zakres: diagnostyka spektrum autyzmu u osób dorosłych.",
            Website = "https://dziewczynywspektrum.pl/certyfikacja/",
            Source = "Fundacja Dziewczyny w Spektrum",
            VerificationStatus = VerificationStatus.Certified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(cert3);

        var cert4 = new Facility
        {
            Name = "Urszula Strzelczyk-Raduli (Pomoc Psychologiczno-Pedagogiczna)",
            Type = FacilityType.Therapy,
            Address = "Kędzierzyn-Koźle",
            City = "Kędzierzyn-Koźle",
            Description = "Placówka certyfikowana przez Fundację Dziewczyny w Spektrum (Certyfikat Spektrum Inkluzywności). Zakres: diagnostyka spektrum autyzmu u osób dorosłych.",
            Website = "https://dziewczynywspektrum.pl/certyfikacja/",
            Source = "Fundacja Dziewczyny w Spektrum",
            VerificationStatus = VerificationStatus.Certified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(cert4);

        var cert5 = new Facility
        {
            Name = "Justyna Marquardt (Katowicki Instytut Psychoterapii)",
            Type = FacilityType.Therapy,
            Address = "Katowice",
            City = "Katowice",
            Description = "Placówka certyfikowana przez Fundację Dziewczyny w Spektrum (Certyfikat Spektrum Inkluzywności). Zakres: diagnostyka spektrum autyzmu u osób dorosłych, psychoterapia dorosłych osób w spektrum autyzmu.",
            Website = "https://dziewczynywspektrum.pl/certyfikacja/",
            Source = "Fundacja Dziewczyny w Spektrum",
            VerificationStatus = VerificationStatus.Certified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(cert5);

        var cert6 = new Facility
        {
            Name = "Magdalena Kubicka Psychoterapia",
            Type = FacilityType.Therapy,
            Address = "Gdańsk",
            City = "Gdańsk",
            Description = "Placówka certyfikowana przez Fundację Dziewczyny w Spektrum (Certyfikat Spektrum Inkluzywności). Zakres: psychoterapia dorosłych osób w spektrum autyzmu.",
            Website = "https://dziewczynywspektrum.pl/certyfikacja/",
            Source = "Fundacja Dziewczyny w Spektrum",
            VerificationStatus = VerificationStatus.Certified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(cert6);

        var cert7 = new Facility
        {
            Name = "Justyna Paprota Psychoterapia",
            Type = FacilityType.Therapy,
            Address = "Warszawa",
            City = "Warszawa",
            Description = "Placówka certyfikowana przez Fundację Dziewczyny w Spektrum (Certyfikat Spektrum Inkluzywności). Zakres: psychoterapia dorosłych osób w spektrum autyzmu.",
            Website = "https://dziewczynywspektrum.pl/certyfikacja/",
            Source = "Fundacja Dziewczyny w Spektrum",
            VerificationStatus = VerificationStatus.Certified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(cert7);

        var cert8 = new Facility
        {
            Name = "Natalia Jabłońska (Potęga Relacji)",
            Type = FacilityType.Therapy,
            Address = "Kraków",
            City = "Kraków",
            Description = "Placówka certyfikowana przez Fundację Dziewczyny w Spektrum (Certyfikat Spektrum Inkluzywności). Zakres: psychoterapia dorosłych osób w spektrum autyzmu.",
            Website = "https://dziewczynywspektrum.pl/certyfikacja/",
            Source = "Fundacja Dziewczyny w Spektrum",
            VerificationStatus = VerificationStatus.Certified,
            VerifiedById = adminUser.Id,
            VerifiedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        context.Facilities.Add(cert8);

        // Seed reviews for the first therapist to make sure tests pass
        var review = new Review
        {
            FacilityId = f1.Id,
            UserId = testUser.Id,
            Rating = 5,
            Comment = "Świetna terapeutka! Bardzo polecam.",
            IsApproved = true,
            ApprovedById = adminUser.Id,
            ApprovedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };
        context.Reviews.Add(review);
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

Polecam sprawdzić naszą sekcję z placówkami - znajdziesz tam listę zweryfikowanych i polecanych terapeutów. Wielu rodziców poleca np. Martę Baron w Warszawie.

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
        Console.WriteLine($"   - Facilities: 117");
        Console.WriteLine($"   - Reviews: 2");
        Console.WriteLine($"   - Forum topics: 2");
        Console.WriteLine($"   - Forum posts: 3");
    }
}
