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

## 3️⃣ AWS Elastic Beanstalk (AUTOMATYCZNY SKRYPT)

### ⚡ SZYBKIE WDROŻENIE (15 minut)

**Krok 1: Przygotuj konto AWS**
```bash
# Utwórz konto: https://aws.amazon.com/free/
# Zainstaluj AWS CLI: https://aws.amazon.com/cli/
# Skonfiguruj credentials:
aws configure
# Wprowadź: AWS Access Key ID, Secret Access Key, Region: eu-central-1
```

**Krok 2: Zainstaluj EB CLI**
```bash
# Windows (PowerShell):
pip install awsebcli

# macOS/Linux:
pip3 install awsebcli --upgrade --user

# Weryfikacja:
eb --version
```

**Krok 3: Skonfiguruj credentials w deploy-aws.sh**
```bash
# Edytuj deploy-aws.sh:
SMTP_USERNAME="your-email@gmail.com"
SMTP_PASSWORD="your-gmail-app-password"
RECAPTCHA_SITE_KEY="your-recaptcha-site-key"
RECAPTCHA_SECRET_KEY="your-recaptcha-secret"
DB_PASSWORD="YourStrongPassword123!"  # Zmień!
```

**Krok 4: Uruchom automatyczny deployment**
```bash
chmod +x deploy-aws.sh
./deploy-aws.sh
```

✅ **To wszystko! Aplikacja będzie online za 10-15 minut!**

---

### 📋 CO ROBI SKRYPT `deploy-aws.sh`?

1. ✅ Tworzy RDS PostgreSQL (db.t3.micro)
2. ✅ Tworzy Elastic Beanstalk environment (t3.small)
3. ✅ Konfiguruje Load Balancer
4. ✅ Ustawia zmienne środowiskowe (SMTP, reCAPTCHA, DB)
5. ✅ Deploy aplikacji .NET
6. ✅ Zapisuje dane logowania w `aws-deployment-info.txt`

---

### 🔧 RĘCZNE WDROŻENIE (krok po kroku)

**Krok 1: Inicjalizacja projektu**
```bash
cd src/OazaDlaAutyzmu.Web
eb init oaza-dla-autyzmu \
  --platform "64bit Amazon Linux 2023 v3.2.0 running .NET 8" \
  --region eu-central-1
```

**Krok 2: Utwórz RDS PostgreSQL**
```bash
# Przez AWS Console:
# 1. Idź do RDS → Create database
# 2. Engine: PostgreSQL 16
# 3. Template: Free tier (db.t3.micro)
# 4. DB instance identifier: oaza-db
# 5. Master username: oazaadmin
# 6. Master password: [SILNE_HASŁO]
# 7. Public access: Yes
# 8. Database name: OazaDlaAutyzmu

# LUB przez CLI:
aws rds create-db-instance \
  --db-instance-identifier oaza-db \
  --db-instance-class db.t3.micro \
  --engine postgres \
  --master-username oazaadmin \
  --master-user-password 'YourPassword123!' \
  --allocated-storage 20 \
  --publicly-accessible \
  --region eu-central-1
```

**Krok 3: Utwórz środowisko Elastic Beanstalk**
```bash
eb create oaza-production \
  --instance-type t3.small \
  --region eu-central-1
```

**Krok 4: Pobierz endpoint RDS**
```bash
aws rds describe-db-instances \
  --db-instance-identifier oaza-db \
  --query 'DBInstances[0].Endpoint.Address' \
  --output text
```

**Krok 5: Ustaw zmienne środowiskowe**
```bash
eb setenv \
  ASPNETCORE_ENVIRONMENT=Production \
  ConnectionStrings__DefaultConnection='Host=YOUR_RDS_ENDPOINT.rds.amazonaws.com;Database=OazaDlaAutyzmu;Username=oazaadmin;Password=YourPassword123!;SSL Mode=Require;' \
  EmailSettings__SmtpServer=smtp.gmail.com \
  EmailSettings__SmtpPort=587 \
  EmailSettings__SmtpUsername=your-email@gmail.com \
  EmailSettings__SmtpPassword=your-app-password \
  RecaptchaSettings__SiteKey=your-site-key \
  RecaptchaSettings__SecretKey=your-secret-key
```

