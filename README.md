# Task Management System

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-21-DD0031?logo=angular&logoColor=white)](https://angular.dev/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-supported-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Oracle](https://img.shields.io/badge/Oracle-provider%20support-F80000?logo=oracle&logoColor=white)](https://www.oracle.com/database/)
[![Angular Material](https://img.shields.io/badge/Angular%20Material-UI-757575?logo=angular&logoColor=white)](https://material.angular.dev/)

Task Management System; görev, kategori, yorum ve dosya süreçlerini tek bir arayüzde yönetmek için geliştirilmiş, JWT tabanlı kimlik doğrulama ve rol bazlı yetkilendirme kullanan full-stack bir görev yönetimi uygulamasıdır. ASP.NET Core Web API ve Angular üzerine kurulan proje; responsive arayüz, Dark/Light tema, Kanban görünümü ve PostgreSQL/Oracle provider ayrımıyla hem geliştirme hem de production senaryolarını destekler.

## İçindekiler

- [Temel özellikler](#temel-özellikler)
- [Kullanılan teknolojiler](#kullanılan-teknolojiler)
- [Proje klasör yapısı](#proje-klasör-yapısı)
- [Gereksinimler](#gereksinimler)
- [Kurulum](#kurulum)
- [Backend yapılandırması](#backend-yapılandırması)
- [Frontend yapılandırması](#frontend-yapılandırması)
- [Veritabanı kurulumu](#veritabanı-kurulumu)
- [Migration işlemleri](#migration-işlemleri)
- [User Secrets kullanımı](#user-secrets-kullanımı)
- [Environment Variables](#environment-variables)
- [Development ortamında çalıştırma](#development-ortamında-çalıştırma)
- [Production build ve publish](#production-build-ve-publish)
- [deploy.ps1 kullanımı](#deployps1-kullanımı)
- [Health Check](#health-check)
- [Swagger / API dokümantasyonu](#swagger--api-dokümantasyonu)
- [Güvenlik notları](#güvenlik-notları)
- [Ekran görüntüleri](#ekran-görüntüleri)
- [Gelecek geliştirmeler](#gelecek-geliştirmeler)
- [Katkıda bulunma](#katkıda-bulunma)
- [Lisans](#lisans)

## Temel özellikler

- Kullanıcı kayıt, giriş, profil güncelleme ve parola değiştirme işlemleri
- JWT Bearer authentication
- `User` ve `Admin` rolleriyle role-based authorization
- Kullanıcıların yalnız kendi görev, kategori, yorum ve dosyalarına erişmesini sağlayan veri izolasyonu
- Admin kullanıcı listeleme, rol değiştirme ve hesap aktif/pasif yönetimi
- Görev oluşturma, görüntüleme, güncelleme ve silme
- Öncelik, durum, kategori, tarih ve metin tabanlı filtreleme
- Arama, sıralama ve server-side pagination
- Dashboard üzerinde görev istatistikleri ve geciken görevler
- Angular CDK Drag & Drop ile Kanban durum güncelleme
- Kategori CRUD işlemleri ve renk yönetimi
- Görev detayında yorum oluşturma, düzenleme ve silme
- Göreve dosya yükleme, indirme ve silme
- Responsive mobil/masaüstü arayüz
- Kalıcı Dark/Light tema tercihi
- PostgreSQL ve Oracle için birbirinden ayrılmış EF Core migration zincirleri
- Serilog ile console ve günlük rolling file logları
- Health Check endpoint’i
- Development ortamında Swagger/OpenAPI arayüzü
- Production ortamında HSTS ve environment tabanlı yapılandırma

## Kullanılan teknolojiler

| Katman | Teknolojiler |
| --- | --- |
| Backend | ASP.NET Core Web API, .NET 9, C# |
| Veri erişimi | Entity Framework Core 8, AutoMapper |
| Veritabanı | PostgreSQL, Oracle Entity Framework Core provider |
| Güvenlik | JWT Bearer, BCrypt, role-based authorization |
| Loglama ve izleme | Serilog, Health Checks |
| Frontend | Angular 21, TypeScript 5.9, RxJS |
| UI | Angular Material, Angular CDK, SCSS |
| Araçlar | .NET CLI, EF Core CLI, Angular CLI, npm, PowerShell |

## Proje klasör yapısı

```text
TaskManagementSystem/
├── Backend/
│   ├── TaskManagement.API/                    # Web API, controller, servis ve yapılandırmalar
│   ├── TaskManagement.Data/                   # Ortak ApplicationDbContext ve entity modeli
│   ├── Migrations.PostgreSql/                 # PostgreSQL migration assembly'si
│   ├── TaskManagement.API.Migrations.Oracle/  # Oracle migration assembly'si
│   ├── TaskManagement.SecurityTests/          # Yetkilendirme güvenlik senaryoları
│   ├── TaskManagement.ProviderTests/          # Provider migration/API doğrulamaları
│   └── MIGRATIONS.md                          # Ayrıntılı migration komutları
├── Frontend/
│   └── TaskManagement.Web/                    # Angular uygulaması
├── Database/
│   └── Scripts/                               # Veritabanı scriptleri için ayrılmış alan
├── docs/
│   └── screenshots/                           # README ekran görüntüleri
├── deploy.ps1                                 # Backend Release publish scripti
└── README.md
```

`ApplicationDbContext` ortak `TaskManagement.Data` assembly’sinde derlenir. PostgreSQL ve Oracle migration projeleri aynı modeli kullanır ancak farklı migration assembly’leri ve farklı `__EFMigrationsHistory` zincirleriyle çalışır.

## Gereksinimler

- Windows PowerShell 5.1 veya PowerShell 7+
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- EF Core CLI 8.x
- Node.js `^20.19.0`, `^22.12.0` veya `>=24.0.0`
- npm 8 veya üzeri; projedeki package manager sürümü `npm 10.9.2`
- PostgreSQL veya Oracle Database
- Git

Kurulu sürümleri kontrol etmek için:

```powershell
dotnet --version
dotnet ef --version
node --version
npm --version
```

EF Core CLI kurulu değilse proje paketleriyle uyumlu sürümü yükleyin:

```powershell
dotnet tool install --global dotnet-ef --version 8.0.8
```

## Kurulum

### 1. Repoyu klonlayın

```powershell
git clone <REPOSITORY_URL>
Set-Location .\TaskManagementSystem
```

### 2. Backend bağımlılıklarını yükleyin

```powershell
dotnet restore .\Backend\TaskManagement.API\TaskManagement.API.csproj
```

Bu komut ortak data projesini ve iki provider migration projesini de restore eder.

### 3. Frontend bağımlılıklarını yükleyin

```powershell
Set-Location .\Frontend\TaskManagement.Web
npm ci
Set-Location ..\..
```

### 4. Secret ve veritabanı ayarlarını yapılandırın

Geliştirme ortamında hassas değerleri `appsettings.json` içine yazmak yerine [User Secrets](#user-secrets-kullanımı) veya [Environment Variables](#environment-variables) kullanın.

### 5. Seçilen provider’ın migration’larını uygulayın

PostgreSQL veya Oracle için [Migration işlemleri](#migration-işlemleri) bölümündeki ilgili komutu çalıştırın.

## Backend yapılandırması

Backend ana yapılandırmaları aşağıdaki dosyalardadır:

- `Backend/TaskManagement.API/appsettings.json`
- `Backend/TaskManagement.API/appsettings.Development.json`
- `Backend/TaskManagement.API/appsettings.Production.json`

Hassas değer içermeyen örnek yapı:

```json
{
  "DatabaseProvider": "PostgreSql",
  "ConnectionStrings": {
    "PostgreSql": "Host=<HOST>;Port=<PORT>;Database=<DATABASE>;Username=<USERNAME>;Password=<PASSWORD>",
    "Oracle": "User Id=<USERNAME>;Password=<PASSWORD>;Data Source=<HOST>:<PORT>/<SERVICE_NAME>"
  },
  "Jwt": {
    "Key": "<STRONG_RANDOM_SECRET>",
    "Issuer": "<JWT_ISSUER>",
    "Audience": "<JWT_AUDIENCE>",
    "ExpireMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200"
    ]
  }
}
```

`DatabaseProvider` yalnızca şu değerlerden biri olmalıdır:

- `PostgreSql`
- `Oracle`

API seçilen provider’a göre doğru EF Core provider’ını ve migration assembly’sini yükler.

Serilog varsayılan olarak console’a ve `logs/log-.txt` günlük rolling dosyalarına yazar. Dosya logları 14 gün tutulacak şekilde yapılandırılmıştır. Production’da log dizini için yazma izni ve kalıcı disk alanı sağlanmalıdır.

## Frontend yapılandırması

Frontend API adresleri environment dosyalarında tanımlıdır:

- Development: `Frontend/TaskManagement.Web/src/environments/environment.ts`
- Production: `Frontend/TaskManagement.Web/src/environments/environment.prod.ts`

Development varsayılanı:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5266/api'
};
```

Production build öncesinde `environment.prod.ts` içindeki `apiUrl` değerini yayınlanan API adresine göre güncelleyin:

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://<API_HOST>/api'
};
```

API tarafındaki `Cors:AllowedOrigins` listesine frontend origin’inin birebir eklenmesi gerekir.

## Veritabanı kurulumu

### PostgreSQL

1. PostgreSQL servisinin çalıştığından emin olun.
2. Uygulama için boş bir veritabanı ve yetkili bir kullanıcı oluşturun.
3. Connection string’i User Secrets veya Environment Variable olarak tanımlayın.
4. PostgreSQL migration zincirini uygulayın.

Örnek veritabanı oluşturma komutu:

```powershell
psql -h <HOST> -U <ADMIN_USER> -c "CREATE DATABASE task_management_db;"
```

### Oracle

1. Oracle servisinin ve hedef PDB/service’in erişilebilir olduğundan emin olun.
2. Uygulama için ayrı bir schema/user oluşturun.
3. Kullanıcıya migration’ların ihtiyaç duyduğu tablo, index, sequence ve constraint oluşturma yetkilerini verin.
4. Oracle connection string’ini secret olarak tanımlayın.
5. Oracle migration zincirini uygulayın.

Oracle user/schema oluşturma işlemleri DBA yetkisi gerektirebilir. Production yetkilerini least-privilege yaklaşımıyla sınırlandırın.

## Migration işlemleri

Provider migration’ları birbirinden tamamen ayrıdır. PostgreSQL aktifken Oracle migration’ları; Oracle aktifken PostgreSQL migration’ları keşfedilmez.

### PostgreSQL migration update

```powershell
dotnet ef database update `
  --project .\Backend\Migrations.PostgreSql\TaskManagement.API.Migrations.PostgreSql.csproj `
  --startup-project .\Backend\Migrations.PostgreSql\TaskManagement.API.Migrations.PostgreSql.csproj `
  --context ApplicationDbContext `
  --connection "$env:ConnectionStrings__PostgreSql"
```

Yeni PostgreSQL migration oluşturma:

```powershell
dotnet ef migrations add <MIGRATION_NAME> `
  --project .\Backend\Migrations.PostgreSql\TaskManagement.API.Migrations.PostgreSql.csproj `
  --startup-project .\Backend\Migrations.PostgreSql\TaskManagement.API.Migrations.PostgreSql.csproj `
  --context ApplicationDbContext `
  --namespace TaskManagement.API.Migrations.PostgreSql `
  --output-dir .
```

### Oracle migration update

```powershell
dotnet ef database update `
  --project .\Backend\TaskManagement.API.Migrations.Oracle\TaskManagement.API.Migrations.Oracle.csproj `
  --startup-project .\Backend\TaskManagement.API.Migrations.Oracle\TaskManagement.API.Migrations.Oracle.csproj `
  --context ApplicationDbContext `
  --connection "$env:ConnectionStrings__Oracle"
```

Yeni Oracle migration oluşturma:

```powershell
dotnet ef migrations add <MIGRATION_NAME> `
  --project .\Backend\TaskManagement.API.Migrations.Oracle\TaskManagement.API.Migrations.Oracle.csproj `
  --startup-project .\Backend\TaskManagement.API.Migrations.Oracle\TaskManagement.API.Migrations.Oracle.csproj `
  --context ApplicationDbContext `
  --namespace TaskManagement.API.Migrations.Oracle `
  --output-dir ..\TaskManagement.API\Migrations
```

Daha ayrıntılı bilgi için [`Backend/MIGRATIONS.md`](Backend/MIGRATIONS.md) dosyasına bakın.

## User Secrets kullanımı

API projesinde `UserSecretsId` tanımlıdır. Development secret’larını repo dışında saklamak için proje kökünde aşağıdaki komutları çalıştırabilirsiniz:

```powershell
dotnet user-secrets set "ConnectionStrings:PostgreSql" "Host=<HOST>;Port=<PORT>;Database=<DATABASE>;Username=<USERNAME>;Password=<PASSWORD>" `
  --project .\Backend\TaskManagement.API\TaskManagement.API.csproj

dotnet user-secrets set "Jwt:Key" "<STRONG_RANDOM_SECRET>" `
  --project .\Backend\TaskManagement.API\TaskManagement.API.csproj

dotnet user-secrets set "Jwt:Issuer" "<JWT_ISSUER>" `
  --project .\Backend\TaskManagement.API\TaskManagement.API.csproj

dotnet user-secrets set "Jwt:Audience" "<JWT_AUDIENCE>" `
  --project .\Backend\TaskManagement.API\TaskManagement.API.csproj
```

Development ortamında ilk admin hesabını güvenli şekilde oluşturmak için:

```powershell
dotnet user-secrets set "AdminSeed:Email" "<ADMIN_EMAIL>" `
  --project .\Backend\TaskManagement.API\TaskManagement.API.csproj

dotnet user-secrets set "AdminSeed:Username" "<ADMIN_USERNAME>" `
  --project .\Backend\TaskManagement.API\TaskManagement.API.csproj

dotnet user-secrets set "AdminSeed:Password" "<STRONG_ADMIN_PASSWORD>" `
  --project .\Backend\TaskManagement.API\TaskManagement.API.csproj
```

Admin seed yalnız Development ortamında çalışır; gerekli üç değer eksikse işlem atlanır ve mevcut bir admin varsa ikinci bir admin oluşturulmaz.

Tanımlı secret anahtarlarını görmek için:

```powershell
dotnet user-secrets list --project .\Backend\TaskManagement.API\TaskManagement.API.csproj
```

Bu komut secret değerlerini terminalde gösterir; paylaşılan ekranlarda veya CI loglarında kullanırken dikkatli olun.

## Environment Variables

ASP.NET Core nested configuration anahtarları Environment Variable içinde çift alt çizgi (`__`) ile yazılır.

PostgreSQL örneği:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:DatabaseProvider = "PostgreSql"
$env:ConnectionStrings__PostgreSql = "Host=<HOST>;Port=<PORT>;Database=<DATABASE>;Username=<USERNAME>;Password=<PASSWORD>"
$env:Jwt__Key = "<STRONG_RANDOM_SECRET>"
$env:Jwt__Issuer = "<JWT_ISSUER>"
$env:Jwt__Audience = "<JWT_AUDIENCE>"
$env:Cors__AllowedOrigins__0 = "http://localhost:4200"
```

Oracle örneği:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:DatabaseProvider = "Oracle"
$env:ConnectionStrings__Oracle = "User Id=<USERNAME>;Password=<PASSWORD>;Data Source=<HOST>:<PORT>/<SERVICE_NAME>"
$env:Jwt__Key = "<STRONG_RANDOM_SECRET>"
$env:Jwt__Issuer = "<JWT_ISSUER>"
$env:Jwt__Audience = "<JWT_AUDIENCE>"
$env:Cors__AllowedOrigins__0 = "http://localhost:4200"
```

Bu atamalar yalnız açık PowerShell oturumu boyunca geçerlidir. Production’da platformun secret manager veya environment configuration mekanizmasını kullanın.

## Development ortamında çalıştırma

Backend ve frontend’i iki ayrı PowerShell terminalinde çalıştırın.

### Backend

```powershell
dotnet run --project .\Backend\TaskManagement.API\TaskManagement.API.csproj --launch-profile http
```

Varsayılan development adresleri:

- HTTP: `http://localhost:5266`
- HTTPS: `https://localhost:7012`

### Frontend

```powershell
Set-Location .\Frontend\TaskManagement.Web
npm start
```

Angular development server varsayılan olarak `http://localhost:4200` adresinde açılır.

## Production build ve publish

### Backend Release publish

```powershell
dotnet publish .\Backend\TaskManagement.API\TaskManagement.API.csproj `
  --configuration Release `
  --output .\publish\backend
```

Bu publish varsayılan olarak framework-dependent çıktı üretir; hedef sunucuda uygun .NET 9 runtime bulunmalıdır.

Production çalıştırma örneği:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:DatabaseProvider = "PostgreSql"
$env:ConnectionStrings__PostgreSql = "<PRODUCTION_CONNECTION_STRING>"
$env:Jwt__Key = "<PRODUCTION_JWT_SECRET>"
$env:Jwt__Issuer = "<PRODUCTION_JWT_ISSUER>"
$env:Jwt__Audience = "<PRODUCTION_JWT_AUDIENCE>"

dotnet .\publish\backend\TaskManagement.API.dll
```

Migration’ları production uygulaması başlamadan önce kontrollü bir deployment adımı olarak çalıştırın.

### Frontend production build

```powershell
Set-Location .\Frontend\TaskManagement.Web
npm ci
npm run build
```

Build çıktısı:

```text
Frontend/TaskManagement.Web/dist/TaskManagement.Web/browser/
```

Bu dizini IIS, Nginx, Apache, CDN veya tercih edilen statik hosting çözümünde yayınlayabilirsiniz. SPA route’larının `index.html` dosyasına yönlendirilmesi gerekir.

## deploy.ps1 kullanımı

Root dizindeki [`deploy.ps1`](deploy.ps1) backend için aşağıdaki adımları otomatikleştirir:

1. `publish/backend` dizinindeki eski çıktıyı temizler.
2. NuGet paketlerini restore eder.
3. Release build alır.
4. Backend’i `publish/backend` dizinine publish eder.

Çalıştırmak için:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\deploy.ps1
```

> `deploy.ps1` yalnız backend publish çıktısını hazırlar. Frontend build, database migration, sunucuya dosya aktarımı ve servis restart işlemlerini gerçekleştirmez.

## Health Check

Health Check endpoint’i:

```text
GET /health
```

Development ortamında kontrol etmek için:

```powershell
Invoke-WebRequest http://localhost:5266/health
```

`DatabaseProvider=PostgreSql` olduğunda endpoint PostgreSQL bağlantısını da kontrol eder. Mevcut uygulamada Oracle için provider-specific database health check kaydı bulunmadığından Oracle seçiliyken endpoint yalnız kayıtlı genel kontrolleri yansıtır.

## Swagger / API dokümantasyonu

Swagger yalnız Development ortamında etkinleştirilir:

```text
http://localhost:5266/swagger
https://localhost:7012/swagger
```

Korumalı endpoint’leri denemek için:

1. `/api/auth/login` üzerinden token alın.
2. Swagger’daki **Authorize** butonunu açın.
3. Token’ı `Bearer <TOKEN>` biçiminde girin.

Production ortamında Swagger UI varsayılan olarak kapalıdır.

## Güvenlik notları

- Gerçek connection string, JWT key ve parolaları repoya commit etmeyin.
- Development için User Secrets, production için Environment Variables veya bir secret manager kullanın.
- JWT key yüksek entropili ve yeterince uzun olmalıdır.
- Yeni kayıt olan kullanıcılar request içeriğinden bağımsız olarak `User` rolüyle oluşturulur.
- Admin endpoint’leri `[Authorize(Roles = "Admin")]` ile korunur.
- Son aktif adminin yanlışlıkla pasifleştirilmesi veya rolünün düşürülmesi engellenir.
- Rol değişikliği mevcut JWT’yi geriye dönük iptal etmez; yeni rol login veya token yenilenmesi sonrasında alınan token’da geçerli olur.
- CORS origin’lerini production frontend adresleriyle sınırlandırın.
- HTTPS, HSTS, reverse proxy ve güvenlik header’larını production ortamında doğru yapılandırın.
- Yüklenen dosyalar uygulamanın çalışma dizinindeki `Uploads` klasöründe tutulur. Production’da kalıcı depolama, yedekleme, dosya boyutu/type doğrulaması ve zararlı içerik taraması planlanmalıdır.
- `logs` ve `Uploads` klasörlerinin web root üzerinden doğrudan yayınlanmadığından emin olun.
- Database kullanıcılarına yalnız gerekli yetkileri verin ve provider migration’larını birbirine karıştırmayın.

## Ekran görüntüleri

| Dashboard — Light | Dashboard — Dark |
| --- | --- |
| ![Dashboard Light](docs/screenshots/dashboard-light.png) | ![Dashboard Dark](docs/screenshots/dashboard-dark.png) |

| Görevler | Görev detayı |
| --- | --- |
| ![Görevler](docs/screenshots/tasks.png) | ![Görev detayı](docs/screenshots/task-detail.png) |

| Kategoriler | Profil |
| --- | --- |
| ![Kategoriler](docs/screenshots/categories.png) | ![Profil](docs/screenshots/profile.png) |

Diğer mevcut görseller `docs/screenshots/` klasöründedir. Yeni ekran görüntüleri için önerilen placeholder’lar:

- `docs/screenshots/admin-users.png`
- `docs/screenshots/mobile-kanban.png`
- `docs/screenshots/task-attachments.png`

## Gelecek geliştirmeler

Aşağıdaki maddeler mevcut özellik değil, olası yol haritası önerileridir:

- Oracle için provider-specific database Health Check
- Refresh token ve aktif token iptal mekanizması
- Dosyalar için object storage ve antivirüs taraması
- E-posta/bildirim sistemi ve son tarih hatırlatıcıları
- Daha kapsamlı unit, integration ve end-to-end testleri
- Docker/Docker Compose geliştirme ortamı
- CI/CD pipeline, otomatik migration doğrulaması ve release paketleme
- Audit log ve admin işlem geçmişi
- Çoklu dil desteği

## Katkıda bulunma

Katkılar için önerilen akış:

1. Repoyu fork edin.
2. Açıklayıcı bir feature branch oluşturun.
3. Değişikliklerinizi küçük ve anlamlı commit’lere ayırın.
4. Backend ve frontend build’lerini doğrulayın.
5. Migration değişikliği varsa yalnız ilgili provider projesinde üretildiğini kontrol edin.
6. Değişiklik kapsamı ve test sonuçlarıyla Pull Request açın.

```powershell
git checkout -b feature/aciklayici-isim
dotnet build .\Backend\TaskManagement.API\TaskManagement.API.csproj --configuration Release

Set-Location .\Frontend\TaskManagement.Web
npm ci
npm run build
```

Kod katkısı göndermeden önce secret, connection string, log, publish çıktısı veya kullanıcı yüklemesi eklenmediğini kontrol edin.

## Lisans

Bu repoda şu anda bir `LICENSE` dosyası bulunmamaktadır. Açık kaynak kullanım ve dağıtım koşulları tanımlanana kadar tüm haklar proje sahibine aittir. Proje açık kaynak olarak yayınlanacaksa uygun lisans metni root dizine `LICENSE` adıyla eklenmelidir.
