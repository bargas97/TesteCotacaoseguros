using Application;
using Infrastructure;
using Serilog;
using Serilog.Context;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    var xmlPath = Path.Combine(AppContext.BaseDirectory, "WebApi.xml");
    if (File.Exists(xmlPath)) opt.IncludeXmlComments(xmlPath);
});
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks()
    .AddCheck("sqlserver", () =>
    {
        try
        {
            var cs = builder.Configuration.GetConnectionString("DefaultConnection");
            using var conn = new SqlConnection(cs);
            conn.Open();
            using var cmd = new SqlCommand("SELECT 1;", conn);
            cmd.ExecuteScalar();
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    });

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] (Corr:{CorrelationId}) {SourceContext} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
builder.Host.UseSerilog((ctx, services, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .Enrich.WithMachineName()
       .Enrich.WithThreadId()
       .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] (Corr:{CorrelationId}) {SourceContext} {Message:lj}{NewLine}{Exception}");
});

var app = builder.Build();

// CorrelationId + Request Logging
app.Use(async (ctx, next) =>
{
    var cid = ctx.Request.Headers["X-Correlation-Id"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(cid)) cid = Guid.NewGuid().ToString();
    ctx.Response.Headers["X-Correlation-Id"] = cid;
    using (LogContext.PushProperty("CorrelationId", cid))
    {
        await next();
    }
});

app.UseSerilogRequestLogging(opts =>
{
    opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

// Middleware de tratamento de exceções de domínio/negócio
app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch (InvalidOperationException ex)
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (KeyNotFoundException ex)
    {
        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
        await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await ctx.Response.WriteAsJsonAsync(new { error = "Erro interno." });
        // log detalhado já sai via Serilog RequestLogging + exceção
        Serilog.Log.Error(ex, "Erro não tratado");
    }
});

// Configure the HTTP request pipeline.
app.UseCors();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}
app.UseAuthorization();
app.MapControllers();
// Liveness (simple), readiness (aggregate health checks) endpoints
app.MapGet("/health", () => Results.Ok(new { status = "ok" })); // liveness
app.MapHealthChecks("/healthz"); // includes sqlserver check
app.MapHealthChecks("/ready", new HealthCheckOptions { Predicate = _ => false }); // returns healthy if pipeline reached

// Apply migrations + seed
using (var scope = app.Services.CreateScope())
{
    try
    {
        var ctx = scope.ServiceProvider.GetRequiredService<Infrastructure.Persistence.CotacaoDbContext>();
        if (ctx.Database.IsRelational())
        {
            await Infrastructure.Persistence.DbInitializer.InitializeAsync(ctx);
        }
    }
    catch (Exception ex)
    {
        // Em testes com InMemory ou SQL indisponível, apenas loga aviso.
        app.Logger.LogWarning(ex, "Falha ao aplicar migrations/seed, prosseguindo sem inicialização de BD.");
    }
}

app.Run();

// Controllers mapeados

public partial class Program { }
