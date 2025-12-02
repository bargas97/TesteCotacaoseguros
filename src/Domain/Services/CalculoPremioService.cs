using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities;

namespace Domain.Services;

public class CalculoPremioService
{
    // Pontos críticos do cálculo: multiplicação sequencial de fatores.
    // Qualquer fator zero anula prêmio da cobertura.
    public void CalcularPremios(Cotacao cotacao, Produto produto,
        IReadOnlyCollection<FatorBonus> fatoresBonus,
        IReadOnlyCollection<FatorPerfil> fatoresPerfil,
        IReadOnlyCollection<FatorRegiao> fatoresRegiao,
        IReadOnlyCollection<FatorUtilizacao> fatoresUtilizacao,
        IReadOnlyCollection<FatorFranquia> fatoresFranquia)
    {
        if (!cotacao.PodeCalcular()) throw new InvalidOperationException("Cotação não possui dados mínimos para cálculo.");
        if (cotacao.Proponente == null || cotacao.Veiculo == null) throw new InvalidOperationException("Proponente ou Veículo ausente.");
        if (cotacao.Proponente.Idade() < 18) throw new InvalidOperationException("Proponente deve possuir idade mínima de 18 anos.");

        var faixa = cotacao.Proponente.ObterFaixaIdade();
        var genero = cotacao.Proponente.Genero;
        var cepResidencial = cotacao.Proponente.CepResidencial;
        var tipoUso = cotacao.Veiculo.TipoUtilizacao;

        var fatorPerfil = fatoresPerfil.FirstOrDefault(x => x.FaixaIdade == faixa && x.Genero == genero)?.Fator ?? 1m;
        var fatorRegiao = fatoresRegiao.FirstOrDefault(x => x.ContemCep(cepResidencial))?.Fator ?? 1m;
        var fatorUtilizacao = fatoresUtilizacao.FirstOrDefault(x => x.TipoUtilizacao == tipoUso)?.Fator ?? 1m;

        // Supondo bônus vindo externamente (ex: classe 0 default)
        var fatorBonus = fatoresBonus.FirstOrDefault(x => x.ClasseBonus == 0)?.Fator ?? 1m;

        decimal premioLiquido = 0m;
        foreach (var c in cotacao.Coberturas.Where(x => x.Contratada))
        {
            var fatorFranquia = 1m;
            if (!string.IsNullOrWhiteSpace(c.FranquiaSelecionada))
            {
                fatorFranquia = fatoresFranquia.FirstOrDefault(x => x.Franquia == c.FranquiaSelecionada)?.Fator ?? 1m;
            }

            var premioCobertura = c.ImportanciaSegurada * fatorBonus * fatorPerfil * fatorRegiao * fatorUtilizacao * fatorFranquia;
            c.DefinirPremio(decimal.Round(premioCobertura, 2));
            premioLiquido += c.PremioCobertura;
        }
        cotacao.DefinirPremios(decimal.Round(premioLiquido, 2), produto);
        cotacao.MarcarCalculada();
    }
}