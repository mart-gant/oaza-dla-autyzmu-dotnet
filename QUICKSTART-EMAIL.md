# 🚀 Szybka Konfiguracja Email (Resend)

## Dla developera (5 minut)

### 1. Zarejestruj się na Resend
```bash
# Otwórz w przeglądarce
https://resend.com/signup
```

### 2. Wygeneruj API Key
1. Dashboard → API Keys → Create API Key
2. Skopiuj klucz (zaczyna się od `re_`)

### 3. Dodaj do appsettings.Development.json
```json
{
  "EmailSettings": {
    "Provider": "Resend",
    "ResendApiKey": "re_TwojKluczTutaj",
    "SenderEmail": "onboarding@resend.dev",
    "SenderName": "Oaza dla Autyzmu [DEV]"
  }
}
```

### 4. Testuj!
```csharp
// Email zostanie wysłany przez Resend
await _emailService.SendEmailAsync(
    "delivered@resend.dev",
    "Test",
    "<h1>Działa!</h1>"
);
```

## Dla Azure (produkcja)

### Metoda 1: Użyj skryptu deploy-azure.sh

1. Edytuj `deploy-azure.sh`:
```bash
RESEND_API_KEY="re_TwójProdukcyjnyKlucz"
SENDER_EMAIL="noreply@twojadomena.pl"
```

2. Uruchom skrypt:
```bash
./deploy-azure.sh
```

### Metoda 2: Ręczna konfiguracja

```bash
az webapp config appsettings set \
  --name twoja-aplikacja \
  --resource-group twoja-grupa \
  --settings \
    "EmailSettings__Provider=Resend" \
    "EmailSettings__ResendApiKey=re_xyz..." \
    "EmailSettings__SenderEmail=noreply@twojadomena.pl" \
    "EmailSettings__SenderName=Oaza dla Autyzmu"
```

## Weryfikacja działania

### W developmencie
```bash
# Uruchom aplikację
dotnet run --project src/OazaDlaAutyzmu.Web

# Sprawdź logi w konsoli
# Powinno być: "Email sent successfully to ..."
```

### W Azure
```bash
# Podejrzyj logi
az webapp log tail --name twoja-aplikacja --resource-group twoja-grupa

# Lub w Azure Portal
# App Service → Monitoring → Log stream
```

## Limity darmowego planu

- ✅ 3,000 emaili/miesiąc
- ✅ 100 emaili/dzień
- ✅ Weryfikacja domeny
- ✅ Podstawowe statystyki

Dla większej liczby emaili: https://resend.com/pricing

## Troubleshooting

**Problem: "Resend not configured"**
→ Sprawdź czy `ResendApiKey` jest ustawiony

**Problem: "401 Unauthorized"**
→ Wygeneruj nowy API Key w Resend Dashboard

**Problem: Nie widzę emaili w skrzynce**
→ Użyj `delivered@resend.dev` do testów

## Potrzebujesz pomocy?

📖 Szczegółowa dokumentacja: [RESEND-SETUP.md](./RESEND-SETUP.md)
💬 Wsparcie Resend: https://resend.com/discord
