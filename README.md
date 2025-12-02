# Cotação de Seguros - Full Stack

Solução full-stack com ASP.NET Core 8 (Web API), Angular 18 (SSR), SQL Server e Docker. Arquitetura Clean (DDD + CQRS) com EF Core, MediatR, AutoMapper e FluentValidation.

## Executar local

### Automatizado com Podman (Recomendado)

```powershell
# Subir ambiente completo (SQL Server + API + WebApp)
.\dev-up.ps1

# Ver logs de containers específicos
.\logs.ps1 [sqlserver|api|webapp|all]

# Derrubar ambiente completo
.\dev-down.ps1
```

Após executar `dev-up.ps1`, aguarde ~30 segundos para os serviços inicializarem:
- **SQL Server**: localhost:14333 (sa / Teste@123)
- **WebApi**: http://localhost:5138 (Swagger: /swagger)
- **WebApp**: http://localhost:4200
- **Health Check**: http://localhost:5138/health

**Requisitos**: Podman instalado com `podman-compose` (`pip install podman-compose`)

### Manual (SQL Container + API/WebApp Local)

**Opção recomendada para desenvolvimento**:

```powershell
# 1. Subir apenas SQL Server em container
.\dev-local.ps1 -Mode sql-only

# 2. Em outro terminal, rodar API local
.\dev-api-local.ps1

# 3. Em outro terminal, rodar WebApp local
.\dev-webapp-local.ps1
```

**Ou executar individualmente**:

1. Backend API
```powershell
Push-Location .\src\WebApi
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DefaultConnection = "Server=localhost,14333;Database=CotacaoSegurosDb;User Id=sa;Password=Teste@123;TrustServerCertificate=True;Encrypt=False"
dotnet run
Pop-Location
```

2. Frontend WebApp
```powershell
Push-Location .\src\WebApp
npm install
npm start
Pop-Location
```

**URLs**:
- WebApp: http://localhost:4200
- API Swagger: http://localhost:5138/swagger
- SQL Server: localhost:14333 (sa / Teste@123)

## Docker Compose

```powershell
docker compose build
docker compose up -d
```

- Web: http://localhost:4200
- API: http://localhost:5138
- SQL Server: localhost,14333 (sa / Teste@123)

A API aplica `Migrate()` na inicialização com retry automático. O seed cria um produto, coberturas e fatores, além da SP `sp_GetCotacaoCompleta`.

## Frontend (Angular 18)
- Angular Material + máscaras (ngx-mask)
- Validações: CPF/CNPJ, CEP, datas com Datepicker
- SSR habilitado com "prerender" (evitamos chamadas HTTP durante SSR)

### Scripts úteis
```powershell
# build SSR
npm run build
# rodar SSR local
npm run serve:ssr:webapp
```

## Testes
- Domain (xUnit): cálculos e validações de regras do domínio
- API (Integração): fluxo principal de cotação via `WebApplicationFactory`
- WebApp (E2E): Playwright para fluxo happy path

Para executar (exemplos):
```powershell
# Backend (ajuste paths conforme necessário)
Push-Location .\tests\Domain.Tests; dotnet test; Pop-Location
Push-Location .\tests\WebApi.Tests; dotnet test; Pop-Location
```

## Decisões Arquiteturais
- DDD + CQRS com MediatR nas camadas Application e Domain
- EF Core 8 + Migrations (Baseline aplicada; `DbInitializer` chama `Migrate()`)
- Logs com Serilog; validações com FluentValidation; mapeamentos com AutoMapper
- Docker: SQL Server 2022, API .NET 8, WebApp Nginx

## Estrutura
- `src/Domain` – Entidades e serviços de domínio
- `src/Application` – DTOs, Commands/Queries, Handlers, Validators e Profiles
- `src/Infrastructure` – DbContext, Migrations, Repositórios, Seed e SP
- `src/WebApi` – Controllers, Program, DI
- `src/WebApp` – Angular 18 (Material, máscaras)
- `tests` – Testes de domínio e integração

## Observações
- Ajuste a connection string se necessário: `ConnectionStrings__DefaultConnection`
- Para desenvolvimento com proxy, `src/WebApp/proxy.conf.json` aponta para a API
- Budgets de build ajustados para uso com Angular Material
 
## Troubleshooting

### Ver logs de container específico
```powershell
podman logs --tail 50 -f cotacao-sqlserver
podman logs --tail 50 -f cotacao-api  
podman logs --tail 50 -f cotacao-webapp
```

### Rebuild apenas um serviço
```powershell
# Rebuild apenas WebApp
podman stop cotacao-webapp
podman rm cotacao-webapp
podman rmi cotacao-webapp
podman build -t cotacao-webapp -f "src\WebApp\Dockerfile" "src\WebApp"
podman run -d --name cotacao-webapp --network cotacao-network -p 4200:80 cotacao-webapp
```

### Testar conexão SQL dentro do container
```powershell
podman exec -it cotacao-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Teste@123 -Q "SELECT name FROM sys.databases;"
```

### Acessar shell do container
```powershell
podman exec -it cotacao-api /bin/bash
podman exec -it cotacao-webapp /bin/sh
```
