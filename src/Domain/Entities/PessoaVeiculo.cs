using System;

namespace Domain.Entities;

public class Proponente
{
    public int Id { get; private set; }
    public int CotacaoId { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string CpfCnpj { get; private set; } = string.Empty;
    public Genero Genero { get; private set; }
    public string EstadoCivil { get; private set; } = string.Empty;
    public DateTime DtNascimento { get; private set; }
    public string CepResidencial { get; private set; } = string.Empty;

    // Construtor sem parâmetros para EF Core
    private Proponente() {}

    public Proponente(string nome, string cpfCnpj, Genero genero, string estadoCivil, DateTime dtNascimento, string cepResidencial)
    {
        Nome = nome; CpfCnpj = cpfCnpj; Genero = genero; EstadoCivil = estadoCivil; DtNascimento = dtNascimento; CepResidencial = cepResidencial;
    }

    public int Idade()
    {
        var hoje = DateTime.Today;
        var idade = hoje.Year - DtNascimento.Year;
        if (DtNascimento.Date > hoje.AddYears(-idade)) idade--;
        return idade;
    }

    public FaixaIdade ObterFaixaIdade()
    {
        var idade = Idade();
        return idade switch
        {
            >=18 and <=25 => FaixaIdade.Idade18_25,
            >=26 and <=35 => FaixaIdade.Idade26_35,
            >=36 and <=60 => FaixaIdade.Idade36_60,
            _ => FaixaIdade.Idade60Mais
        };
    }
}

public class Veiculo
{
    public int Id { get; private set; }
    public int CotacaoId { get; private set; }
    public string? Placa { get; private set; }
    public string? Chassi { get; private set; }
    public string CodigoFipeOuVeiculo { get; private set; } = string.Empty;
    public int AnoModelo { get; private set; }
    public int AnoFabricacao { get; private set; }
    public string CepPernoite { get; private set; } = string.Empty;
    public TipoUtilizacao TipoUtilizacao { get; private set; }
    public bool ZeroKm { get; private set; }
    public bool Blindado { get; private set; }
    public bool KitGas { get; private set; }

    // Construtor sem parâmetros para EF Core
    private Veiculo() {}

    public Veiculo(string codigoFipeOuVeiculo, int anoModelo, int anoFabricacao, string cepPernoite, TipoUtilizacao tipo, bool zeroKm)
    {
        CodigoFipeOuVeiculo = codigoFipeOuVeiculo; AnoModelo = anoModelo; AnoFabricacao = anoFabricacao; CepPernoite = cepPernoite; TipoUtilizacao = tipo; ZeroKm = zeroKm;
    }
}