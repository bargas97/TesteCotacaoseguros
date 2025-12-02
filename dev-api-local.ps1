# Script dedicado para rodar apenas a API localmente
# Pressupõe que SQL Server está em container (use dev-local.ps1 primeiro)

param(
    [Parameter(Mandatory=$false)]
    [string]$SqlHost = "localhost",
    
    [Parameter(Mandatory=$false)]
    [int]$SqlPort = 14333,
    
    [Parameter(Mandatory=$false)]
    [string]$SqlPassword = "Teste@123"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Executando API Localmente ===" -ForegroundColor Cyan

# Connection string
$connString = "Server=$SqlHost,$SqlPort;Database=CotacaoSegurosDb;User Id=sa;Password=$SqlPassword;TrustServerCertificate=True;Encrypt=False"

# Variáveis de ambiente
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5138"
$env:ConnectionStrings__DefaultConnection = $connString

Push-Location ".\src\WebApi"

try {
    Write-Host "[API] Restaurando pacotes..." -ForegroundColor Yellow
    dotnet restore
    
    Write-Host "[API] Aplicando migrations..." -ForegroundColor Yellow
    dotnet ef database update --project ..\Infrastructure\Infrastructure.csproj --no-build
    
    Write-Host "`n[API] ✓ API rodando em http://localhost:5138" -ForegroundColor Green
    Write-Host "[API] ✓ Swagger: http://localhost:5138/swagger" -ForegroundColor Green
    Write-Host "[API] ✓ Health Check: http://localhost:5138/health" -ForegroundColor Green
    Write-Host "`nPressione Ctrl+C para parar...`n" -ForegroundColor Cyan
    
    dotnet run
}
finally {
    Pop-Location
}
