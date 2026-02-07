# 🧩 Oaza dla Autyzmu

Kompleksowa platforma wsparcia dla osób z autyzmem i ich rodzin. System umożliwia wyszukiwanie placówek terapeutycznych, wymianę opinii oraz dyskusję na forum.

## ✨ Funkcje

### Core Features
- 🏢 **System Placówek** - Wyszukiwanie i filtrowanie placówek terapeutycznych
- ⭐ **System Opinii** - Wystawianie opinii z moderacją
- 💬 **Forum Dyskusyjne** - Kategorie tematyczne z systemem postów
- 🔐 **Zarządzanie Użytkownikami** - Rejestracja, logowanie, role (User, Moderator, Admin)
- 📸 **Galerie Zdjęć** - Upload i zarządzanie zdjęciami placówek (max 5MB, lightbox)
- 📧 **Formularze Kontaktowe** - System wiadomości do placówek z powiadomieniami

### Advanced Features
- 📊 **Panel Administracyjny** - Dashboard ze statystykami, wykresami wzrostu, zarządzanie wiadomościami
- 🔔 **System Powiadomień** - Real-time powiadomienia o wydarzeniach (recenzje, wiadomości, moderacja)
- 🔌 **REST API** - Pełne API z dokumentacją Swagger (v1)
- 🎨 **Responsywny Design** - Dark mode, mobile menu, touch-friendly, Tailwind CSS
- ⚡ **Performance** - Caching, compression, 19 database indexes, image optimization
- ♿ **Dostępność** - Wysoki kontrast, większy tekst, mniej ruchu, tryb spokojny

### Security (14 Features ✅)
CSRF • XSS • Rate Limiting • 2FA • Email Confirmation • Account Lockout • Security Headers • Audit Logging • Password Reset • Session Timeout • HTTPS + HSTS • Content Moderation • GDPR • reCAPTCHA

## 🛠 Stack Technologiczny

- **.NET 10.0** - ASP.NET Core MVC
- **Entity Framework Core** - ORM z SQLite
- **MediatR** - CQRS pattern
- **FluentValidation** - Input validation
- **Tailwind CSS** - Styling
- **Swashbuckle** - API documentation
- **xUnit + Moq + Playwright** - Testing (45 tests total: 22 unit + 9 integration + 14 E2E)

## 📦 Wymagania

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQLite (included)
- Git

## 🚀 Quick Start

```bash
# Clone repository
git clone https://github.com/your-username/oaza-dla-autyzmu-dotnet.git
cd oaza-dla-autyzmu-dotnet

# Restore packages
dotnet restore

# Apply database migrations
cd src/OazaDlaAutyzmu.Infrastructure
dotnet ef database update --startup-project ../OazaDlaAutyzmu.Web

# Run application
cd ../OazaDlaAutyzmu.Web
dotnet run
```

Aplikacja: `https://localhost:5050`  
API Docs: `https://localhost:5050/api/docs`

## ⚙️ Konfiguracja

### appsettings.json

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password"
  },
  "RecaptchaSettings": {
    "SiteKey": "your-recaptcha-site-key",
    "SecretKey": "your-recaptcha-secret-key"
  }
}
```

### Test Users (seeded)
- **Admin**: admin@oaza.pl / Admin123!
- **User**: test@oaza.pl / Test123!

### Environment Variables (optional)
- `ASPNETCORE_ENVIRONMENT` - Development/Production
- `USE_INMEMORY_DB` - "true" for in-memory database (testing)

## 🧪 Testy

```bash
# All tests
dotnet test

# Unit tests only
dotnet test --filter "FullyQualifiedName!~Integration&FullyQualifiedName!~E2E"

# Integration tests only
dotnet test --filter "FullyQualifiedName~Integration"

# E2E tests (requires Playwright browsers)
dotnet test --filter "FullyQualifiedName~E2E"
```

**Test Coverage**: 31/45 passing ✅ (14 E2E skipped - require browser setup)
- **Unit Tests** (22) - Validators, handlers, pagination, commands
- **Integration Tests** (9) - API endpoints, health checks
- **E2E Tests** (14) - Authentication, facilities, galleries, contact forms

### Setup E2E Tests
```bash
# Install Playwright browsers
pwsh tests/OazaDlaAutyzmu.Tests/bin/Debug/net10.0/playwright.ps1 install

