param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("all", "sql-only", "app-only")]
    [string]$Mode = "sql-only"
)

$ErrorActionPreference = "Stop"

Write-Host "=== Ambiente Local - Modo: $Mode ===" -ForegroundColor Cyan

# Configurações
$SqlPort = 14333
$ApiPort = 5138
$WebAppPort = 4200
$SqlPassword = "Teste@123"
$NetworkName = "cotacao-network"

function Start-SqlContainer {
    Write-Host "`n[SQL Server] Verificando container..." -ForegroundColor Yellow
    
    # Criar rede se não existir
    $networkExists = podman network exists $NetworkName 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[SQL Server] Criando rede $NetworkName..." -ForegroundColor Yellow
        podman network create $NetworkName
    }
    
    # Verificar se container já existe
    $containerExists = podman ps -a --filter "name=cotacao-sqlserver" --format "{{.Names}}" 2>$null
    
    if ($containerExists) {
        $isRunning = podman ps --filter "name=cotacao-sqlserver" --format "{{.Names}}" 2>$null
        if ($isRunning) {
            Write-Host "[SQL Server] Container já está rodando" -ForegroundColor Green
        } else {
            Write-Host "[SQL Server] Iniciando container existente..." -ForegroundColor Yellow
            podman start cotacao-sqlserver
        }
    } else {
        Write-Host "[SQL Server] Criando e iniciando container..." -ForegroundColor Yellow
        podman run -d `
            --name cotacao-sqlserver `
            --network $NetworkName `
            -e "ACCEPT_EULA=Y" `
            -e "SA_PASSWORD=$SqlPassword" `
            -p ${SqlPort}:1433 `
            -v mssql-data:/var/opt/mssql `
            mcr.microsoft.com/mssql/server:2022-latest
    }
    
    Write-Host "[SQL Server] Aguardando inicialização (10s)..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
    Write-Host "[SQL Server] ✓ Disponível em localhost:$SqlPort (sa / $SqlPassword)" -ForegroundColor Green
}

function Start-ApiLocal {
    Write-Host "`n[API] Compilando e executando localmente..." -ForegroundColor Yellow
    
    # Connection string para SQL local ou container
    $connString = "Server=localhost,$SqlPort;Database=CotacaoSegurosDb;User Id=sa;Password=$SqlPassword;TrustServerCertificate=True;Encrypt=False"
    
    # Variáveis de ambiente
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:ASPNETCORE_URLS = "http://localhost:$ApiPort"
    $env:ConnectionStrings__DefaultConnection = $connString
    
    Push-Location ".\src\WebApi"
    
    Write-Host "[API] Restaurando pacotes..." -ForegroundColor Yellow
    dotnet restore
    
    Write-Host "[API] Executando migrations..." -ForegroundColor Yellow
    dotnet ef database update --project ..\Infrastructure\Infrastructure.csproj
    
    Write-Host "[API] Iniciando aplicação..." -ForegroundColor Yellow
    Write-Host "[API] ✓ API rodando em http://localhost:$ApiPort" -ForegroundColor Green
    Write-Host "[API] ✓ Swagger disponível em http://localhost:$ApiPort/swagger" -ForegroundColor Green
    Write-Host "`nPressione Ctrl+C para parar..." -ForegroundColor Cyan
    
    dotnet run
    
    Pop-Location
}

function Start-WebAppLocal {
    Write-Host "`n[WebApp] Instalando dependências e executando..." -ForegroundColor Yellow
    
    Push-Location ".\src\WebApp"
    
    # Verificar se node_modules existe
    if (-not (Test-Path "node_modules")) {
        Write-Host "[WebApp] Instalando dependências npm..." -ForegroundColor Yellow
        npm install --registry=https://registry.npmjs.org/
    }
    
    Write-Host "[WebApp] Iniciando aplicação Angular..." -ForegroundColor Yellow
    Write-Host "[WebApp] ✓ WebApp rodando em http://localhost:$WebAppPort" -ForegroundColor Green
    Write-Host "`nPressione Ctrl+C para parar..." -ForegroundColor Cyan
    
    npm start
    
    Pop-Location
}

function Show-LocalInfo {
    Write-Host "`n=== Informações do Ambiente ===" -ForegroundColor Cyan
    Write-Host "SQL Server: localhost:$SqlPort (sa / $SqlPassword)" -ForegroundColor White
    Write-Host "API: http://localhost:$ApiPort" -ForegroundColor White
    Write-Host "Swagger: http://localhost:$ApiPort/swagger" -ForegroundColor White
    Write-Host "WebApp: http://localhost:$WebAppPort" -ForegroundColor White
    Write-Host "`nPara parar:" -ForegroundColor Cyan
    Write-Host "  - API/WebApp local: Ctrl+C no terminal" -ForegroundColor White
    Write-Host "  - SQL Container: podman stop cotacao-sqlserver" -ForegroundColor White
    Write-Host ""
}

# Executar de acordo com o modo
switch ($Mode) {
    "all" {
        Write-Host "`nModo 'all': Tudo em containers" -ForegroundColor Cyan
        Write-Host "Use .\dev-up.ps1 para este modo!" -ForegroundColor Yellow
        exit 0
    }
    
    "sql-only" {
        Write-Host "`nModo 'sql-only': SQL em container, API e WebApp local" -ForegroundColor Cyan
        Start-SqlContainer
        Show-LocalInfo
        
        Write-Host "`n=== Escolha o que executar ===" -ForegroundColor Cyan
        Write-Host "1 - API (ASP.NET Core)" -ForegroundColor White
        Write-Host "2 - WebApp (Angular)" -ForegroundColor White
        Write-Host "3 - Ambos (em terminais separados)" -ForegroundColor White
        $choice = Read-Host "`nOpção"
        
        switch ($choice) {
            "1" { Start-ApiLocal }
            "2" { Start-WebAppLocal }
            "3" {
                Write-Host "`nAbra 2 terminais separados:" -ForegroundColor Yellow
                Write-Host "  Terminal 1: .\dev-local.ps1 -Mode sql-only  # Escolha opção 1 (API)" -ForegroundColor White
                Write-Host "  Terminal 2: .\dev-local.ps1 -Mode sql-only  # Escolha opção 2 (WebApp)" -ForegroundColor White
            }
            default {
                Write-Host "Opção inválida!" -ForegroundColor Red
            }
        }
    }
    
    "app-only" {
        Write-Host "`nModo 'app-only': SQL local, API e WebApp em containers" -ForegroundColor Cyan
        Write-Host "`nPré-requisitos:" -ForegroundColor Yellow
        Write-Host "  1. SQL Server instalado localmente" -ForegroundColor White
        Write-Host "  2. Database 'CotacaoSegurosDb' criada" -ForegroundColor White
        Write-Host "  3. Usuario 'sa' com senha '$SqlPassword'" -ForegroundColor White
        Write-Host "`nPara instalar SQL Server localmente, visite:" -ForegroundColor Yellow
        Write-Host "  https://www.microsoft.com/sql-server/sql-server-downloads" -ForegroundColor Cyan
        Write-Host "`nDepois de instalar o SQL Server local, use .\dev-up.ps1" -ForegroundColor Yellow
        Write-Host "mas ajuste a connection string em appsettings.Development.json" -ForegroundColor Yellow
    }
}
