using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class CotacaoDbContext : DbContext
{
    public CotacaoDbContext(DbContextOptions<CotacaoDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Cobertura> Coberturas => Set<Cobertura>();
    public DbSet<FatorBonus> FatoresBonus => Set<FatorBonus>();
    public DbSet<FatorPerfil> FatoresPerfil => Set<FatorPerfil>();
    public DbSet<FatorRegiao> FatoresRegiao => Set<FatorRegiao>();
    public DbSet<FatorUtilizacao> FatoresUtilizacao => Set<FatorUtilizacao>();
    public DbSet<FatorFranquia> FatoresFranquia => Set<FatorFranquia>();
    public DbSet<Cotacao> Cotacoes => Set<Cotacao>();
    public DbSet<Proponente> Proponentes => Set<Proponente>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<CotacaoCobertura> CotacaoCoberturas => Set<CotacaoCobertura>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CotacaoDbContext).Assembly);
    }
}