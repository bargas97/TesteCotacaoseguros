using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence;

public class CotacaoDbContextFactory : IDesignTimeDbContextFactory<CotacaoDbContext>
{
    public CotacaoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CotacaoDbContext>();

        // Usa mesma conexão padrão utilizada em DI (fallback),
        // permitindo gerar/aplicar migrations sem depender do WebApi.
        var connectionString = "Server=localhost,62461;Database=CotacaoSegurosDb;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";

        optionsBuilder.UseSqlServer(connectionString);

        return new CotacaoDbContext(optionsBuilder.Options);
    }
}
