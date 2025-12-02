#!/usr/bin/env pwsh
# Script para ver logs dos containers
# Uso: .\logs.ps1 [sqlserver|api|webapp|all]

param(
    [Parameter(Position=0)]
    [ValidateSet("sqlserver", "api", "webapp", "all")]
    [string]$Container = "all"
)

$containers = @{
    "sqlserver" = "cotacao-sqlserver"
    "api" = "cotacao-api"
    "webapp" = "cotacao-webapp"
}

if ($Container -eq "all") {
    Write-Host "=== Logs de todos os containers ===" -ForegroundColor Cyan
    foreach ($c in $containers.Values) {
        Write-Host "`n--- $c ---" -ForegroundColor Yellow
        podman logs --tail 50 $c 2>&1
    }
} else {
    $name = $containers[$Container]
    Write-Host "=== Logs de $name ===" -ForegroundColor Cyan
    podman logs --tail 100 -f $name
}
