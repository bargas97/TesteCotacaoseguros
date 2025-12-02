#!/usr/bin/env pwsh
# Script para subir ambiente com Podman (sem compose)
# Uso: .\dev-up.ps1

Write-Host "=== Iniciando ambiente de desenvolvimento ===" -ForegroundColor Cyan

# Verificar se Podman está instalado
if (-not (Get-Command podman -ErrorAction SilentlyContinue)) {
    Write-Host "Erro: Podman não encontrado" -ForegroundColor Red
    exit 1
}

# Criar rede se não existir
Write-Host "Criando rede cotacao-network..." -ForegroundColor Gray
podman network create cotacao-network 2>$null

# 1. Subir SQL Server
Write-Host "`n[1/3] Subindo SQL Server..." -ForegroundColor Yellow
podman run -d `
    --name cotacao-sqlserver `
    --network cotacao-network `
    -e SA_PASSWORD=Teste@123 `
    -e ACCEPT_EULA=Y `
    -p 14333:1433 `
    -v mssql-data:/var/opt/mssql `
    mcr.microsoft.com/mssql/server:2022-latest

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao subir SQL Server (pode já estar rodando)" -ForegroundColor Yellow
}

Start-Sleep -Seconds 5

# 2. Build e subir API
Write-Host "`n[2/3] Building e subindo API..." -ForegroundColor Yellow
$apiContext = Resolve-Path "."
$apiDockerfile = Resolve-Path "src\WebApi\Dockerfile"

podman build -t cotacao-api -f $apiDockerfile $apiContext
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro no build da API" -ForegroundColor Red
    exit 1
}

podman run -d `
    --name cotacao-api `
    --network cotacao-network `
    -e ASPNETCORE_ENVIRONMENT=Development `
    -e ASPNETCORE_URLS=http://+:8080 `
    -e "ConnectionStrings__DefaultConnection=Server=cotacao-sqlserver,1433;Database=CotacaoSegurosDb;User Id=sa;Password=Teste@123;TrustServerCertificate=True;Encrypt=False" `
    -p 5138:8080 `
    cotacao-api

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao subir API (pode já estar rodando)" -ForegroundColor Yellow
}

Start-Sleep -Seconds 5

# 3. Build e subir WebApp
Write-Host "`n[3/3] Building e subindo WebApp..." -ForegroundColor Yellow
$webContext = Resolve-Path "src\WebApp"
$webDockerfile = Resolve-Path "src\WebApp\Dockerfile"

podman build -t cotacao-webapp -f $webDockerfile $webContext
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro no build do WebApp" -ForegroundColor Red
    exit 1
}

podman run -d `
    --name cotacao-webapp `
    --network cotacao-network `
    -p 4200:80 `
    cotacao-webapp

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao subir WebApp (pode já estar rodando)" -ForegroundColor Yellow
}

Write-Host "`n=== Ambiente iniciado! ===" -ForegroundColor Cyan
Write-Host "`nServiços:" -ForegroundColor White
Write-Host "  SQL Server -> localhost:14333 (sa/Teste@123)" -ForegroundColor Gray
Write-Host "  API        -> http://localhost:5138 (/swagger)" -ForegroundColor Gray  
Write-Host "  WebApp     -> http://localhost:4200" -ForegroundColor Gray
Write-Host "`nComandos úteis:" -ForegroundColor White
Write-Host "  Ver logs:      .\logs.ps1 [api|webapp|sqlserver|all]" -ForegroundColor Gray
Write-Host "  Parar tudo:    .\dev-down.ps1" -ForegroundColor Gray
Write-Host "  Status:        podman ps" -ForegroundColor Gray