**Krok 6: Deploy aplikacji**
```bash
# Z Visual Studio:
dotnet publish -c Release -o ./publish

# Spakuj:
cd publish
zip -r ../deploy.zip .

# Deploy:
eb deploy
```

**Krok 7: Uruchom migracje**
```bash
dotnet ef database update \
  --connection "Host=YOUR_RDS_ENDPOINT;Database=OazaDlaAutyzmu;Username=oazaadmin;Password=YourPassword123!;SSL Mode=Require;"
```

---

### 🔐 DODAJ HTTPS (SSL Certificate)

**Krok 1: Request certificate w AWS Certificate Manager**
```bash
# Przez AWS Console:
# 1. Certificate Manager → Request certificate
# 2. Domain: yourdomain.com, www.yourdomain.com
# 3. Validation: DNS (dodaj CNAME w domenowym DNS)
# 4. Wait for validation

# LUB przez CLI:
aws acm request-certificate \
  --domain-name yourdomain.com \
  --subject-alternative-names www.yourdomain.com \
  --validation-method DNS \
  --region eu-central-1
```

**Krok 2: Przypisz certificate do Load Balancera**
```bash
# Przez EB CLI:
eb config

# Dodaj w sekcji aws:elbv2:listener:443:
# ListenerEnabled: true
# Protocol: HTTPS
# SSLCertificateArns: arn:aws:acm:eu-central-1:ACCOUNT_ID:certificate/CERT_ID
```

---

### 🌐 WŁASNA DOMENA (Route 53)

**Krok 1: Kup domenę**
```bash
# Przez AWS Route 53 lub zewnętrznego providera (np. Cloudflare, Namecheap)
```

**Krok 2: Utwórz Hosted Zone**
```bash
aws route53 create-hosted-zone \
  --name yourdomain.com \
  --caller-reference $(date +%s)
```

**Krok 3: Dodaj CNAME do Elastic Beanstalk**
```bash
# Pobierz CNAME EB:
eb status

# Dodaj CNAME record w Route 53:
# yourdomain.com → CNAME → oaza-production.eu-central-1.elasticbeanstalk.com
```

---

### 📊 KOSZTY AWS (miesięcznie)

| Usługa | Konfiguracja | Koszt |
|--------|--------------|-------|
| **Elastic Beanstalk (EC2)** | t3.small | ~$15 |
| **RDS PostgreSQL** | db.t3.micro | ~$13 |
| **Application Load Balancer** | - | ~$16 |
| **Data Transfer** | ~10GB | ~$1 |
| **S3 (backupy)** | ~5GB | ~$0.12 |
| **RAZEM** | | **~$45 USD/miesiąc** |

**Free Tier (pierwsze 12 miesięcy):**
- 750 godzin EC2 t2.micro (FREE)
- 750 godzin RDS db.t2.micro (FREE)
- **Koszt przez 12 miesięcy: ~$0-5 USD/miesiąc!**

---

### 🛠️ ZARZĄDZANIE APLIKACJĄ

```bash
# Status aplikacji
eb status

# Logi
eb logs

# SSH do instancji
eb ssh

# Deploy nowej wersji
eb deploy

# Restart aplikacji
eb restart

# Skalowanie (auto-scaling)
eb scale 3  # 3 instancje

# Monitoring
eb health

# Otwórz w przeglądarce
eb open

# Usuń środowisko (UWAGA!)
eb terminate oaza-production
```

---

### 🔄 CI/CD - GitHub Actions

**Krok 1: Dodaj secrets w GitHub**
```
Settings → Secrets → Actions:
- AWS_ACCESS_KEY_ID
- AWS_SECRET_ACCESS_KEY
- AWS_ACCOUNT_ID
```

**Krok 2: Workflow już gotowy!**
```
.github/workflows/aws-deploy.yml
```

**Krok 3: Push do main = automatyczny deployment!**
```bash
git push origin main
# GitHub Actions automatycznie deploy na AWS!
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
