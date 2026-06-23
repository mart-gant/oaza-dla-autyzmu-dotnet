#!/bin/bash
# 🟧 AWS Elastic Beanstalk Deployment Script
# Oaza dla Autyzmu - Quick AWS Setup

echo "🟧 AWS Elastic Beanstalk - Oaza dla Autyzmu"
echo "============================================"

# Zmienne (ZMIEŃ TE WARTOŚCI!)
APP_NAME="oaza-dla-autyzmu"
ENV_NAME="oaza-production"
REGION="eu-central-1"  # Frankfurt (bliżej Polski)
PLATFORM="64bit Amazon Linux 2023 v3.2.0 running .NET 8"

# Database
DB_INSTANCE="oaza-db"
DB_NAME="OazaDlaAutyzmu"
DB_USERNAME="oazaadmin"
DB_PASSWORD="OazaDB123!@#"  # ZMIEŃ TO!

# SMTP (UZUPEŁNIJ!)
SMTP_USERNAME="your-email@gmail.com"
SMTP_PASSWORD="your-app-password"

# reCAPTCHA (UZUPEŁNIJ!)
RECAPTCHA_SITE_KEY="your-site-key"
RECAPTCHA_SECRET_KEY="your-secret-key"

echo ""
echo "📝 Konfiguracja:"
echo "  App Name: $APP_NAME"
echo "  Environment: $ENV_NAME"
echo "  Region: $REGION"
echo "  DB Instance: $DB_INSTANCE"
echo ""
read -p "Czy chcesz kontynuować? (y/n) " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]
then
    exit 1
fi

# Sprawdź czy AWS CLI i EB CLI są zainstalowane
if ! command -v aws &> /dev/null; then
    echo "❌ AWS CLI nie jest zainstalowane!"
    echo "Instalacja: https://aws.amazon.com/cli/"
    exit 1
fi

if ! command -v eb &> /dev/null; then
    echo "❌ EB CLI nie jest zainstalowane!"
    echo "Instalacja: pip install awsebcli"
    exit 1
fi

echo ""
echo "1️⃣ Inicjalizacja Elastic Beanstalk..."
cd src/OazaDlaAutyzmu.Web
eb init $APP_NAME --region $REGION --platform "$PLATFORM"

echo ""
echo "2️⃣ Tworzenie RDS PostgreSQL..."
aws rds create-db-instance \
    --db-instance-identifier $DB_INSTANCE \
    --db-instance-class db.t3.micro \
    --engine postgres \
    --engine-version 16.1 \
    --master-username $DB_USERNAME \
    --master-user-password "$DB_PASSWORD" \
    --allocated-storage 20 \
    --backup-retention-period 7 \
    --vpc-security-group-ids default \
    --publicly-accessible \
    --region $REGION

echo "⏳ Czekam na utworzenie RDS (to może potrwać 5-10 minut)..."
aws rds wait db-instance-available \
    --db-instance-identifier $DB_INSTANCE \
    --region $REGION

# Pobierz endpoint RDS
DB_ENDPOINT=$(aws rds describe-db-instances \
    --db-instance-identifier $DB_INSTANCE \
    --region $REGION \
    --query 'DBInstances[0].Endpoint.Address' \
    --output text)

echo "✅ RDS endpoint: $DB_ENDPOINT"

# Connection string
CONNECTION_STRING="Host=${DB_ENDPOINT};Database=${DB_NAME};Username=${DB_USERNAME};Password=${DB_PASSWORD};SSL Mode=Require;"

echo ""
echo "3️⃣ Tworzenie środowiska Elastic Beanstalk..."
eb create $ENV_NAME \
    --instance-type t3.small \
    --platform "$PLATFORM" \
    --region $REGION \
    --database.username $DB_USERNAME \
    --database.password "$DB_PASSWORD" \
    --envvars \
        ASPNETCORE_ENVIRONMENT=Production,\
        ConnectionStrings__DefaultConnection="$CONNECTION_STRING",\
        EmailSettings__SmtpServer=smtp.gmail.com,\
        EmailSettings__SmtpPort=587,\
        EmailSettings__SmtpUsername=$SMTP_USERNAME,\
        EmailSettings__SmtpPassword=$SMTP_PASSWORD,\
        EmailSettings__SenderEmail=noreply@oaza.pl,\
        EmailSettings__SenderName="Oaza dla Autyzmu",\
        RecaptchaSettings__SiteKey=$RECAPTCHA_SITE_KEY,\
        RecaptchaSettings__SecretKey=$RECAPTCHA_SECRET_KEY

