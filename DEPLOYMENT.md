# 🚀 Przewodnik Wdrożenia - Oaza dla Autyzmu

## 📋 Spis treści
1. [Wymagania](#wymagania)
2. [Azure App Service (zalecane)](#azure-app-service)
3. [Docker + Azure Container Instances](#docker--azure-container-instances)
4. [AWS Elastic Beanstalk](#aws-elastic-beanstalk)
5. [Własny serwer VPS](#własny-serwer-vps)
6. [Konfiguracja po wdrożeniu](#konfiguracja-po-wdrożeniu)

---

## Wymagania

### Przed wdrożeniem musisz mieć:
1. ✅ Konto w chmurze (Azure, AWS, lub dostawca VPS)
2. ✅ Bazę danych (PostgreSQL lub SQL Server - **NIE SQLite**)
3. ✅ SMTP dla emaili (Gmail, SendGrid, Mailgun)
4. ✅ Google reCAPTCHA keys (bezpłatne)
5. ✅ (Opcjonalnie) Sentry.io dla monitoringu błędów

---

## 1️⃣ Azure App Service (ZALECANE - najprostsze)

### Krok 1: Utwórz App Service
```bash
# Zaloguj się do Azure
az login

# Utwórz Resource Group
az group create --name oaza-rg --location westeurope

# Utwórz App Service Plan (B1 = ~13 USD/miesiąc)
az appservice plan create \
  --name oaza-plan \
  --resource-group oaza-rg \
  --sku B1 \
  --is-linux

# Utwórz Web App
az webapp create \
  --name oaza-dla-autyzmu \
  --resource-group oaza-rg \
  --plan oaza-plan \
  --runtime "DOTNETCORE:10.0"
```

### Krok 2: Utwórz bazę danych Azure SQL
```bash
# Utwórz SQL Server
az sql server create \
  --name oaza-sql-server \
  --resource-group oaza-rg \
  --location westeurope \
  --admin-user oazaadmin \
  --admin-password 'YourStrongPassword123!'

# Utwórz bazę danych
az sql db create \
  --resource-group oaza-rg \
  --server oaza-sql-server \
  --name OazaDlaAutyzmu \
  --service-objective Basic
```

### Krok 3: Skonfiguruj Connection String
```bash
az webapp config connection-string set \
  --name oaza-dla-autyzmu \
  --resource-group oaza-rg \
  --connection-string-type SQLServer \
  --settings DefaultConnection='Server=tcp:oaza-sql-server.database.windows.net,1433;Database=OazaDlaAutyzmu;User ID=oazaadmin;Password=YourStrongPassword123!;Encrypt=True;'
```

### Krok 4: Ustaw zmienne środowiskowe
```bash
az webapp config appsettings set \
  --name oaza-dla-autyzmu \
  --resource-group oaza-rg \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    EmailSettings__SmtpServer=smtp.gmail.com \
    EmailSettings__SmtpPort=587 \
    EmailSettings__SmtpUsername=your-email@gmail.com \
    EmailSettings__SmtpPassword=your-app-password \
    RecaptchaSettings__SiteKey=your-site-key \
    RecaptchaSettings__SecretKey=your-secret-key
```

### Krok 5: Deploy z Visual Studio
1. W Visual Studio, kliknij prawym na projekt `OazaDlaAutyzmu.Web`
2. Wybierz **Publish** → **Azure** → **Azure App Service (Linux)**
3. Zaloguj się i wybierz swoją App Service
4. Kliknij **Publish**

**LUB** użyj GitHub Actions (automatyczny deployment):

```yaml
# .github/workflows/azure-deploy.yml
name: Deploy to Azure

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '10.0.x'
    
    - name: Restore
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release
    
    - name: Publish
      run: dotnet publish src/OazaDlaAutyzmu.Web/OazaDlaAutyzmu.Web.csproj -c Release -o ./publish
    
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'oaza-dla-autyzmu'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

### Krok 6: Uruchom migracje bazy danych
```bash
# Lokalnie, z connection stringiem do Azure SQL:
dotnet ef database update --project src/OazaDlaAutyzmu.Infrastructure --startup-project src/OazaDlaAutyzmu.Web
```

---

## 2️⃣ Docker + Azure Container Instances

### Krok 1: Zbuduj i wypchnij obraz Docker
```bash
# Zaloguj się do Azure Container Registry
az acr create --name oazaregistry --resource-group oaza-rg --sku Basic
az acr login --name oazaregistry

# Zbuduj obraz
docker build -t oazaregistry.azurecr.io/oaza-web:latest .

# Wypchnij do registry
docker push oazaregistry.azurecr.io/oaza-web:latest
```

### Krok 2: Uruchom kontener
```bash
az container create \
  --name oaza-container \
  --resource-group oaza-rg \
  --image oazaregistry.azurecr.io/oaza-web:latest \
  --cpu 1 \
  --memory 1.5 \
  --dns-name-label oaza-dla-autyzmu \
  --ports 80 443 \
  --environment-variables \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__DefaultConnection='YOUR_CONNECTION_STRING'
```

---

## 3️⃣ AWS Elastic Beanstalk

### Krok 1: Zainstaluj AWS CLI i EB CLI
```bash
pip install awsebcli
```

### Krok 2: Inicjalizuj projekt
```bash
cd src/OazaDlaAutyzmu.Web
eb init -p "64bit Amazon Linux 2023 v3.0.0 running .NET 8" oaza-app --region eu-central-1
```

### Krok 3: Utwórz środowisko i deploy
```bash
eb create oaza-production
eb deploy
```

### Krok 4: Ustaw zmienne środowiskowe
```bash
eb setenv \
  ASPNETCORE_ENVIRONMENT=Production \
  ConnectionStrings__DefaultConnection='YOUR_RDS_CONNECTION_STRING' \
  EmailSettings__SmtpServer=smtp.gmail.com
```

---

## 4️⃣ Własny Serwer VPS (DigitalOcean, Linode, Hetzner)

### Krok 1: Zainstaluj Docker na serwerze
```bash
# SSH do serwera
ssh root@your-server-ip

# Zainstaluj Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sh get-docker.sh

# Zainstaluj Docker Compose
apt install docker-compose-plugin
```

### Krok 2: Sklonuj projekt
```bash
git clone https://github.com/mart-gant/oaza-dla-autyzmu-dotnet.git
cd oaza-dla-autyzmu-dotnet
```

### Krok 3: Skonfiguruj zmienne środowiskowe
```bash
# Stwórz plik .env
cat > .env << EOF
DB_PASSWORD=your_secure_password_here
SMTP_PASSWORD=your_smtp_password
RECAPTCHA_SITE_KEY=your_site_key
RECAPTCHA_SECRET_KEY=your_secret_key
EOF
```

### Krok 4: Uruchom z Docker Compose
```bash
docker compose up -d
```

### Krok 5: Skonfiguruj Nginx + Certbot (HTTPS)
```bash
# Zainstaluj Certbot
apt install certbot python3-certbot-nginx

# Uzyskaj certyfikat SSL
certbot --nginx -d yourdomain.com -d www.yourdomain.com
```

---

## Konfiguracja po wdrożeniu

### 1. Uruchom migracje bazy danych
```bash
# Połącz się z bazą przez SSH lub Azure portal
dotnet ef database update
```

### 2. Utwórz konto administratora
Zaloguj się jako `admin@oaza.pl` / `Admin123!` i natychmiast zmień hasło!

### 3. Skonfiguruj monitoring
- **Sentry**: https://sentry.io (monitoring błędów)
- **Application Insights** (Azure)
- **CloudWatch** (AWS)

### 4. Ustaw backupy bazy danych
- **Azure SQL**: Automatyczne backupy włączone
- **PostgreSQL**: Cron job z `pg_dump`

### 5. Sprawdź bezpieczeństwo
- [ ] HTTPS włączony
- [ ] reCAPTCHA skonfigurowany
- [ ] Rate limiting działa
- [ ] Secrets w zmiennych środowiskowych (NIE w kodzie!)

---

## 📊 Koszty miesięczne (szacunkowo)

| Platforma | Plan | Koszt/miesiąc |
|-----------|------|---------------|
| **Azure App Service** | B1 + Basic SQL | ~30 USD |
| **AWS Elastic Beanstalk** | t3.small + RDS | ~25 USD |
| **DigitalOcean VPS** | 2GB Droplet | ~12 USD |
| **Hetzner VPS** | CX21 | ~5 EUR |

---

## ❓ Pomoc

**Potrzebujesz pomocy?**
- Discord: [link]
- Email: martgant@gmail.com
- Issues: https://github.com/mart-gant/oaza-dla-autyzmu-dotnet/issues

---

**Powodzenia z wdrożeniem! 🚀**
