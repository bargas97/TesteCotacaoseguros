using System.Collections.Generic;

namespace Domain.Entities;

public class Produto
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public bool Ativo { get; private set; } = true;
    public decimal PercentualComissao { get; private set; } = 0.10m; // default 10%
    public decimal IofPercentual { get; private set; } = 0.0738m; // 7.38%
    public decimal CustoServicos { get; private set; } = 25.00m;
    public decimal DescontoComercialMaxPercentual { get; private set; } = 0.20m; // 20%

    private readonly List<string> _franquiasDisponiveis = new();
    public IReadOnlyCollection<string> FranquiasDisponiveis => _franquiasDisponiveis;

    private readonly List<Cobertura> _coberturasDisponiveis = new();
    public IReadOnlyCollection<Cobertura> CoberturasDisponiveis => _coberturasDisponiveis;

    // Construtor sem parâmetros para EF Core
    private Produto() {}

    public Produto(string nome, IEnumerable<string>? franquias = null)
    {
        Nome = nome;
        if (franquias != null)
            _franquiasDisponiveis.AddRange(franquias);
    }

    public void DefinirParametros(decimal percentualComissao, decimal iof, decimal custoServicos, decimal descontoMax)
    {
        PercentualComissao = percentualComissao;
        IofPercentual = iof;
        CustoServicos = custoServicos;
        DescontoComercialMaxPercentual = descontoMax;
    }

    public bool ValidarDesconto(decimal descontoPercentual) => descontoPercentual <= DescontoComercialMaxPercentual;

    public void AdicionarCobertura(Cobertura cobertura)
    {
        _coberturasDisponiveis.Add(cobertura);
    }
}