# Remove Skip attribute from E2E tests to run them
```

## 📚 API Endpoints

### Facilities
```http
GET  /api/v1/facilities              # List (paginated)
GET  /api/v1/facilities/{id}         # Details
GET  /api/v1/facilities/{id}/reviews # Reviews
```

### Reviews
```http
GET  /api/v1/reviews?facilityId={id} # List
POST /api/v1/reviews                  # Create (auth)
```

### Gallery
```http
GET  /Gallery/Index/{facilityId}     # View gallery
GET  /Gallery/Upload/{facilityId}    # Upload form (Admin/Owner)
POST /Gallery/Upload                  # Upload image (max 5MB: jpg, png, gif, webp)
POST /Gallery/SetMain/{imageId}      # Set main image
POST /Gallery/Delete/{imageId}       # Delete image
```

### Contact
```http
GET  /Contact/Index/{facilityId}     # Contact form
POST /Contact/Send                    # Send message
GET  /Contact/Messages                # Admin: view messages
POST /Contact/MarkAsRead/{id}        # Mark message as read
POST /Contact/Delete/{id}            # Delete message
```

### Forum
```http
GET  /api/v1/forum/categories                # List
GET  /api/v1/forum/categories/{id}/topics    # Topics
GET  /api/v1/forum/topics/{id}               # Topic + posts
POST /api/v1/forum/topics                    # Create (auth)
POST /api/v1/forum/topics/{id}/posts         # Reply (auth)
```

## 🔒 Security Features

Szczegóły w [SECURITY.md](SECURITY.md)

- **Authentication**: ASP.NET Identity + 2FA
- **Authorization**: Role-based (User, Moderator, Admin)
- **Protection**: CSRF, XSS, Rate Limiting, reCAPTCHA
- **Privacy**: GDPR compliance, audit logging
- **Headers**: CSP, HSTS, X-Frame-Options

## 🚢 Deployment

### Docker
```bash
docker build -t oaza-dla-autyzmu .
docker run -p 8080:80 oaza-dla-autyzmu
```

### Production
```bash
dotnet publish -c Release -o ./publish
cd publish
dotnet OazaDlaAutyzmu.Web.dll
```

## 📊 Architecture

```
src/
├── OazaDlaAutyzmu.Domain/         # Entities, interfaces
├── OazaDlaAutyzmu.Application/    # CQRS, DTOs, validators
├── OazaDlaAutyzmu.Infrastructure/ # EF Core, services
└── OazaDlaAutyzmu.Web/            # MVC, API controllers

tests/
└── OazaDlaAutyzmu.Tests/          # Unit tests (xUnit)
```

**Pattern**: Clean Architecture + CQRS with MediatR

## 🤝 Contributing

1. Fork repository
2. Create feature branch
3. Commit changes
4. Push and open PR

## 📝 Licencja

MIT License - see [LICENSE](LICENSE)

---

Built with ❤️ for the autism community in Poland

**Audyt dostępności:** szczegóły w [docs/ACCESSIBILITY_AUDIT.md](docs/ACCESSIBILITY_AUDIT.md)

---

## ✅ Checklist produkcyjny

**Bezpieczeństwo i zgodność**
- [ ] CSP, HSTS, X-Content-Type-Options, Referrer-Policy zweryfikowane
- [ ] Skanowanie uploadów (AV) + walidacja MIME/rozmiarów
- [ ] Polityka prywatności i zgody RODO w UI

**Monitoring i niezawodność**
- [ ] Centralne logi (strukturalne) i alerty błędów
- [ ] Sentry skonfigurowany (DSN w ustawieniach środowiska)
- [ ] Backup bazy + procedura odtworzeniowa
- [ ] Health checks w środowisku produkcyjnym

**Wydajność**
- [ ] Cache dla list i detali, kompresja statycznych
- [ ] Lazy-loading obrazów, optymalizacja rozmiarów

**Operacje**
- [ ] CI/CD: build + test + security scan
- [ ] Sekrety w konfiguracji środowiska (nie w plikach)
- [ ] Kontrolowany proces migracji bazy
