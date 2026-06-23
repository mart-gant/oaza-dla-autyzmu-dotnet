# 🔧 AWS Troubleshooting - Oaza dla Autyzmu

## ❌ NAJCZĘSTSZE PROBLEMY I ROZWIĄZANIA

---

## 1️⃣ "eb: command not found"

**Problem:** EB CLI nie jest zainstalowane

**Rozwiązanie:**
```bash
# Zainstaluj EB CLI:
pip install awsebcli --upgrade

# Lub przez pip3:
pip3 install awsebcli --upgrade --user

# Dodaj do PATH (macOS/Linux):
export PATH=$PATH:~/.local/bin

# Zweryfikuj:
eb --version
```

---

## 2️⃣ "AWS credentials not found"

**Problem:** Brak skonfigurowanych credentials AWS

**Rozwiązanie:**
```bash
# Skonfiguruj AWS CLI:
aws configure

# Wprowadź:
AWS Access Key ID: [YOUR_KEY]
AWS Secret Access Key: [YOUR_SECRET]
Default region name: eu-central-1
Default output format: json

# Sprawdź:
aws sts get-caller-identity
```

**Gdzie znaleźć keys?**
1. AWS Console → IAM → Users → [Twój user]
2. Security credentials → Create access key
3. Zapisz Access Key ID i Secret

---

## 3️⃣ "Database connection failed"

**Problem:** Aplikacja nie może połączyć się z RDS

**Możliwe przyczyny:**

### A) Firewall RDS (Security Group)
```bash
# Sprawdź Security Group:
aws rds describe-db-instances \
  --db-instance-identifier oaza-db \
  --query 'DBInstances[0].VpcSecurityGroups'

# Dodaj regułę dla EB:
# 1. AWS Console → RDS → oaza-db
# 2. Security → Security groups → Edit inbound rules
# 3. Dodaj: Type: PostgreSQL, Source: EB Security Group
```

### B) Błędny connection string
```bash
# Sprawdź endpoint RDS:
aws rds describe-db-instances \
  --db-instance-identifier oaza-db \
  --query 'DBInstances[0].Endpoint.Address' \
  --output text

# Connection string powinien być:
Host=YOUR-RDS-ENDPOINT.rds.amazonaws.com;Database=OazaDlaAutyzmu;Username=oazaadmin;Password=YOUR_PASSWORD;SSL Mode=Require;
```

### C) RDS nie jest dostępny
```bash
# Sprawdź status:
aws rds describe-db-instances \
  --db-instance-identifier oaza-db \
  --query 'DBInstances[0].DBInstanceStatus'

# Powinno być: "available"
```

---

## 4️⃣ "502 Bad Gateway"

**Problem:** Load Balancer nie może połączyć się z aplikacją

**Rozwiązania:**

### A) Sprawdź health check
```bash
eb health

# Jeśli unhealthy, sprawdź logi:
eb logs
```

### B) Port configuration
```bash
# Aplikacja musi słuchać na porcie 5000 (nie 80!)
# W appsettings.Production.json lub .ebextensions:
"Kestrel": {
  "EndPoints": {
    "Http": {
      "Url": "http://0.0.0.0:5000"
    }
  }
}
```

### C) Health check endpoint
```bash
# Dodaj w Program.cs (już jest!):
app.MapHealthChecks("/health");

# W .ebextensions/01-environment.config:
aws:elasticbeanstalk:application:
  Application Healthcheck URL: /health
```

---

## 5️⃣ "Deployment failed"

**Problem:** Deploy nie powiódł się

**Rozwiązania:**

### A) Sprawdź logi
```bash
eb logs --all
```

### B) Sprawdź format package
```bash
# Package musi być ZIP z zawartością publish/
cd publish
zip -r ../deploy.zip .
```

### C) Sprawdź .NET runtime
```bash
# Platform musi być .NET 8:
eb platform show

# Jeśli nie, zmień:
eb platform select "64bit Amazon Linux 2023 v3.2.0 running .NET 8"
```

---

## 6️⃣ "Application Error - 500"

**Problem:** Błąd aplikacji

**Rozwiązania:**

### A) Sprawdź logi aplikacji
```bash
eb logs --all | grep -i error
```

### B) Sprawdź zmienne środowiskowe
```bash
eb printenv

# Upewnij się że są:
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=...
```

### C) Sprawdź migracje bazy
```bash
# Czy uruchomiłeś migracje?
dotnet ef database update --connection "YOUR_CONNECTION_STRING"
```

---

## 7️⃣ "Cost unexpectedly high"

**Problem:** Koszty wyższe niż oczekiwane

**Rozwiązania:**

### A) Sprawdź aktywne usługi
```bash
# EC2 instances:
aws ec2 describe-instances --query 'Reservations[].Instances[].InstanceId'

# RDS instances:
aws rds describe-db-instances --query 'DBInstances[].DBInstanceIdentifier'

# Load Balancers:
aws elbv2 describe-load-balancers
```

