using System;

namespace Domain.Entities;

public class FatorBonus
{
    public int Id { get; private set; }
    public int ClasseBonus { get; private set; } // 0-10
    public decimal Fator { get; private set; }
    private FatorBonus() { }
    public FatorBonus(int classeBonus, decimal fator){ ClasseBonus = classeBonus; Fator = fator; }
}

public class FatorPerfil
{
    public int Id { get; private set; }
    public FaixaIdade FaixaIdade { get; private set; }
    public Genero Genero { get; private set; }
    public decimal Fator { get; private set; }
    private FatorPerfil() { }
    public FatorPerfil(FaixaIdade faixa, Genero genero, decimal fator){ FaixaIdade = faixa; Genero = genero; Fator = fator; }
}

public class FatorRegiao
{
    public int Id { get; private set; }
    public string CepInicio { get; private set; }
    public string CepFim { get; private set; }
    public decimal Fator { get; private set; }
    private FatorRegiao() { }
    public FatorRegiao(string inicio, string fim, decimal fator){ CepInicio = inicio; CepFim = fim; Fator = fator; }
    public bool ContemCep(string cep) => string.Compare(cep, CepInicio, StringComparison.Ordinal) >= 0 && string.Compare(cep, CepFim, StringComparison.Ordinal) <= 0;
}

public class FatorUtilizacao
{
    public int Id { get; private set; }
    public TipoUtilizacao TipoUtilizacao { get; private set; }
    public decimal Fator { get; private set; }
    private FatorUtilizacao() { }
    public FatorUtilizacao(TipoUtilizacao tipo, decimal fator){ TipoUtilizacao = tipo; Fator = fator; }
}

public class FatorFranquia
{
    public int Id { get; private set; }
    public string Franquia { get; private set; } = string.Empty;
    public decimal Fator { get; private set; }
    private FatorFranquia() { }
    public FatorFranquia(string franquia, decimal fator){ Franquia = franquia; Fator = fator; }
}