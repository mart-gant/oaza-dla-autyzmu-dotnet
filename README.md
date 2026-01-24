# 🧩 Oaza dla Autyzmu - ASP.NET Core

Platforma wsparcia dla osób z autyzmem i ich rodzin. Migracja z Laravel do ASP.NET Core z wykorzystaniem Clean Architecture i CQRS.

## 🎯 Status projektu

✅ **FAZA 1 - MVP ZAKOŃCZONA!**

- ✅ Clean Architecture (Domain, Application, Infrastructure, Web)
- ✅ Entity Framework Core + PostgreSQL
- ✅ ASP.NET Core Identity
- ✅ CQRS z MediatR
- ✅ Domain Entities (Facility, Review, Article, Forum, Event)
- ✅ Facilities CRUD (Create, Read, Update, Delete)
- ✅ Razor Views z Tailwind CSS
- ✅ Migracje bazy danych

## 🏗️ Architektura

```
src/
├── OazaDlaAutyzmu.Web/                 # ASP.NET Core MVC + API
├── OazaDlaAutyzmu.Application/         # Business Logic (CQRS)
├── OazaDlaAutyzmu.Domain/              # Domain Models
├── OazaDlaAutyzmu.Infrastructure/      # Data Access + Services
└── tests/OazaDlaAutyzmu.Tests/         # Unit Tests
```

## 🚀 Jak uruchomić

### Wymagania:
- .NET 10 SDK
- PostgreSQL 14+
- Visual Studio 2022 / VS Code

### Krok 1: Sklonuj repozytorium
```bash
git clone https://github.com/your-username/oaza-dla-autyzmu-dotnet.git
cd oaza-dla-autyzmu-dotnet
```

### Krok 2: Skonfiguruj connection string
Edytuj `src/OazaDlaAutyzmu.Web/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=oaza_dla_autyzmu;Username=postgres;Password=your_password"
  }
}
```

### Krok 3: Uruchom migracje
```bash
cd src/OazaDlaAutyzmu.Web
dotnet ef database update --project ../OazaDlaAutyzmu.Infrastructure/OazaDlaAutyzmu.Infrastructure.csproj
```

### Krok 4: Uruchom aplikację
```bash
dotnet run
```

Aplikacja będzie dostępna pod adresem: `https://localhost:5001`

## 📦 Stack technologiczny

### Backend:
- **ASP.NET Core 10.0** - Web framework
- **Entity Framework Core 10.0** - ORM
- **PostgreSQL** - Database
- **MediatR 12.2** - CQRS pattern
- **FluentValidation 11.9** - Validation
- **ASP.NET Core Identity** - Authentication & Authorization

### Frontend:
- **Razor Pages** - Server-side rendering
- **Tailwind CSS** - Styling (via CDN)
- **Alpine.js** - (planowane) JavaScript interactions

### Testing:
- **xUnit** - Unit testing framework

## 🗂️ Struktura bazy danych

### Główne tabele:
- `facilities` - Placówki dla osób z autyzmem
- `reviews` - Opinie użytkowników o placówkach
- `articles` - Artykuły edukacyjne
- `article_categories` - Kategorie artykułów
- `forum_categories` - Kategorie forum
- `forum_topics` - Tematy forum
- `forum_posts` - Posty w forum
- `events` - Wydarzenia
- `users` - Użytkownicy (ASP.NET Identity)

## 🎨 Główne funkcjonalności

### ✅ Zaimplementowane:
- Przeglądanie placówek z filtrowaniem (miasto, typ, status)
- Szczegóły placówki z informacjami kontaktowymi
- Dodawanie nowych placówek (Admin/Moderator)
- Edycja placówek (Admin/Moderator)
- Usuwanie placówek (Admin)
- Responsywny design z Tailwind CSS

### 🔜 W kolejnej wersji:
- System rejestracji i logowania
- Opinie o placówkach
- Forum dyskusyjne
- Artykuły edukacyjne
- Wydarzenia
- Wiadomości prywatne
- Panel administratora
- Mapa placówek (Google Maps / OpenStreetMap)

## 📝 Przykładowe użycie CQRS

### Command (Dodawanie placówki):
```csharp
var command = new CreateFacilityCommand
{
    Name = "Centrum Terapii ABC",
    City = "Warszawa",
    Address = "ul. Przykładowa 123",
    Type = FacilityType.Therapy
};

var id = await _mediator.Send(command);
```

### Query (Pobieranie placówek):
```csharp
var query = new GetFacilitiesQuery
{
    City = "Warszawa",
    Type = FacilityType.Therapy,
    Status = VerificationStatus.Verified
};

var facilities = await _mediator.Send(query);
```

## 🧪 Testowanie

```bash
cd tests/OazaDlaAutyzmu.Tests
dotnet test
```

## 📚 Dodawanie nowej migracji

```bash
cd src/OazaDlaAutyzmu.Web
dotnet ef migrations add NazwaMigracji --project ../OazaDlaAutyzmu.Infrastructure/OazaDlaAutyzmu.Infrastructure.csproj
dotnet ef database update --project ../OazaDlaAutyzmu.Infrastructure/OazaDlaAutyzmu.Infrastructure.csproj
```

## 🌍 Deployment

### Azure App Service (zalecane):
```bash
az login
az group create --name OazaDlaAutyzmu --location northeurope
az appservice plan create --name OazaDlaAutyzmuPlan --resource-group OazaDlaAutyzmu --sku B1 --is-linux
az webapp create --name oaza-dla-autyzmu --resource-group OazaDlaAutyzmu --plan OazaDlaAutyzmuPlan --runtime "DOTNET:10.0"
```

### Docker:
```bash
docker build -t oaza-dla-autyzmu .
docker run -p 8080:80 oaza-dla-autyzmu
```

## 🤝 Wkład w projekt

1. Fork repozytorium
2. Stwórz branch dla nowej funkcjonalności (`git checkout -b feature/AmazingFeature`)
3. Commit zmian (`git commit -m 'Add some AmazingFeature'`)
4. Push do brancha (`git push origin feature/AmazingFeature`)
5. Otwórz Pull Request

## 📄 Licencja

MIT License - szczegóły w pliku `LICENSE`

## 📧 Kontakt

- Email: kontakt@oazadlaautyzmu.pl
- GitHub: [@your-username](https://github.com/your-username)

## 🙏 Podziękowania

Projekt powstał jako migracja aplikacji Laravel do ASP.NET Core, z myślą o społeczności osób z autyzmem i ich rodzin.

---

**Developed with ❤️ for the autism community**
