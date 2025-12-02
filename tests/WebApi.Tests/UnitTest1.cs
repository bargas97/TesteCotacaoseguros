using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Tests;

public class CriarCotacaoRequest
{
    public int produtoId { get; set; }
    public ProponenteDto proponente { get; set; } = default!;
    public VeiculoDto veiculo { get; set; } = default!;
    public List<CoberturaInputDto> coberturas { get; set; } = new();
}
public class ProponenteDto { public string nome { get; set; } = ""; public string cpfCnpj { get; set; } = ""; public int genero { get; set; } public string estadoCivil { get; set; } = ""; public string dtNascimento { get; set; } = ""; public string cepResidencial { get; set; } = ""; }
public class VeiculoDto { public string codigoFipeOuVeiculo { get; set; } = ""; public int anoModelo { get; set; } public int anoFabricacao { get; set; } public string cepPernoite { get; set; } = ""; public int tipoUtilizacao { get; set; } public bool zeroKm { get; set; } public bool blindado { get; set; } public bool kitGas { get; set; } }
public class CoberturaInputDto { public int coberturaId { get; set; } public decimal importanciaSegurada { get; set; } public bool contratada { get; set; } public string? franquiaSelecionada { get; set; } }
public class CotacaoResponse { public int id { get; set; } public int status { get; set; } public decimal premioLiquido { get; set; } }

