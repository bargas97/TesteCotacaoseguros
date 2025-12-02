using Application.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost,62461;Database=CotacaoSegurosDb;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";
        services.AddDbContext<CotacaoDbContext>(opt => opt.UseSqlServer(conn));
        services.AddScoped<ICotacaoRepository, CotacaoRepository>();
        return services;
    }
}