### B) Usuń nieużywane resources
```bash
# Zakończ środowisko:
eb terminate oaza-production

# Usuń RDS:
aws rds delete-db-instance \
  --db-instance-identifier oaza-db \
  --skip-final-snapshot
```

### C) Zmień na tańsze typy
```bash
# Zamiast t3.small → t3.micro (Free Tier)
eb scale 1 --instance-type t3.micro

# Zamiast db.t3.micro → db.t2.micro (Free Tier)
```

---

## 8️⃣ "SMTP emails not sending"

**Problem:** Emaile nie są wysyłane

**Rozwiązania:**

### A) Gmail App Password
```bash
# Nie używaj normalnego hasła!
# 1. Google Account → Security
# 2. 2-Step Verification → ON
# 3. App passwords → Generate
# 4. Użyj wygenerowanego hasła (16 znaków)
```

### B) Sprawdź zmienne
```bash
eb printenv | grep Email

# Powinny być:
EmailSettings__SmtpServer=smtp.gmail.com
EmailSettings__SmtpPort=587
EmailSettings__SmtpUsername=your-email@gmail.com
EmailSettings__SmtpPassword=your-16-char-app-password
```

### C) AWS SES (alternatywa)
```bash
# AWS Simple Email Service - tańsze i bardziej reliable
# 1. AWS Console → SES
# 2. Verify domain
# 3. Zmień SMTP na SES endpoint
```

---

## 9️⃣ "SSL Certificate not working"

**Problem:** HTTPS nie działa

**Rozwiązania:**

### A) Request certificate
```bash
aws acm request-certificate \
  --domain-name yourdomain.com \
  --validation-method DNS \
  --region eu-central-1
```

### B) Validate domain
```bash
# Dodaj CNAME record w DNS (Route 53 lub zewnętrzny)
# AWS wyśle email z instrukcjami
```

### C) Przypisz do Load Balancera
```bash
eb config

# Dodaj sekcję:
# aws:elbv2:listener:443:
#   ListenerEnabled: true
#   Protocol: HTTPS
#   SSLCertificateArns: arn:aws:acm:...
```

---

## 🔟 "Can't SSH to instance"

**Problem:** `eb ssh` nie działa

**Rozwiązania:**

### A) Dodaj SSH key
```bash
# Generuj key:
ssh-keygen -t rsa -b 4096 -f ~/.ssh/eb-key

# Dodaj do EB:
eb init --keyname eb-key
```

### B) Security Group
```bash
# Sprawdź czy port 22 jest otwarty:
# AWS Console → EC2 → Security Groups
# Dodaj inbound rule: Type: SSH, Port: 22, Source: My IP
```

---

## 📊 MONITORING & DEBUGGING

### Sprawdź logi real-time
```bash
eb logs --stream
```

### Sprawdź health
```bash
eb health --refresh
```

### Sprawdź eventy
```bash
eb events
```

### CloudWatch logs
```bash
# AWS Console → CloudWatch → Log groups
# /aws/elasticbeanstalk/oaza-production/...
```

### SSH i debug
```bash
eb ssh
cd /var/app/current
sudo systemctl status web
sudo journalctl -u web -f
```

---

## 🆘 EMERGENCY - Rollback

**Jeśli deployment zepsuł aplikację:**

```bash
# Lista poprzednich wersji:
aws elasticbeanstalk describe-application-versions \
  --application-name oaza-dla-autyzmu

# Rollback do poprzedniej:
eb deploy --version previous-version-label
```

---

## 📞 POMOC

**Nie możesz rozwiązać problemu?**

1. **AWS Support Console** (jeśli masz plan support)
2. **AWS Forums**: https://forums.aws.amazon.com/
3. **Stack Overflow**: Tag `aws-elastic-beanstalk`
4. **GitHub Issues**: [link do repo]

**Wyślij diagnostics:**
```bash
eb health > health.txt
eb logs --all > logs.txt
eb status > status.txt

# Wyślij na: martgant@gmail.com
```

---

## ✅ PREVENTION CHECKLIST

Aby uniknąć problemów:

- [ ] Zawsze testuj lokalnie przed deployment
- [ ] Używaj `eb deploy` zamiast ręcznego uploadu
- [ ] Sprawdzaj health po każdym deployment (`eb health`)
- [ ] Regularnie czytaj logi (`eb logs`)
- [ ] Backup bazy danych (RDS automated backups)
- [ ] Monitoruj koszty (AWS Cost Explorer)
- [ ] Używaj Free Tier przez pierwsze 12 miesięcy
- [ ] Ustaw CloudWatch alarms (wysoki koszt, błędy)

---

**Powodzenia z AWS! 🚀**
