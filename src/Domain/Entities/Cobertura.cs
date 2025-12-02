namespace Domain.Entities;

public class Cobertura
{
    public int Id { get; private set; }
    public int ProdutoId { get; private set; }
    public string Codigo { get; private set; } = string.Empty; // Ex.: DM, DC, RCF-M
    public string Descricao { get; private set; } = string.Empty;
    public TipoCobertura Tipo { get; private set; }
    public decimal? ValorMinimo { get; private set; }
    public decimal? ValorMaximo { get; private set; }

    // Construtor sem parâmetros para EF Core
    private Cobertura() {}

    public Cobertura(int produtoId, string codigo, string descricao, TipoCobertura tipo, decimal? minimo = null, decimal? maximo = null)
    {
        ProdutoId = produtoId;
        Codigo = codigo;
        Descricao = descricao;
        Tipo = tipo;
        ValorMinimo = minimo;
        ValorMaximo = maximo;
    }

    public bool ValorDentroLimite(decimal valor)
    {
        if (ValorMinimo.HasValue && valor < ValorMinimo.Value) return false;
        if (ValorMaximo.HasValue && valor > ValorMaximo.Value) return false;
        return true;
    }
}