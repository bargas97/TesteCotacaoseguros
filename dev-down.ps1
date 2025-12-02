#!/usr/bin/env pwsh
# Script para derrubar ambiente Podman
# Uso: .\dev-down.ps1

Write-Host "=== Derrubando ambiente ===" -ForegroundColor Cyan

# Verificar se Podman está instalado
if (-not (Get-Command podman -ErrorAction SilentlyContinue)) {
    Write-Host "Erro: Podman não encontrado" -ForegroundColor Red
    exit 1
}

Write-Host "Parando e removendo containers..." -ForegroundColor Yellow

# Parar e remover containers
$containers = @("cotacao-webapp", "cotacao-api", "cotacao-sqlserver")
foreach ($c in $containers) {
    Write-Host "  Removendo $c..." -ForegroundColor Gray
    podman stop $c 2>$null
    podman rm $c 2>$null
}

# Remover rede (opcional - comentado para manter volume do SQL)
# podman network rm cotacao-network 2>$null

Write-Host "`n=== Ambiente derrubado! ===" -ForegroundColor Cyan
Write-Host "Para subir: .\dev-up.ps1" -ForegroundColor Gray
Write-Host "`nNota: Volume do SQL Server foi mantido (mssql-data)" -ForegroundColor Yellow
Write-Host "Para remover completamente: podman volume rm mssql-data" -ForegroundColor Gray
