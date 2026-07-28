$ErrorActionPreference = "Stop"

$projectPath = ".\Backend\TaskManagement.API\TaskManagement.API.csproj"
$publishPath = ".\publish\backend"

Write-Host "========================================"
Write-Host " Task Management API Deployment"
Write-Host "========================================"

if (-not (Test-Path $projectPath)) {
    Write-Error "Proje dosyası bulunamadı: $projectPath"
    exit 1
}

Write-Host "`nEski publish çıktısı temizleniyor..."

if (Test-Path $publishPath) {
    Remove-Item $publishPath -Recurse -Force
}

Write-Host "`nNuGet paketleri geri yükleniyor..."
dotnet restore $projectPath

Write-Host "`nRelease build alınıyor..."
dotnet build $projectPath -c Release --no-restore

Write-Host "`nUygulama publish ediliyor..."
dotnet publish $projectPath `
    -c Release `
    -o $publishPath `
    --no-build

Write-Host "`nDeployment çıktısı başarıyla hazırlandı."
Write-Host "Konum: $publishPath"