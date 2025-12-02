# Script dedicado para rodar apenas o WebApp localmente
# A API pode estar em container ou local (http://localhost:5138)

$ErrorActionPreference = "Stop"

Write-Host "=== Executando WebApp Localmente ===" -ForegroundColor Cyan

Push-Location ".\src\WebApp"

try {
    # Verificar se node_modules existe
    if (-not (Test-Path "node_modules")) {
        Write-Host "[WebApp] Instalando dependências npm..." -ForegroundColor Yellow
        npm install --registry=https://registry.npmjs.org/
    } else {
        Write-Host "[WebApp] Dependências já instaladas" -ForegroundColor Green
    }
    
    Write-Host "`n[WebApp] ✓ WebApp rodando em http://localhost:4200" -ForegroundColor Green
    Write-Host "[WebApp] ✓ API configurada para: http://localhost:5138" -ForegroundColor Green
    Write-Host "`nPressione Ctrl+C para parar...`n" -ForegroundColor Cyan
    
    npm start
}
finally {
    Pop-Location
}
