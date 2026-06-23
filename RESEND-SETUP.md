# 📧 Konfiguracja Resend - Email Service

## Czym jest Resend?

[Resend](https://resend.com) to nowoczesna platforma do wysyłania emaili, stworzona specjalnie dla programistów. Jest prostsza w konfiguracji niż tradycyjny SMTP i oferuje:

- ✅ **Wysoka doręczalność** - lepsze dotarcie do skrzynek odbiorczych
- ✅ **Prosta integracja** - jedno API, bez konfiguracji SMTP
- ✅ **Darmowy plan** - 3,000 emaili/miesiąc za darmo
- ✅ **Weryfikacja domeny** - zwiększa reputację nadawcy
- ✅ **Monitoring** - śledzenie dostarczonych, otwartych i klikniętych emaili
- ✅ **Szablony HTML** - wsparcie dla nowoczesnych szablonów email

## 🚀 Szybki Start

### 1. Załóż konto Resend

1. Przejdź do https://resend.com/signup
2. Zarejestruj się używając GitHub lub email
3. Potwierdź swój adres email

### 2. Dodaj i zweryfikuj domenę

#### Opcja A: Własna domena (REKOMENDOWANE dla produkcji)

1. W dashboardzie Resend kliknij **Domains** → **Add Domain**
2. Wpisz swoją domenę (np. `oaza.pl`)
3. Dodaj rekordy DNS podane przez Resend:
   ```
   TXT Record: resend._domainkey.oaza.pl
   Value: [wartość podana przez Resend]
   ```
4. Poczekaj na weryfikację (może zająć do 24h, zwykle kilka minut)

**Przykładowe ustawienia DNS:**
```
Type: TXT
Host: resend._domainkey
Value: k=rsa;p=MIGfMA0GCSqGSIb3...
TTL: 3600
```

#### Opcja B: Subdomena Resend (dla testów)

Resend automatycznie daje ci subdomenę `onresend.com` która działa od razu, ale:
- ⚠️ Mniejsza doręczalność
- ⚠️ Nie zalecane dla produkcji
- ✅ Dobre do testów

### 3. Wygeneruj API Key

1. W dashboardzie kliknij **API Keys** → **Create API Key**
2. Nazwij klucz (np. "Oaza Production")
3. Wybierz uprawnienia: **Full Access** lub **Sending Access**
4. Skopiuj klucz (zaczyna się od `re_`)
5. ⚠️ **WAŻNE**: Zapisz klucz bezpiecznie - nie zobaczysz go ponownie!

### 4. Skonfiguruj aplikację

#### Development (appsettings.Development.json):

```json
{
  "EmailSettings": {
    "Provider": "Resend",
    "ResendApiKey": "re_123456789_twojKluczTestowy",
    "SenderEmail": "dev@resend.dev",
    "SenderName": "Oaza dla Autyzmu [TEST]"
  }
}
```

#### Production (zmienne środowiskowe Azure):

```bash
EmailSettings__Provider=Resend
EmailSettings__ResendApiKey=re_prod_xyz...
EmailSettings__SenderEmail=noreply@oaza.pl
EmailSettings__SenderName=Oaza dla Autyzmu
```

## 🔧 Szczegółowa konfiguracja

### Zmienne środowiskowe

| Zmienna | Opis | Przykład | Wymagana |
|---------|------|----------|----------|
| `EmailSettings__Provider` | Wybór providera email | `Resend` lub `SMTP` | Tak |
| `EmailSettings__ResendApiKey` | Klucz API Resend | `re_123...` | Tak (gdy Provider=Resend) |
| `EmailSettings__SenderEmail` | Email nadawcy | `noreply@oaza.pl` | Tak |
| `EmailSettings__SenderName` | Nazwa nadawcy | `Oaza dla Autyzmu` | Nie |

### Konfiguracja w Azure

```bash
# Ustaw zmienne w Azure App Service
az webapp config appsettings set \
  --name oaza-app \
  --resource-group oaza-rg \
  --settings \
    "EmailSettings__Provider=Resend" \
    "EmailSettings__ResendApiKey=re_prod_xyz..." \
    "EmailSettings__SenderEmail=noreply@oaza.pl" \
    "EmailSettings__SenderName=Oaza dla Autyzmu"
```

### Użycie w kodzie

Serwis `IEmailService` automatycznie używa Resend jeśli jest skonfigurowany:

```csharp
public class ContactController : Controller
{
    private readonly IEmailService _emailService;
    
    public ContactController(IEmailService emailService)
    {
        _emailService = emailService;
    }
    
    public async Task<IActionResult> SendResponse()
    {
        await _emailService.SendEmailAsync(
            to: "user@example.com",
            subject: "Odpowiedź z placówki",
            htmlBody: "<h1>Dziękujemy za kontakt!</h1>"
        );
        
        return Ok();
    }
}
```

## 📊 Limity i ceny

### Plan darmowy
- 3,000 emaili/miesiąc
- 100 emaili/dzień
- Weryfikacja domeny
- Podstawowe statystyki

### Plan Pro ($20/miesiąc)
- 50,000 emaili/miesiąc
- Bez limitu dziennego
- Zaawansowane statystyki
- Webhooks
- Dedykowane IP

[Pełny cennik](https://resend.com/pricing)

## 🔍 Testowanie

### Test lokalny

1. Ustaw klucz API w `appsettings.Development.json`
2. Użyj adresu testowego Resend: `delivered@resend.dev`

```csharp
await _emailService.SendEmailAsync(
    "delivered@resend.dev", 
    "Test Email", 
    "<h1>Test działa!</h1>"
);
```

3. Sprawdź logi w konsoli lub w Resend Dashboard

### Test w Azure

```bash
# Wyślij test email przez Azure CLI
az webapp log tail --name oaza-app --resource-group oaza-rg
```

## 🐛 Rozwiązywanie problemów

### Problem: "Email not sent - Resend not configured"

**Rozwiązanie:**
- Sprawdź czy `EmailSettings__ResendApiKey` jest ustawiony
- Sprawdź czy klucz zaczyna się od `re_`
- Sprawdź logi aplikacji

### Problem: "401 Unauthorized"

**Rozwiązanie:**
- Klucz API jest nieprawidłowy lub wygasł
- Wygeneruj nowy klucz w Resend Dashboard
- Zaktualizuj zmienną środowiskową

### Problem: "Domain not verified"

**Rozwiązanie:**
- Sprawdź rekordy DNS w panelu domeny
- Poczekaj do 24h na propagację DNS
- Użyj `onresend.com` jako tymczasowej domeny

### Problem: Emaile trafiają do SPAM

**Rozwiązanie:**
- Zweryfikuj domenę w Resend
- Dodaj rekordy SPF i DKIM
- Użyj profesjonalnego szablonu HTML
- Unikaj słów spam w temacie

## 📚 Dodatkowe zasoby

- 📖 [Dokumentacja Resend](https://resend.com/docs)
- 🎯 [.NET SDK](https://resend.com/docs/send-with-dotnet)
- 💬 [Discord Community](https://resend.com/discord)
- 📧 [Support](mailto:support@resend.com)

## 🔄 Migracja z SMTP na Resend

Jeśli używasz obecnie SMTP (Gmail, SendGrid, etc.), możesz łatwo przełączyć się na Resend:

1. Ustaw `EmailSettings__Provider=Resend`
2. Dodaj `EmailSettings__ResendApiKey`
3. Zrestartuj aplikację
4. Stare ustawienia SMTP zostaną zignorowane

**Rollback do SMTP:**
```bash
# Przełącz z powrotem na SMTP
az webapp config appsettings set \
  --name oaza-app \
  --resource-group oaza-rg \
  --settings "EmailSettings__Provider=SMTP"
```

## ✅ Checklist produkcyjna

- [ ] Konto Resend założone i zweryfikowane
- [ ] Domena dodana i zweryfikowana (SPF, DKIM)
- [ ] API Key wygenerowany (Full Access)
- [ ] Zmienne środowiskowe ustawione w Azure
- [ ] Test email wysłany i dostarczony
- [ ] Monitoring włączony w Resend Dashboard
- [ ] Alerty email skonfigurowane
- [ ] Limity API sprawdzone (3000/miesiąc dla free tier)

---

**🎉 Gotowe!** Twoja aplikacja może teraz wysyłać emaile przez Resend.
