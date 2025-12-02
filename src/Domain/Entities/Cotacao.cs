using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Entities;

public class CotacaoCobertura
{
    public int CotacaoId { get; private set; }
    public int CoberturaId { get; private set; }
    public decimal ImportanciaSegurada { get; private set; }
    public decimal PremioCobertura { get; private set; }
    public string? FranquiaSelecionada { get; private set; }
    public bool Contratada { get; private set; }

    // Construtor sem parâmetros para EF Core
    private CotacaoCobertura() {}

    public CotacaoCobertura(int coberturaId, decimal isValor, bool contratada, string? franquia = null)
    {
        CoberturaId = coberturaId; ImportanciaSegurada = isValor; Contratada = contratada; FranquiaSelecionada = franquia; PremioCobertura = 0m;
    }

    public void DefinirPremio(decimal premio) => PremioCobertura = premio;
}

public class Cotacao
{
    public int Id { get; private set; }
    public string Numero { get; private set; } = string.Empty;
    public int ProdutoId { get; private set; }
    public int? CorretorId { get; private set; }
    public StatusCotacao Status { get; private set; } = StatusCotacao.Rascunho;
    public DateTime DtCriacao { get; private set; } = DateTime.UtcNow;
    public DateTime? DtValidade { get; private set; }
    public decimal PremioLiquido { get; private set; }
    public decimal DescontoComercial { get; private set; }
    public decimal PremioComercial { get; private set; }
    public decimal Comissao { get; private set; }
    public decimal PremioTotal { get; private set; }
    public decimal IofValor { get; private set; }
    public decimal CustoServicosAplicado { get; private set; }

    private readonly List<CotacaoCobertura> _coberturas = new();
    public IReadOnlyCollection<CotacaoCobertura> Coberturas => _coberturas;

    public Proponente? Proponente { get; private set; }
    public Veiculo? Veiculo { get; private set; }

    // Construtor sem parâmetros para EF Core
    private Cotacao() {}

    public Cotacao(int produtoId)
    {
        ProdutoId = produtoId;
    }

    public void DefinirNumero(string numero)
    {
        if(string.IsNullOrWhiteSpace(numero)) throw new ArgumentException("Número da cotação inválido", nameof(numero));
        Numero = numero;
    }

    public void DefinirProponente(Proponente p) => Proponente = p;
    public void DefinirVeiculo(Veiculo v) => Veiculo = v;
    public void AdicionarCobertura(CotacaoCobertura c) => _coberturas.Add(c);

    public bool PodeCalcular() => Proponente != null && Veiculo != null && _coberturas.Any(x => x.Contratada);
    public bool PodeAprovar() => Status == StatusCotacao.Calculada;
    public bool PodeCancelar() => Status is StatusCotacao.Rascunho or StatusCotacao.Calculada;

    public void AplicarDesconto(decimal descontoPercentual, Produto produto)
    {
        if (!produto.ValidarDesconto(descontoPercentual)) throw new InvalidOperationException("Desconto comercial acima do teto do produto.");
        var valorDesconto = PremioLiquido * descontoPercentual;
        DescontoComercial = valorDesconto;
        RecalcularTotais(produto);
    }

    public void MarcarCalculada() => Status = StatusCotacao.Calculada;
    public void MarcarAprovada()
    {
        if(!PodeAprovar()) throw new InvalidOperationException("Cotação não está em status Calculada.");
        Status = StatusCotacao.Aprovada;
        DtValidade = DateTime.UtcNow.AddDays(15);
    }
    public void MarcarCancelada()
    {
        if(!PodeCancelar()) throw new InvalidOperationException("Status não permite cancelamento.");
        Status = StatusCotacao.Cancelada;
    }

    public void DefinirPremios(decimal premioLiquido, Produto produto)
    {
        PremioLiquido = premioLiquido;
        RecalcularTotais(produto);
    }

    private void RecalcularTotais(Produto produto)
    {
        PremioComercial = PremioLiquido - DescontoComercial;
        Comissao = PremioComercial * produto.PercentualComissao;
        IofValor = PremioComercial * produto.IofPercentual;
        CustoServicosAplicado = produto.CustoServicos;
        PremioTotal = PremioComercial + CustoServicosAplicado + IofValor;
    }
}