using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Mvc.Testing;
using Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace WebApi.Tests;

[TestClass]
public class StoredProcedureTests
{
    private WebApplicationFactory<Program> _factory = null!;

    [TestInitialize]
    public void Setup()
    {
        // Usa SQL Server local para testar SP; se indisponível o teste será marcado inconclusivo
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CotacaoDbContext>));
                    if (descriptor != null) services.Remove(descriptor);
                    var cs = Environment.GetEnvironmentVariable("COTACAO_TEST_CS") ?? "Server=localhost;Database=CotacoesSpTest;User Id=sa;Password=Your_password123;TrustServerCertificate=True;";
                    services.AddDbContext<CotacaoDbContext>(o => o.UseSqlServer(cs));
                });
            });
    }

    [TestMethod]
    public async Task Executar_sp_GetCotacaoCompleta_DeveRetornar4ResultSets()
    {
        using var scope = _factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<CotacaoDbContext>();
        try
        {
            await ctx.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            Assert.Inconclusive("SQL Server indisponível: " + ex.Message);
            return;
        }

        // Seed mínimo caso não exista cotação
        if (!ctx.Produtos.Any())
        {
            var produto = new Produto("Auto Individual", new[]{"Alta"});
            ctx.Produtos.Add(produto);
            await ctx.SaveChangesAsync();
            ctx.Coberturas.Add(new Cobertura(produto.Id, "DM", "Danos Materiais", TipoCobertura.Basica, 1000, 50000));
            await ctx.SaveChangesAsync();

            var cotacao = new Cotacao(produto.Id);
            cotacao.DefinirNumero("TEST-000001");
            var prop = new Proponente("Teste", "12345678901", Genero.M, "Solteiro", DateTime.Today.AddYears(-30), "06000000");
            var vei = new Veiculo("FIPE123", 2024, 2024, "06000000", TipoUtilizacao.Particular, false);
            cotacao.DefinirProponente(prop);
            cotacao.DefinirVeiculo(vei);
            cotacao.AdicionarCobertura(new CotacaoCobertura(ctx.Coberturas.First().Id, 2000m, true, "Alta"));
            ctx.Cotacoes.Add(cotacao);
            await ctx.SaveChangesAsync();
        }

        var cotacaoId = await ctx.Cotacoes.Select(x => x.Id).FirstAsync();

        using var conn = ctx.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "sp_GetCotacaoCompleta";
        cmd.CommandType = CommandType.StoredProcedure;
        var p = cmd.CreateParameter();
        p.ParameterName = "@CotacaoId";
        p.Value = cotacaoId;
        cmd.Parameters.Add(p);

        using var reader = await cmd.ExecuteReaderAsync();
        int resultSets = 0;
        // 1: Cabeçalho
        Assert.IsTrue(reader.HasRows, "Cabeçalho vazio");
        resultSets++;
        Assert.IsTrue(await reader.NextResultAsync(), "Não avançou para Proponente");
        Assert.IsTrue(reader.HasRows, "Proponente vazio");
        resultSets++;
        Assert.IsTrue(await reader.NextResultAsync(), "Não avançou para Veículo");
        Assert.IsTrue(reader.HasRows, "Veículo vazio");
        resultSets++;
        Assert.IsTrue(await reader.NextResultAsync(), "Não avançou para Coberturas");
        Assert.IsTrue(reader.HasRows, "Coberturas vazias");
        resultSets++;
        Assert.IsFalse(await reader.NextResultAsync(), "SP retornou mais que 4 result sets");

        Assert.AreEqual(4, resultSets, "Quantidade de result sets incorreta");
    }
}