echo ""
echo "4️⃣ Konfiguracja HTTPS (Load Balancer)..."
eb setenv HTTPS_ENABLED=true

echo ""
echo "5️⃣ Deploy aplikacji..."
eb deploy

echo ""
echo "✅ WDROŻENIE ZAKOŃCZONE!"
echo ""
echo "==================================================="
echo "📋 Szczegóły wdrożenia AWS:"
echo "==================================================="
echo "🌐 URL aplikacji: http://${ENV_NAME}.${REGION}.elasticbeanstalk.com"
echo "🗄️  RDS Endpoint: ${DB_ENDPOINT}"
echo "📊 Database: ${DB_NAME}"
echo "👤 DB User: ${DB_USERNAME}"
echo "🔑 DB Password: ${DB_PASSWORD}"
echo ""
echo "Connection String:"
echo "$CONNECTION_STRING"
echo ""
echo "==================================================="
echo "📝 Następne kroki:"
echo "==================================================="
echo "1. Uruchom migracje bazy danych:"
echo "   dotnet ef database update --connection \"$CONNECTION_STRING\""
echo ""
echo "2. Skonfiguruj własną domenę:"
echo "   - Kup domenę w Route 53"
echo "   - Dodaj CNAME record"
echo ""
echo "3. Dodaj HTTPS (SSL):"
echo "   - Request certificate w AWS Certificate Manager"
echo "   - Przypisz do Load Balancera"
echo ""
echo "4. Zaloguj się:"
echo "   - Admin: admin@oaza.pl / Admin123!"
echo "   - ⚠️  ZMIEŃ HASŁO!"
echo ""
echo "==================================================="
echo "📊 Koszty szacunkowe (miesięcznie):"
echo "==================================================="
echo "- Elastic Beanstalk (t3.small): ~$15"
echo "- RDS PostgreSQL (db.t3.micro): ~$13"
echo "- Data Transfer: ~$2-5"
echo "- RAZEM: ~$30-33 USD/miesiąc"
echo ""
echo "==================================================="

# Zapisz dane
cat > aws-deployment-info.txt << EOF
AWS Deployment - Oaza dla Autyzmu
==================================
Data: $(date)

URL: http://${ENV_NAME}.${REGION}.elasticbeanstalk.com
Region: ${REGION}
Environment: ${ENV_NAME}

RDS PostgreSQL:
- Endpoint: ${DB_ENDPOINT}
- Database: ${DB_NAME}
- Username: ${DB_USERNAME}
- Password: ${DB_PASSWORD}

Connection String:
$CONNECTION_STRING

AWS Console:
https://${REGION}.console.aws.amazon.com/elasticbeanstalk/home?region=${REGION}#/environment/dashboard?environmentId=${ENV_NAME}

Komendy zarządzania:
- eb status          # Status aplikacji
- eb logs            # Pobierz logi
- eb deploy          # Deploy nowej wersji
- eb ssh             # SSH do instancji
- eb terminate       # Usuń środowisko (UWAGA!)

Następne kroki:
1. Uruchom migracje: dotnet ef database update
2. Skonfiguruj domenę w Route 53
3. Dodaj SSL certificate
4. Zmień hasło administratora!
EOF

echo "💾 Dane zapisane w: aws-deployment-info.txt"
echo "⚠️  BEZPIECZNIE przechowuj ten plik!"
echo ""
echo "🎉 Deployment zakończony! Sprawdź aplikację:"
echo "   http://${ENV_NAME}.${REGION}.elasticbeanstalk.com"