[TestClass]
public class CotacoesIntegrationTests
{
    private WebApplicationFactory<Program> _factory = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Substitui SQL Server por InMemory para testes
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<CotacaoDbContext>));
                    if (descriptor != null) services.Remove(descriptor);
                    services.AddDbContext<CotacaoDbContext>(o => o.UseInMemoryDatabase("CotacoesTestDb"));
                });
            });
    }

    [TestMethod]
    public async Task FluxoCompleto_Rascunho_Calcula()
    {
        var client = _factory.CreateClient();

        // Seed manual mínimo (produto + cobertura) se necessário
        using (var scope = _factory.Services.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<CotacaoDbContext>();
            if (!ctx.Produtos.Any())
            {
                var p = new Domain.Entities.Produto("Auto", new[]{"Alta"});
                p.DefinirParametros(0.10m,0.0738m,25m,0.20m);
                ctx.Produtos.Add(p);
                ctx.Coberturas.Add(new Domain.Entities.Cobertura(p.Id, "DM", "Danos Materiais", Domain.Entities.TipoCobertura.Basica, 1000, 50000));
                ctx.SaveChanges();
            }
        }

        var request = new CriarCotacaoRequest
        {
            produtoId = 1,
            // CPF válido gerado para testes (não real): 52998224725
            proponente = new ProponenteDto { nome = "Teste", cpfCnpj = "52998224725", genero=0, estadoCivil="Solteiro", dtNascimento=DateTime.Today.AddYears(-30).ToString("yyyy-MM-dd"), cepResidencial="06000000" },
            veiculo = new VeiculoDto { codigoFipeOuVeiculo="FIPE123", anoModelo=2024, anoFabricacao=2024, cepPernoite="06000000", tipoUtilizacao=1, zeroKm=false, blindado=false, kitGas=false },
            coberturas = new List<CoberturaInputDto>{ new CoberturaInputDto{ coberturaId=1, importanciaSegurada=1000m, contratada=true, franquiaSelecionada="Alta" } }
        };

        var createResp = await client.PostAsJsonAsync("/cotacoes", request);
        if (!createResp.IsSuccessStatusCode)
        {
            var body = await createResp.Content.ReadAsStringAsync();
            Assert.Fail($"Falha ao criar rascunho. Status: {createResp.StatusCode}. Corpo: {body}");
        }
        var created = await createResp.Content.ReadFromJsonAsync<CotacaoResponse>();
        created.Should().NotBeNull();
        created!.id.Should().BeGreaterThan(0);
        created.status.Should().Be(0); // Rascunho

        var calcResp = await client.PutAsync($"/cotacoes/{created.id}/calcular", null);
        if (!calcResp.IsSuccessStatusCode)
        {
            var body = await calcResp.Content.ReadAsStringAsync();
            Assert.Fail($"Falha ao calcular. Status: {calcResp.StatusCode}. Corpo: {body}");
        }

        var getResp = await client.GetAsync($"/cotacoes/{created.id}");
        getResp.EnsureSuccessStatusCode();
        var cotacao = await getResp.Content.ReadFromJsonAsync<CotacaoResponse>();
        cotacao.Should().NotBeNull();
        cotacao!.status.Should().Be(1); // Calculada
        cotacao.premioLiquido.Should().BeGreaterThan(0);
    }

    [TestMethod]
    public async Task NaoPermite_Aprovar_Rascunho()
    {
        var client = _factory.CreateClient();
        await GarantirProdutoCobertura();
        var request = new CriarCotacaoRequest
        {
            produtoId = 1,
            proponente = new ProponenteDto { nome = "Teste", cpfCnpj = "52998224725", genero=0, estadoCivil="Solteiro", dtNascimento=DateTime.Today.AddYears(-30).ToString("yyyy-MM-dd"), cepResidencial="06000000" },
            veiculo = new VeiculoDto { codigoFipeOuVeiculo="FIPE123", anoModelo=2024, anoFabricacao=2024, cepPernoite="06000000", tipoUtilizacao=1, zeroKm=false, blindado=false, kitGas=false },
            coberturas = new List<CoberturaInputDto>{ new CoberturaInputDto{ coberturaId=1, importanciaSegurada=1000m, contratada=true, franquiaSelecionada="Alta" } }
        };
        var createResp = await client.PostAsJsonAsync("/cotacoes", request);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var created = await createResp.Content.ReadFromJsonAsync<CotacaoResponse>();
        var aprovarResp = await client.PutAsync($"/cotacoes/{created!.id}/aprovar", null);
        aprovarResp.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest, await aprovarResp.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task NaoPermite_Desconto_Acima_Teto()
    {
        var client = _factory.CreateClient();
        await GarantirProdutoCobertura();
        var request = new CriarCotacaoRequest
        {
            produtoId = 1,
            proponente = new ProponenteDto { nome = "Teste", cpfCnpj = "52998224725", genero=0, estadoCivil="Solteiro", dtNascimento=DateTime.Today.AddYears(-30).ToString("yyyy-MM-dd"), cepResidencial="06000000" },
            veiculo = new VeiculoDto { codigoFipeOuVeiculo="FIPE123", anoModelo=2024, anoFabricacao=2024, cepPernoite="06000000", tipoUtilizacao=1, zeroKm=false, blindado=false, kitGas=false },
            coberturas = new List<CoberturaInputDto>{ new CoberturaInputDto{ coberturaId=1, importanciaSegurada=1000m, contratada=true, franquiaSelecionada="Alta" } }
        };
        var createResp = await client.PostAsJsonAsync("/cotacoes", request);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var created = await createResp.Content.ReadFromJsonAsync<CotacaoResponse>();
        // Calcula primeiro
        var calcResp = await client.PutAsync($"/cotacoes/{created!.id}/calcular", null);
        calcResp.IsSuccessStatusCode.Should().BeTrue();
        // tenta desconto 90% onde teto do produto é 20%
        var descontoPayload = new { percentual = 0.90m };
        var descResp = await client.PutAsJsonAsync($"/cotacoes/{created.id}/desconto-comercial", descontoPayload);
        descResp.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest, await descResp.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public async Task NaoPermite_Calcular_SemCoberturasContratadas()
    {
        var client = _factory.CreateClient();
        await GarantirProdutoCobertura();
        // envia cobertura não contratada (contratada=false) e sem outras
        var request = new CriarCotacaoRequest
        {
            produtoId = 1,
            proponente = new ProponenteDto { nome = "Teste", cpfCnpj = "52998224725", genero=0, estadoCivil="Solteiro", dtNascimento=DateTime.Today.AddYears(-30).ToString("yyyy-MM-dd"), cepResidencial="06000000" },
            veiculo = new VeiculoDto { codigoFipeOuVeiculo="FIPE123", anoModelo=2024, anoFabricacao=2024, cepPernoite="06000000", tipoUtilizacao=1, zeroKm=false, blindado=false, kitGas=false },
            coberturas = new List<CoberturaInputDto>{ new CoberturaInputDto{ coberturaId=1, importanciaSegurada=1000m, contratada=false, franquiaSelecionada="Alta" } }
        };
        var createResp = await client.PostAsJsonAsync("/cotacoes", request);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var created = await createResp.Content.ReadFromJsonAsync<CotacaoResponse>();
        var calcResp = await client.PutAsync($"/cotacoes/{created!.id}/calcular", null);
        calcResp.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest, await calcResp.Content.ReadAsStringAsync());
    }

    private async Task GarantirProdutoCobertura()
    {
        using var scope = _factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<CotacaoDbContext>();
        if (!ctx.Produtos.Any())
        {
            var p = new Domain.Entities.Produto("Auto", new[]{"Alta"});
            p.DefinirParametros(0.10m,0.0738m,25m,0.20m);
            ctx.Produtos.Add(p);
            ctx.Coberturas.Add(new Domain.Entities.Cobertura(p.Id, "DM", "Danos Materiais", Domain.Entities.TipoCobertura.Basica, 1000, 50000));
            ctx.SaveChanges();
        }
    }
}