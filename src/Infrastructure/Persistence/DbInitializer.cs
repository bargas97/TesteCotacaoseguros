using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeAsync(CotacaoDbContext ctx)
    {
        if (!ctx.Database.IsRelational()) return; // Ignora inicialização para provedores não relacionais (ex.: InMemory nos testes)
        // Sempre aplica migrations para manter o schema atualizado (com retry para ambiente de containers)
        var maxAttempts = 10;
        var delayMs = 3000;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await ctx.Database.MigrateAsync();
                break;
            }
            catch
            {
                if (attempt == maxAttempts) throw;
                await Task.Delay(delayMs);
            }
        }

        if (!ctx.Produtos.Any())
        {
            var produto = new Produto("Auto Individual", new []{"Baixa", "Média", "Alta"});
            produto.DefinirParametros(0.10m, 0.0738m, 25.00m, 0.20m);
            ctx.Produtos.Add(produto);
            await ctx.SaveChangesAsync();

            // Coberturas padrão
            ctx.Coberturas.AddRange(
                new Cobertura(produto.Id, "DM", "Danos Materiais", TipoCobertura.Basica, 1000, 50000),
                new Cobertura(produto.Id, "DC", "Danos Corporais", TipoCobertura.Basica, 1000, 50000),
                new Cobertura(produto.Id, "RCF-M", "Responsabilidade Civil - Materiais", TipoCobertura.Adicional, 500, 100000),
                new Cobertura(produto.Id, "APP-Passageiro", "Acidentes Pessoais Passageiro", TipoCobertura.Adicional, 500, 100000)
            );

            // Fatores de exemplo
            for (int i = 0; i <= 10; i++) ctx.FatoresBonus.Add(new FatorBonus(i, 1 - (i * 0.02m))); // bônus reduz 2% por classe
            ctx.FatoresPerfil.AddRange(
                new FatorPerfil(FaixaIdade.Idade18_25, Genero.M, 1.10m),
                new FatorPerfil(FaixaIdade.Idade18_25, Genero.F, 1.05m),
                new FatorPerfil(FaixaIdade.Idade26_35, Genero.M, 1.00m),
                new FatorPerfil(FaixaIdade.Idade26_35, Genero.F, 0.98m),
                new FatorPerfil(FaixaIdade.Idade36_60, Genero.M, 0.95m),
                new FatorPerfil(FaixaIdade.Idade36_60, Genero.F, 0.93m),
                new FatorPerfil(FaixaIdade.Idade60Mais, Genero.M, 1.20m),
                new FatorPerfil(FaixaIdade.Idade60Mais, Genero.F, 1.15m)
            );
            ctx.FatoresRegiao.AddRange(
                new FatorRegiao("01000000", "05999999", 1.10m),
                new FatorRegiao("06000000", "09999999", 1.00m),
                new FatorRegiao("10000000", "19999999", 0.95m)
            );
            ctx.FatoresUtilizacao.AddRange(
                new FatorUtilizacao(TipoUtilizacao.Particular, 1.00m),
                new FatorUtilizacao(TipoUtilizacao.Profissional, 1.15m),
                new FatorUtilizacao(TipoUtilizacao.Aplicativo, 1.25m)
            );
            ctx.FatoresFranquia.AddRange(
                new FatorFranquia("Baixa", 1.20m),
                new FatorFranquia("Média", 1.00m),
                new FatorFranquia("Alta", 0.85m)
            );

            await ctx.SaveChangesAsync();
        }

        // Garantir que sequence CotacaoSeq existe (fallback se migration não aplicou)
        try
        {
            var checkSeq = @"IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = 'CotacaoSeq')
                CREATE SEQUENCE dbo.CotacaoSeq START WITH 1 INCREMENT BY 1;";
            await ctx.Database.ExecuteSqlRawAsync(checkSeq);
        }
        catch { /* ignora erro se sequence já existir */ }

        // Garantir que colunas IofValor e CustoServicosAplicado existem (fallback migration)
        try
        {
            var addColumns = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Cotacoes') AND name = 'IofValor')
                    ALTER TABLE Cotacoes ADD IofValor decimal(18,2) NOT NULL DEFAULT 0;
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'Cotacoes') AND name = 'CustoServicosAplicado')
                    ALTER TABLE Cotacoes ADD CustoServicosAplicado decimal(18,2) NOT NULL DEFAULT 0;";
            await ctx.Database.ExecuteSqlRawAsync(addColumns);
        }
        catch { /* ignora erro se colunas já existirem */ }

        // Aplicar Stored Procedure se não existir
        try
        {
            var sp = @"IF OBJECT_ID('dbo.sp_GetCotacaoCompleta', 'P') IS NULL
            EXEC('CREATE PROCEDURE dbo.sp_GetCotacaoCompleta @CotacaoId INT AS BEGIN SELECT 1 END')";
            await ctx.Database.ExecuteSqlRawAsync(sp);
            // Atualiza corpo completo
            var fullSp = @"ALTER PROCEDURE dbo.sp_GetCotacaoCompleta @CotacaoId INT AS BEGIN SET NOCOUNT ON;
                SELECT c.Id, c.Numero, c.ProdutoId, c.Status, c.DtCriacao, c.DtValidade, c.PremioLiquido, c.DescontoComercial, c.PremioComercial, c.Comissao, c.PremioTotal FROM Cotacoes c WHERE c.Id=@CotacaoId;
                SELECT p.* FROM Proponentes p WHERE p.CotacaoId=@CotacaoId;
                SELECT v.* FROM Veiculos v WHERE v.CotacaoId=@CotacaoId;
                SELECT cc.* FROM CotacaoCoberturas cc WHERE cc.CotacaoId=@CotacaoId; END";
            await ctx.Database.ExecuteSqlRawAsync(fullSp);
        }
        catch { /* ignora se não conseguir criar SP aqui */ }
    }
}