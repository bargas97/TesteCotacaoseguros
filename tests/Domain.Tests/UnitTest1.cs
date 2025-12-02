using Domain.Entities;
using Domain.Services;
using FluentAssertions;

namespace Domain.Tests;

public class CalculoPremioServiceTests
{
    [Fact]
    public void CalcularPremios_DeveCalcularPremioCoberturaEAtualizarTotais()
    {
        // Arrange
        var produto = new Produto("Auto", new[]{"Alta"});
        produto.DefinirParametros(0.10m, 0.0738m, 25.00m, 0.20m);
        var cotacao = new Cotacao(produto.Id);
        var proponente = new Proponente("João", "12345678901", Genero.M, "Solteiro", DateTime.Today.AddYears(-30), "06000000");
        cotacao.DefinirProponente(proponente);
        var veiculo = new Veiculo("FIPE123", 2024, 2024, "06000000", TipoUtilizacao.Profissional, false);
        cotacao.DefinirVeiculo(veiculo);
        var cobertura = new CotacaoCobertura(1, 1000m, true, "Alta");
        cotacao.AdicionarCobertura(cobertura);

        var svc = new CalculoPremioService();
        var fatoresBonus = new[]{ new FatorBonus(0, 1m)};
        var fatoresPerfil = new[]{ new FatorPerfil(FaixaIdade.Idade26_35, Genero.M, 1m)};
        var fatoresRegiao = new[]{ new FatorRegiao("06000000","09999999",1.10m)};
        var fatoresUtilizacao = new[]{ new FatorUtilizacao(TipoUtilizacao.Profissional,1.20m)};
        var fatoresFranquia = new[]{ new FatorFranquia("Alta",0.85m)};

        // Act
        svc.CalcularPremios(cotacao, produto, fatoresBonus, fatoresPerfil, fatoresRegiao, fatoresUtilizacao, fatoresFranquia);

        // Assert
        cobertura.PremioCobertura.Should().Be(1122.00m); // 1000 * 1 * 1 * 1.10 * 1.20 * 0.85
        cotacao.PremioLiquido.Should().Be(1122.00m);
        cotacao.PremioComercial.Should().Be(1122.00m);
        cotacao.Comissao.Should().BeApproximately(112.20m, 0.01m);
        var iofEsperado = 1122.00m * 0.0738m; // 82.8036
        cotacao.PremioTotal.Should().BeApproximately(1122.00m + 25.00m + iofEsperado, 0.01m);
        cotacao.Status.Should().Be(StatusCotacao.Calculada);
    }

    [Fact]
    public void AplicarDesconto_AcimaDoLimite_DeveLancarExcecao()
    {
        var produto = new Produto("Auto");
        produto.DefinirParametros(0.10m, 0.0738m, 25.00m, 0.20m);
        var cotacao = new Cotacao(produto.Id);
        var proponente = new Proponente("Maria", "12345678901", Genero.F, "Solteiro", DateTime.Today.AddYears(-40), "06000000");
        cotacao.DefinirProponente(proponente);
        var veiculo = new Veiculo("FIPE", 2024, 2024, "06000000", TipoUtilizacao.Particular, false);
        cotacao.DefinirVeiculo(veiculo);
        cotacao.AdicionarCobertura(new CotacaoCobertura(1, 500m, true));
        var svc = new CalculoPremioService();
        svc.CalcularPremios(cotacao, produto, new[]{new FatorBonus(0,1m)}, new[]{new FatorPerfil(FaixaIdade.Idade36_60, Genero.F,1m)}, new[]{new FatorRegiao("06000000","09999999",1m)}, new[]{new FatorUtilizacao(TipoUtilizacao.Particular,1m)}, new[]{new FatorFranquia("Alta",1m)});

        Action act = () => cotacao.AplicarDesconto(0.25m, produto); // 25% > 20% limite
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Aprovar_SemCalculo_DeveFalhar()
    {
        var produto = new Produto("Auto");
        var cotacao = new Cotacao(produto.Id);
        Action act = () => cotacao.MarcarAprovada();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CalcularPremios_SemCoberturasContratadas_DeveFalhar()
    {
        var produto = new Produto("Auto");
        produto.DefinirParametros(0.10m, 0.0738m, 25.00m, 0.20m);
        var cotacao = new Cotacao(produto.Id);
        var proponente = new Proponente("João", "12345678901", Genero.M, "Solteiro", DateTime.Today.AddYears(-30), "06000000");
        cotacao.DefinirProponente(proponente);
        var veiculo = new Veiculo("FIPE123", 2024, 2024, "06000000", TipoUtilizacao.Particular, false);
        cotacao.DefinirVeiculo(veiculo);
        var svc = new CalculoPremioService();
        Action act = () => svc.CalcularPremios(cotacao, produto, Array.Empty<FatorBonus>(), Array.Empty<FatorPerfil>(), Array.Empty<FatorRegiao>(), Array.Empty<FatorUtilizacao>(), Array.Empty<FatorFranquia>());
        act.Should().Throw<InvalidOperationException>().WithMessage("*mínimos*");
    }

    [Fact]
    public void CalcularPremios_IdadeMenorQue18_DeveFalhar()
    {
        var produto = new Produto("Auto");
        var cotacao = new Cotacao(produto.Id);
        var proponente = new Proponente("João", "12345678901", Genero.M, "Solteiro", DateTime.Today.AddYears(-17), "06000000");
        cotacao.DefinirProponente(proponente);
        var veiculo = new Veiculo("FIPE123", 2024, 2024, "06000000", TipoUtilizacao.Particular, false);
        cotacao.DefinirVeiculo(veiculo);
        var cobertura = new CotacaoCobertura(1, 1000m, true);
        cotacao.AdicionarCobertura(cobertura);
        var svc = new CalculoPremioService();
        Action act = () => svc.CalcularPremios(cotacao, produto, new[]{new FatorBonus(0,1m)}, new[]{new FatorPerfil(FaixaIdade.Idade18_25, Genero.M,1m)}, new[]{new FatorRegiao("06000000","09999999",1m)}, new[]{new FatorUtilizacao(TipoUtilizacao.Particular,1m)}, new[]{new FatorFranquia("Alta",1m)});
        act.Should().Throw<InvalidOperationException>().WithMessage("*idade mínima*");
    }
}