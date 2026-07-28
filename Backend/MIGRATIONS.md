# Provider-specific EF Core migrations

`ApplicationDbContext` ve entity model ortak `TaskManagement.Data` assembly'sinde derlenir.
Migration zincirleri birbirinden bağımsızdır:

- PostgreSQL: `TaskManagement.API.Migrations.PostgreSql`
- Oracle: `TaskManagement.API.Migrations.Oracle`

`DatabaseProvider` değeri `PostgreSql` veya `Oracle` olmalıdır. API, seçilen provider ile
birlikte yalnız ilgili migration assembly'sini yükler.

## PostgreSQL

Yeni migration:

```powershell
dotnet ef migrations add MigrationName `
  --project Backend\Migrations.PostgreSql\TaskManagement.API.Migrations.PostgreSql.csproj `
  --startup-project Backend\Migrations.PostgreSql\TaskManagement.API.Migrations.PostgreSql.csproj `
  --context ApplicationDbContext `
  --namespace TaskManagement.API.Migrations.PostgreSql `
  --output-dir .
```

Veritabanını güncelleme:

```powershell
dotnet ef database update `
  --project Backend\Migrations.PostgreSql\TaskManagement.API.Migrations.PostgreSql.csproj `
  --startup-project Backend\Migrations.PostgreSql\TaskManagement.API.Migrations.PostgreSql.csproj `
  --context ApplicationDbContext `
  --connection "$env:ConnectionStrings__PostgreSql"
```

## Oracle

Mevcut Oracle migration dosyaları API altındaki `Migrations` klasöründe korunur ve yalnız
Oracle migration projesi tarafından derlenir.

Yeni migration:

```powershell
dotnet ef migrations add MigrationName `
  --project Backend\TaskManagement.API.Migrations.Oracle\TaskManagement.API.Migrations.Oracle.csproj `
  --startup-project Backend\TaskManagement.API.Migrations.Oracle\TaskManagement.API.Migrations.Oracle.csproj `
  --context ApplicationDbContext `
  --namespace TaskManagement.API.Migrations.Oracle `
  --output-dir ..\TaskManagement.API\Migrations
```

Veritabanını güncelleme:

```powershell
dotnet ef database update `
  --project Backend\TaskManagement.API.Migrations.Oracle\TaskManagement.API.Migrations.Oracle.csproj `
  --startup-project Backend\TaskManagement.API.Migrations.Oracle\TaskManagement.API.Migrations.Oracle.csproj `
  --context ApplicationDbContext `
  --connection "$env:ConnectionStrings__Oracle"
```

Her provider kendi veritabanındaki `__EFMigrationsHistory` tablosunu ve yalnız kendi
migration ID zincirini kullanır.
