# ✅ Checklist Gotowości do Produkcji

## 🔒 Bezpieczeństwo

- [ ] **HTTPS włączony** (SSL/TLS certificate)
- [ ] **Secrets w zmiennych środowiskowych** (NIE w kodzie/appsettings.json)
- [ ] **Connection string zabezpieczony** (nie w repozytorium)
- [ ] **SMTP credentials w zmiennych środowiskowych**
- [ ] **reCAPTCHA keys skonfigurowane**
- [ ] **Hasło administratora ZMIENIONE** (domyślne: Admin123!)
- [ ] **Rate limiting włączony i skonfigurowany**
- [ ] **CORS skonfigurowany** (jeśli używasz API)
- [ ] **Anti-forgery tokens włączone** ✅
- [ ] **XSS protection włączony** (Content-Security-Policy) ✅
- [ ] **SQL Injection protection** (Entity Framework) ✅

## 🗄️ Baza danych

- [ ] **Zmiana z SQLite na PostgreSQL/SQL Server**
- [ ] **Connection string do produkcyjnej bazy**
- [ ] **Migracje uruchomione** (`dotnet ef database update`)
- [ ] **Backupy skonfigurowane** (automatyczne)
- [ ] **Indeksy bazodanowe** (sprawdź performance)

## 📧 Email

- [ ] **SMTP serwer skonfigurowany**
- [ ] **SMTP credentials ustawione**
- [ ] **Test wysyłania emaili** (rejestracja, reset hasła)
- [ ] **Email templates przetestowane**

## 🔧 Konfiguracja

- [ ] **`appsettings.Production.json` utworzony** ✅
- [ ] **`ASPNETCORE_ENVIRONMENT=Production`** ustawione
- [ ] **Logging level = Warning** (nie Information w produkcji)
- [ ] **AllowedHosts ustawiony** (twoja domena)
- [ ] **Sentry DSN skonfigurowany** (monitoring błędów)

## 🚀 Deployment

- [ ] **Dockerfile działa** ✅
- [ ] **docker-compose.yml skonfigurowany** ✅
- [ ] **Build działa bez błędów** ✅
- [ ] **Publish profil utworzony** (Visual Studio)
- [ ] **CI/CD pipeline skonfigurowany** (GitHub Actions/Azure DevOps)

## 🧪 Testy

- [ ] **Testy jednostkowe przechodzą**
- [ ] **Testy E2E przechodzą**
- [ ] **Manual testing wykonany:**
  - [ ] Rejestracja użytkownika
  - [ ] Logowanie/wylogowanie
  - [ ] Tworzenie tematu na forum
  - [ ] Dodawanie posta
  - [ ] Tworzenie opinii o placówce
  - [ ] Formularz kontaktowy
  - [ ] Panel administratora
  - [ ] Panel moderatora

## 📊 Monitoring & Performance

- [ ] **Sentry skonfigurowany** (error tracking)
- [ ] **Application Insights/CloudWatch** (jeśli Azure/AWS)
- [ ] **Response compression włączona** ✅
- [ ] **Response caching skonfigurowany** ✅
- [ ] **Health check endpoint działa** (/health)
- [ ] **Logs zapisywane** (Serilog) ✅

## 🌐 Infrastruktura

- [ ] **Domena zarejestrowana** (np. oaza-dla-autyzmu.pl)
- [ ] **DNS skonfigurowany** (A record → IP serwera)
- [ ] **SSL certificate zainstalowany**
- [ ] **CDN skonfigurowany** (opcjonalnie - Cloudflare)
- [ ] **Firewall rules ustawione**

## 📝 Dokumentacja

- [ ] **DEPLOYMENT.md przeczytany** ✅
- [ ] **README.md zaktualizowany**
- [ ] **API dokumentacja aktualna** (jeśli używasz API)
- [ ] **Dane kontaktowe aktualne**

## 🎯 Po wdrożeniu

- [ ] **Smoke tests wykonane** (podstawowe funkcje działają)
- [ ] **Monitoring aktywny** (sprawdzaj logi!)
- [ ] **Backupy weryfikowane** (przywróć testowo)
- [ ] **Hasła adminów zmienione**
- [ ] **Seeded data usunięte/zmienione** (test@oaza.pl)
- [ ] **Google Analytics/Matomo skonfigurowane** (opcjonalnie)

---

## ⚠️ KRYTYCZNE przed publikacją!

```bash
# 1. Sprawdź czy secrets NIE są w kodzie:
git log --all --full-history -- "*appsettings.json" | grep -i password

# 2. Sprawdź czy .env NIE jest w repo:
git ls-files | grep -E "\.env$|appsettings.Production.json"

# 3. Sprawdź otwarte porty:
nmap -sV your-server-ip

# 4. Test bezpieczeństwa:
# - https://observatory.mozilla.org/
# - https://securityheaders.com/
```

---

## 📞 Wsparcie

Jeśli napotkasz problemy:
1. Sprawdź logi: `docker logs oaza-web`
2. Sprawdź health check: `https://your-domain.com/health`
3. Sentry dashboard (jeśli skonfigurowane)
4. GitHub Issues

**Powodzenia! 🚀**
