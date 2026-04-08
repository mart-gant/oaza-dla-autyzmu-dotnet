#!/bin/bash
# 🚀 Szybkie wdrożenie na Azure App Service

echo "🚀 Oaza dla Autyzmu - Azure Deployment Script"
echo "=============================================="

# Zmienne (ZMIEŃ TE WARTOŚCI!)
RESOURCE_GROUP="oaza-rg"
LOCATION="westeurope"
APP_NAME="oaza-dla-autyzmu-$(openssl rand -hex 4)"  # Unikalna nazwa
SQL_SERVER="oaza-sql-$(openssl rand -hex 4)"
SQL_ADMIN="oazaadmin"
SQL_PASSWORD="OazaAdmin123!@#"  # ZMIEŃ TO!
DB_NAME="OazaDlaAutyzmu"

# SMTP Configuration (UZUPEŁNIJ!)
SMTP_USERNAME="your-email@gmail.com"
SMTP_PASSWORD="your-app-password"

# reCAPTCHA (UZUPEŁNIJ!)
RECAPTCHA_SITE_KEY="your-site-key"
RECAPTCHA_SECRET_KEY="your-secret-key"

echo ""
echo "📝 Konfiguracja:"
echo "  Resource Group: $RESOURCE_GROUP"
echo "  App Name: $APP_NAME"
echo "  SQL Server: $SQL_SERVER"
echo "  Location: $LOCATION"
echo ""
read -p "Czy chcesz kontynuować? (y/n) " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]
then
    exit 1
fi

echo ""
echo "1️⃣ Tworzenie Resource Group..."
az group create --name $RESOURCE_GROUP --location $LOCATION

echo ""
echo "2️⃣ Tworzenie App Service Plan (B1)..."
az appservice plan create \
  --name ${APP_NAME}-plan \
  --resource-group $RESOURCE_GROUP \
  --sku B1 \
  --is-linux

echo ""
echo "3️⃣ Tworzenie Web App (.NET 10)..."
az webapp create \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan ${APP_NAME}-plan \
  --runtime "DOTNET:10.0"

echo ""
echo "4️⃣ Tworzenie SQL Server..."
az sql server create \
  --name $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user $SQL_ADMIN \
  --admin-password "$SQL_PASSWORD"

echo ""
echo "5️⃣ Konfiguracja firewall SQL Server (zezwól na Azure services)..."
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

echo ""
echo "6️⃣ Tworzenie bazy danych..."
az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name $DB_NAME \
  --service-objective Basic \
  --backup-storage-redundancy Local

# Connection String
CONNECTION_STRING="Server=tcp:${SQL_SERVER}.database.windows.net,1433;Initial Catalog=${DB_NAME};Persist Security Info=False;User ID=${SQL_ADMIN};Password=${SQL_PASSWORD};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

echo ""
echo "7️⃣ Konfiguracja Connection String..."
az webapp config connection-string set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --connection-string-type SQLServer \
  --settings DefaultConnection="$CONNECTION_STRING"

echo ""
echo "8️⃣ Konfiguracja zmiennych środowiskowych..."
az webapp config appsettings set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    "EmailSettings__SmtpServer=smtp.gmail.com" \
    "EmailSettings__SmtpPort=587" \
    "EmailSettings__SmtpUsername=$SMTP_USERNAME" \
    "EmailSettings__SmtpPassword=$SMTP_PASSWORD" \
    "EmailSettings__SenderEmail=noreply@oaza.pl" \
    "EmailSettings__SenderName=Oaza dla Autyzmu" \
    "RecaptchaSettings__SiteKey=$RECAPTCHA_SITE_KEY" \
    "RecaptchaSettings__SecretKey=$RECAPTCHA_SECRET_KEY"

echo ""
echo "9️⃣ Włączanie HTTPS only..."
az webapp update \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --https-only true

echo ""
echo "✅ GOTOWE!"
echo ""
echo "==================================================="
echo "📋 Szczegóły wdrożenia:"
echo "==================================================="
echo "🌐 URL aplikacji: https://${APP_NAME}.azurewebsites.net"
echo "🗄️  SQL Server: ${SQL_SERVER}.database.windows.net"
echo "📊 Database: ${DB_NAME}"
echo "👤 SQL User: ${SQL_ADMIN}"
echo "🔑 SQL Password: ${SQL_PASSWORD}"
echo ""
echo "==================================================="
echo "📝 Następne kroki:"
echo "==================================================="
echo "1. Uruchom migracje bazy danych:"
echo "   dotnet ef database update --connection \"$CONNECTION_STRING\""
echo ""
echo "2. Deploy aplikacji z Visual Studio:"
echo "   - Kliknij prawym na projekt"
echo "   - Publish → Azure → Wybierz $APP_NAME"
echo ""
echo "3. Zaloguj się do aplikacji:"
echo "   - URL: https://${APP_NAME}.azurewebsites.net"
echo "   - Admin: admin@oaza.pl / Admin123!"
echo "   - ⚠️  ZMIEŃ HASŁO ADMINISTRATORA!"
echo ""
echo "==================================================="

# Zapisz dane do pliku
cat > deployment-info.txt << EOF
Azure Deployment - Oaza dla Autyzmu
====================================
Utworzono: $(date)

URL: https://${APP_NAME}.azurewebsites.net
Resource Group: $RESOURCE_GROUP

SQL Server: ${SQL_SERVER}.database.windows.net
Database: ${DB_NAME}
SQL User: ${SQL_ADMIN}
SQL Password: ${SQL_PASSWORD}

Connection String:
$CONNECTION_STRING

Następne kroki:
1. Uruchom migracje: dotnet ef database update
2. Deploy z Visual Studio
3. Zmień hasło administratora!
EOF

echo "💾 Dane zapisane w: deployment-info.txt"
echo "⚠️  UWAGA: Przechowuj ten plik BEZPIECZNIE i USUŃ GO po zapisaniu danych!"
